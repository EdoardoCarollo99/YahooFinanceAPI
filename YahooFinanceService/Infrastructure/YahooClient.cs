using Microsoft.Extensions.Options;
using System.Text.Json;
using YahooFinanceService.Configuration;
using YahooFinanceService.Exceptions;

namespace YahooFinanceService.Infrastructure;

/// <summary>
/// Implementation of Yahoo Finance HTTP client with authentication and error handling.
/// </summary>
public sealed class YahooClient(
    IHttpClientFactory httpClientFactory,
    IYahooSessionManager sessionManager,
    IOptions<YahooFinanceOptions> options) : IYahooClient
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <inheritdoc/>
    public async Task<string> GetAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        var session = await sessionManager.GetSessionDataAsync(cancellationToken);
        
        for (int attempt = 0; attempt <= options.Value.MaxRetryAttempts; attempt++)
        {
            try
            {
                return await ExecuteRequestAsync(requestUri, session, cancellationToken);
            }
            catch (RateLimitException)
            {
                throw;
            }
            catch (YahooFinanceException) when (attempt < options.Value.MaxRetryAttempts)
            {
                await Task.Delay(options.Value.RetryDelay, cancellationToken);
                session = await sessionManager.RefreshSessionAsync(cancellationToken);
            }
        }

        throw new YahooFinanceException($"Request to {requestUri} failed after {options.Value.MaxRetryAttempts} attempts.");
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        var content = await GetAsync(requestUri, cancellationToken);
        
        try
        {
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new YahooFinanceException($"Failed to deserialize response from {requestUri}.", ex);
        }
    }

    private async Task<string> ExecuteRequestAsync(
        string requestUri, 
        YahooSessionData session,
        CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();
        client.Timeout = options.Value.RequestTimeout;

        // CRITICO: Aggiungi il crumb come parametro query string
        var separator = requestUri.Contains('?') ? "&" : "?";
        var urlWithCrumb = $"{requestUri}{separator}crumb={Uri.EscapeDataString(session.Crumb)}";

        var request = new HttpRequestMessage(HttpMethod.Get, urlWithCrumb);
        request.Headers.Add("User-Agent", options.Value.UserAgent);
        request.Headers.Add("Cookie", $"{session.CookieName}={session.CookieValue}");

        var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new SessionException(
                $"Unauthorized: Cookie/Crumb non validi o sessione scaduta. {errorContent}");
        }

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            var retryAfter = response.Headers.RetryAfter?.Date;
            throw new RateLimitException("Rate limit exceeded.", retryAfter);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new YahooFinanceException(
                "Resource not found. The symbol or endpoint may be invalid.", 
                404);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new YahooFinanceException(
                $"Request failed with status {response.StatusCode}: {errorContent}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
