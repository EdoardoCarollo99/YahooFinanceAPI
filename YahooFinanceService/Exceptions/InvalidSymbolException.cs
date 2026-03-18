namespace YahooFinanceService.Exceptions;

/// <summary>
/// Exception thrown when an invalid or unknown stock symbol is requested.
/// </summary>
public sealed class InvalidSymbolException : YahooFinanceException
{
    /// <summary>
    /// Gets the invalid symbol that caused the exception.
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSymbolException"/> class.
    /// </summary>
    /// <param name="symbol">The invalid symbol.</param>
    public InvalidSymbolException(string symbol) 
        : base($"Symbol '{symbol}' is invalid or not found on Yahoo Finance.")
    {
        Symbol = symbol;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSymbolException"/> class.
    /// </summary>
    /// <param name="symbol">The invalid symbol.</param>
    /// <param name="message">Custom error message.</param>
    public InvalidSymbolException(string symbol, string message) : base(message)
    {
        Symbol = symbol;
    }
}
