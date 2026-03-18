namespace YahooFinanceService.Domain;

/// <summary>
/// Represents a stock quote with current market data.
/// </summary>
public sealed record Quote
{
    /// <summary>
    /// Gets the stock symbol.
    /// </summary>
    public required string Symbol { get; init; }

    /// <summary>
    /// Gets the regular market price.
    /// </summary>
    public decimal? RegularMarketPrice { get; init; }

    /// <summary>
    /// Gets the regular market time.
    /// </summary>
    public DateTime? RegularMarketTime { get; init; }

    /// <summary>
    /// Gets the regular market change.
    /// </summary>
    public decimal? RegularMarketChange { get; init; }

    /// <summary>
    /// Gets the regular market change percent.
    /// </summary>
    public decimal? RegularMarketChangePercent { get; init; }

    /// <summary>
    /// Gets the regular market open price.
    /// </summary>
    public decimal? RegularMarketOpen { get; init; }

    /// <summary>
    /// Gets the regular market day high.
    /// </summary>
    public decimal? RegularMarketDayHigh { get; init; }

    /// <summary>
    /// Gets the regular market day low.
    /// </summary>
    public decimal? RegularMarketDayLow { get; init; }

    /// <summary>
    /// Gets the regular market volume.
    /// </summary>
    public long? RegularMarketVolume { get; init; }

    /// <summary>
    /// Gets the previous close price.
    /// </summary>
    public decimal? RegularMarketPreviousClose { get; init; }

    /// <summary>
    /// Gets the bid price.
    /// </summary>
    public decimal? Bid { get; init; }

    /// <summary>
    /// Gets the ask price.
    /// </summary>
    public decimal? Ask { get; init; }

    /// <summary>
    /// Gets the bid size.
    /// </summary>
    public long? BidSize { get; init; }

    /// <summary>
    /// Gets the ask size.
    /// </summary>
    public long? AskSize { get; init; }

    /// <summary>
    /// Gets the market cap.
    /// </summary>
    public long? MarketCap { get; init; }

    /// <summary>
    /// Gets the 52-week high.
    /// </summary>
    public decimal? FiftyTwoWeekHigh { get; init; }

    /// <summary>
    /// Gets the 52-week low.
    /// </summary>
    public decimal? FiftyTwoWeekLow { get; init; }

    /// <summary>
    /// Gets the 50-day average.
    /// </summary>
    public decimal? FiftyDayAverage { get; init; }

    /// <summary>
    /// Gets the 200-day average.
    /// </summary>
    public decimal? TwoHundredDayAverage { get; init; }

    /// <summary>
    /// Gets the trailing P/E ratio.
    /// </summary>
    public decimal? TrailingPE { get; init; }

    /// <summary>
    /// Gets the forward P/E ratio.
    /// </summary>
    public decimal? ForwardPE { get; init; }

    /// <summary>
    /// Gets the trailing annual dividend rate.
    /// </summary>
    public decimal? TrailingAnnualDividendRate { get; init; }

    /// <summary>
    /// Gets the trailing annual dividend yield.
    /// </summary>
    public decimal? TrailingAnnualDividendYield { get; init; }

    /// <summary>
    /// Gets the currency.
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    /// Gets the exchange name.
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
}
