namespace YahooFinanceService.Domain;

/// <summary>
/// Represents a time period for historical data queries.
/// </summary>
public enum Period
{
    /// <summary>
    /// Daily period.
    /// </summary>
    Daily,

    /// <summary>
    /// Weekly period.
    /// </summary>
    Weekly,

    /// <summary>
    /// Monthly period.
    /// </summary>
    Monthly
}

/// <summary>
/// Extension methods for <see cref="Period"/>.
/// </summary>
public static class PeriodExtensions
{
    /// <summary>
    /// Converts the period to Yahoo Finance API parameter value.
    /// </summary>
    public static string ToApiValue(this Period period) => period switch
    {
        Period.Daily => "1d",
        Period.Weekly => "1wk",
        Period.Monthly => "1mo",
        _ => throw new ArgumentOutOfRangeException(nameof(period))
    };
}
