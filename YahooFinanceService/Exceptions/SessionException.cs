namespace YahooFinanceService.Exceptions;

/// <summary>
/// Exception thrown when Yahoo Finance session initialization fails.
/// </summary>
public sealed class SessionException : YahooFinanceException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SessionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public SessionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public SessionException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
