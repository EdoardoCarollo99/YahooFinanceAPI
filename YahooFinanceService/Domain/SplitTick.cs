namespace YahooFinanceService.Domain;

/// <summary>
/// Represents a stock split event.
/// </summary>
public sealed record SplitTick
{
    /// <summary>
    /// Gets the date when the split occurred.
    /// </summary>
    public required DateTime DateTime { get; init; }

    /// <summary>
    /// Gets the number of shares before the split.
    /// </summary>
    public required decimal BeforeSplit { get; init; }

    /// <summary>
    /// Gets the number of shares after the split.
    /// </summary>
    public required decimal AfterSplit { get; init; }

    /// <summary>
    /// Gets the split ratio as a formatted string (e.g., "2:1").
    /// </summary>
    public string SplitRatio => $"{AfterSplit}:{BeforeSplit}";
}
