namespace YahooFinanceService.Domain;

/// <summary>
/// Represents a dividend payment record.
/// </summary>
public sealed record DividendTick
{
    /// <summary>
    /// Gets the ex-dividend date.
    /// </summary>
    public required DateTime DateTime { get; init; }

    /// <summary>
    /// Gets the dividend amount per share.
    /// </summary>
    public required decimal Dividend { get; init; }
}
