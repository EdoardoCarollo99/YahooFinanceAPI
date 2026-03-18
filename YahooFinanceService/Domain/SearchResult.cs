namespace YahooFinanceService.Domain;

/// <summary>
/// Represents a search result for a stock symbol.
/// </summary>
public sealed record SearchResult
{
    /// <summary>
    /// Gets the stock symbol.
    /// </summary>
    public required string Symbol { get; init; }

    /// <summary>
    /// Gets the exchange code.
    /// </summary>
    public string? Exchange { get; init; }

    /// <summary>
    /// Gets the short name.
    /// </summary>
    public string? ShortName { get; init; }

    /// <summary>
    /// Gets the long name.
    /// </summary>
    public string? LongName { get; init; }

    /// <summary>
    /// Gets the security type (e.g., "EQUITY", "ETF").
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Gets the sector.
    /// </summary>
    public string? Sector { get; init; }

    /// <summary>
    /// Gets the industry.
    /// </summary>
    public string? Industry { get; init; }

    /// <summary>
    /// Gets the search relevance score.
    /// </summary>
    public double? Score { get; init; }
}
