namespace YahooFinanceService.Exceptions;

/// <summary>
/// Exception thrown when Yahoo Finance API rate limit is exceeded (HTTP 429).
/// </summary>
public sealed class RateLimitException : YahooFinanceException
{
    /// <summary>
    /// Gets the time when the rate limit will reset, if available.
    /// </summary>
    public DateTimeOffset? RetryAfter { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="retryAfter">The time when the rate limit will reset.</param>
    public RateLimitException(string message, DateTimeOffset? retryAfter = null) 
        : base(message, 429)
    {
        RetryAfter = retryAfter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitException"/> class with default message.
    /// </summary>
    public RateLimitException() 
        : base("Yahoo Finance API rate limit exceeded. Please wait before making additional requests.", 429)
    {
    }
}
