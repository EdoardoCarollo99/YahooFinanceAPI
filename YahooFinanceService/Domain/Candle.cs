namespace YahooFinanceService.Domain;

/// <summary>
/// Represents OHLCV (Open, High, Low, Close, Volume) historical price data.
/// </summary>
public sealed record Candle
{
    /// <summary>
    /// Gets the timestamp for this candle.
    /// </summary>
    public required DateTime DateTime { get; init; }

    /// <summary>
    /// Gets the opening price.
    /// </summary>
    public required decimal Open { get; init; }

    /// <summary>
    /// Gets the highest price during the period.
    /// </summary>
    public required decimal High { get; init; }

    /// <summary>
    /// Gets the lowest price during the period.
    /// </summary>
    public required decimal Low { get; init; }

    /// <summary>
    /// Gets the closing price.
    /// </summary>
    public required decimal Close { get; init; }

    /// <summary>
    /// Gets the trading volume.
    /// </summary>
    public required long Volume { get; init; }

    /// <summary>
    /// Gets the adjusted closing price (adjusted for splits and dividends).
    /// </summary>
    public required decimal AdjustedClose { get; init; }
}
