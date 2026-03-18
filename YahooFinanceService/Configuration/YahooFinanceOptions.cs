namespace YahooFinanceService.Configuration;

/// <summary>
/// Configuration options for Yahoo Finance API.
/// </summary>
public sealed record YahooFinanceOptions
{
    /// <summary>
    /// Gets the base URL for Yahoo Finance API.
    /// </summary>
    public required string BaseUrl { get; init; }

    /// <summary>
    /// Gets the URL for fetching the initial cookie.
    /// </summary>
    public required string CookieUrl { get; init; }

    /// <summary>
    /// Gets the URL for fetching the crumb token.
    /// </summary>
    public required string CrumbUrl { get; init; }

    /// <summary>
    /// Gets the user agent string used for HTTP requests.
    /// </summary>
    public required string UserAgent { get; init; }

    /// <summary>
    /// Gets the timeout for HTTP requests.
    /// </summary>
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets the maximum number of retry attempts for failed requests.
    /// </summary>
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>
    /// Gets the delay between retry attempts.
    /// </summary>
    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Creates default Yahoo Finance options.
    /// </summary>
    public static YahooFinanceOptions Default => new()
    {
        BaseUrl = "https://query2.finance.yahoo.com",
        CookieUrl = "https://guce.yahoo.com/v1/consentRecord?consentTypes=iab%2CiabCCPA%2Cgpp%2CgppSid",
        CrumbUrl = "https://query2.finance.yahoo.com/v1/test/getcrumb",
        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36",
        RequestTimeout = TimeSpan.FromSeconds(30),
        MaxRetryAttempts = 3,
        RetryDelay = TimeSpan.FromSeconds(1)
    };
}
