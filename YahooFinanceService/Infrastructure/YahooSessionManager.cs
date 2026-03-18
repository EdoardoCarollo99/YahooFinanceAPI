using Microsoft.Extensions.Options;
using System.Net;
using YahooFinanceService.Configuration;
using YahooFinanceService.Exceptions;

namespace YahooFinanceService.Infrastructure;

/// <summary>
/// Thread-safe implementation of Yahoo Finance session management.
/// </summary>
public sealed class YahooSessionManager(
    IHttpClientFactory httpClientFactory,
    IOptions<YahooFinanceOptions> options) : IYahooSessionManager
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly TimeSpan _sessionMaxAge = TimeSpan.FromHours(1);
    private YahooSessionData? _cachedSession;

    /// <inheritdoc/>
    public async Task<YahooSessionData> GetSessionDataAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedSession?.IsValid(_sessionMaxAge) == true)
        {
            return _cachedSession;
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_cachedSession?.IsValid(_sessionMaxAge) == true)
            {
                return _cachedSession;
            }

            _cachedSession = await InitializeSessionAsync(cancellationToken);
            return _cachedSession;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<YahooSessionData> RefreshSessionAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _cachedSession = await InitializeSessionAsync(cancellationToken);
            return _cachedSession;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<YahooSessionData> InitializeSessionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var (cookieName, cookieValue) = await FetchCookieAsync(cancellationToken);
            var crumb = await FetchCrumbAsync(cookieName, cookieValue, cancellationToken);

            return new YahooSessionData
            {
                Crumb = crumb,
                CookieValue = cookieValue,
                CookieName = cookieName,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex) when (ex is not SessionException)
        {
            throw new SessionException("Failed to initialize Yahoo Finance session.", ex);
        }
    }

    private async Task<(string Name, string Value)> FetchCookieAsync(CancellationToken cancellationToken)
    {
        // Usiamo l'endpoint che forza il set dei cookie di tracciamento/sessione
        const string url = "https://fc.yahoo.com/";

        var handler = new HttpClientHandler
        {
            // Importante: Yahoo usa redirect multipli (302) per il consenso
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.All,
            CookieContainer = new CookieContainer() // Gestisce automaticamente i cookie tra i redirect
        };

        using var client = new HttpClient(handler);
        client.Timeout = options.Value.RequestTimeout;

        try
        {
            // Header minimi per non farsi chiudere la connessione SSL
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "it-IT,it;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");

            // Eseguiamo la chiamata
            var response = await client.GetAsync(url, cancellationToken);

            // Se non troviamo il cookie negli header della risposta finale, 
            // lo cerchiamo nel CookieContainer (dove finiscono i cookie dei redirect intermedi)
            var uri = new Uri("https://yahoo.com");
            var allCookies = handler.CookieContainer.GetCookies(uri).Cast<Cookie>();

            var target = allCookies.FirstOrDefault(c =>
                c.Name.Equals("A3", StringComparison.OrdinalIgnoreCase) ||
                c.Name.Equals("B", StringComparison.OrdinalIgnoreCase));

            if (target == null)
            {
                // Debug: vediamo cosa abbiamo ricevuto se fallisce
                var debugCookies = string.Join(", ", allCookies.Select(c => c.Name));
                throw new SessionException($"Cookie A3/B non trovato. Ricevuti solo: {debugCookies}");
            }

            return (target.Name, target.Value);
        }
        catch (Exception ex) when (ex is not SessionException)
        {
            throw new SessionException($"Errore critico durante il recupero dei cookie: {ex.Message}", ex);
        }
    }

    private async Task<string> FetchCrumbAsync(
        string cookieName,
        string cookieValue,
        CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();
        client.Timeout = options.Value.RequestTimeout;

        var request = new HttpRequestMessage(HttpMethod.Get, options.Value.CrumbUrl);
        request.Headers.Add("User-Agent", options.Value.UserAgent);
        request.Headers.Add("Cookie", $"{cookieName}={cookieValue}");

        var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new SessionException(
                $"Unauthorized quando si richiede il crumb. Il cookie '{cookieName}' potrebbe non essere valido o l'IP è bloccato.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new SessionException(
                $"Failed to fetch crumb from {options.Value.CrumbUrl}. Status: {response.StatusCode}");
        }

        var crumb = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(crumb))
        {
            throw new SessionException("Empty crumb received from Yahoo Finance.");
        }

        return crumb.Trim();
    }

    private static (string? Name, string? Value) ParseCookie(string cookieHeader)
    {
        var parts = cookieHeader.Split(';')[0].Split('=', 2);
        return parts.Length == 2
            ? (parts[0].Trim(), parts[1].Trim())
            : (null, null);
    }
}
