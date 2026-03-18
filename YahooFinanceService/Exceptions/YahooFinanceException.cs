namespace YahooFinanceService.Exceptions;

/// <summary>
/// Base exception for all Yahoo Finance API errors.
/// </summary>
public class YahooFinanceException : Exception
{
    /// <summary>
    /// Gets the HTTP status code associated with this exception, if applicable.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooFinanceException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public YahooFinanceException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooFinanceException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public YahooFinanceException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooFinanceException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    public YahooFinanceException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooFinanceException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public YahooFinanceException(string message, int statusCode, Exception innerException) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
