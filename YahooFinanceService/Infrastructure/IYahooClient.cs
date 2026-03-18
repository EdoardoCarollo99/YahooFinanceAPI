namespace YahooFinanceService.Infrastructure;

/// <summary>
/// HTTP client for making authenticated requests to Yahoo Finance API.
/// </summary>
public interface IYahooClient
{
    /// <summary>
    /// Sends an authenticated GET request to Yahoo Finance API.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response content as a string.</returns>
    /// <exception cref="Exceptions.RateLimitException">Thrown when rate limit is exceeded.</exception>
    /// <exception cref="Exceptions.YahooFinanceException">Thrown when the request fails.</exception>
    Task<string> GetAsync(string requestUri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an authenticated GET request and deserializes the JSON response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default);
}
