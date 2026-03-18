# Domain Models

All data models returned by `IYahooFinanceService`. Every model is a C# `sealed record` — immutable, value-equality, and non-nullable by default where data is always present.

---

## `Candle`

Namespace: `YahooFinanceService.Domain`

Represents a single OHLCV price bar (open, high, low, close, volume) for a given time period.

```csharp
public sealed record Candle
{
    public required DateTime DateTime       { get; init; }
    public required decimal  Open           { get; init; }
    public required decimal  High           { get; init; }
    public required decimal  Low            { get; init; }
    public required decimal  Close          { get; init; }
    public required long     Volume         { get; init; }
    public required decimal  AdjustedClose  { get; init; }
}
```

| Property | Type | Description |
|---|---|---|
| `DateTime` | `DateTime` | Bar timestamp (UTC) |
| `Open` | `decimal` | Opening price |
| `High` | `decimal` | Highest price during the period |
| `Low` | `decimal` | Lowest price during the period |
| `Close` | `decimal` | Closing price |
| `Volume` | `long` | Number of shares traded |
| `AdjustedClose` | `decimal` | Close price adjusted for splits and dividends |

> Bars with any missing OHLCV value are **skipped** during parsing.

---

## `Quote`

Namespace: `YahooFinanceService.Domain`

Represents a real-time market snapshot for a symbol. Contains over 40 fields covering price, volume, fundamentals, and corporate actions.

```csharp
public sealed record Quote
{
    // ── Identity ─────────────────────────────────────────────
    public required string   Symbol     { get; init; }
    public          string?  Currency   { get; init; }
    public          string?  Exchange   { get; init; }
    public          string?  ShortName  { get; init; }
    public          string?  LongName   { get; init; }

    // ── Regular Session ──────────────────────────────────────
    public decimal?  RegularMarketPrice          { get; init; }
    public DateTime? RegularMarketTime           { get; init; }
    public decimal?  RegularMarketChange         { get; init; }
    public decimal?  RegularMarketChangePercent  { get; init; }
    public decimal?  RegularMarketOpen           { get; init; }
    public decimal?  RegularMarketDayHigh        { get; init; }
    public decimal?  RegularMarketDayLow         { get; init; }
    public decimal?  RegularMarketPreviousClose  { get; init; }
    public long?     RegularMarketVolume         { get; init; }

    // ── Pre-Market ───────────────────────────────────────────
    public decimal?  PreMarketPrice          { get; init; }
    public decimal?  PreMarketChange         { get; init; }
    public decimal?  PreMarketChangePercent  { get; init; }

    // ── Post-Market ──────────────────────────────────────────
    public decimal?  PostMarketPrice          { get; init; }
    public decimal?  PostMarketChange         { get; init; }
    public decimal?  PostMarketChangePercent  { get; init; }

    // ── Bid / Ask ────────────────────────────────────────────
    public decimal?  Bid      { get; init; }
    public decimal?  Ask      { get; init; }
    public long?     BidSize  { get; init; }
    public long?     AskSize  { get; init; }

    // ── Market Cap ───────────────────────────────────────────
    public long? MarketCap { get; init; }

    // ── 52-Week Range ────────────────────────────────────────
    public decimal? FiftyTwoWeekHigh              { get; init; }
    public decimal? FiftyTwoWeekLow               { get; init; }
    public decimal? FiftyTwoWeekHighChangePercent { get; init; }
    public decimal? FiftyTwoWeekLowChangePercent  { get; init; }

    // ── Moving Averages ──────────────────────────────────────
    public decimal? FiftyDayAverage         { get; init; }
    public decimal? TwoHundredDayAverage    { get; init; }

    // ── Valuation ────────────────────────────────────────────
    public decimal? TrailingPE  { get; init; }
    public decimal? ForwardPE   { get; init; }
    public decimal? PriceToBook { get; init; }

    // ── Earnings ─────────────────────────────────────────────
    public decimal?  EpsTrailingTwelveMonths  { get; init; }
    public decimal?  EpsForward               { get; init; }
    public DateTime? EarningsTimestamp        { get; init; }

    // ── Volume ───────────────────────────────────────────────
    public long? AverageDailyVolume3Month { get; init; }
    public long? AverageDailyVolume10Day  { get; init; }

    // ── Dividends ────────────────────────────────────────────
    public decimal?  TrailingAnnualDividendRate   { get; init; }
    public decimal?  TrailingAnnualDividendYield  { get; init; }
    public DateTime? DividendDate                 { get; init; }

    // ── Shares ───────────────────────────────────────────────
    public long? SharesOutstanding { get; init; }
    public long? FloatShares       { get; init; }
}
```

> Most fields are nullable (`?`) because Yahoo Finance does not always return every field for every instrument type (equities, ETFs, crypto, indices, etc.).

---

## `DividendTick`

Namespace: `YahooFinanceService.Domain`

Represents a single dividend payment event.

```csharp
public sealed record DividendTick
{
    public required DateTime DateTime  { get; init; }
    public required decimal  Dividend  { get; init; }
}
```

| Property | Type | Description |
|---|---|---|
| `DateTime` | `DateTime` | Ex-dividend date (UTC) |
| `Dividend` | `decimal` | Dividend amount per share |

---

## `SplitTick`

Namespace: `YahooFinanceService.Domain`

Represents a stock split event.

```csharp
public sealed record SplitTick
{
    public required DateTime DateTime     { get; init; }
    public required decimal  BeforeSplit  { get; init; }
    public required decimal  AfterSplit   { get; init; }

    /// <summary>Formatted split ratio, e.g. "4:1" or "3:2".</summary>
    public string SplitRatio => $"{AfterSplit}:{BeforeSplit}";
}
```

| Property | Type | Description |
|---|---|---|
| `DateTime` | `DateTime` | Split effective date (UTC) |
| `BeforeSplit` | `decimal` | Number of shares before the split |
| `AfterSplit` | `decimal` | Number of shares after the split |
| `SplitRatio` | `string` | Computed ratio string, e.g. `"4:1"` |

**Example**: A 4-for-1 split has `AfterSplit = 4`, `BeforeSplit = 1`, `SplitRatio = "4:1"`.

---

## `SearchResult`

Namespace: `YahooFinanceService.Domain`

Represents a single symbol search result returned by `SearchSymbolsAsync`.

```csharp
public sealed record SearchResult
{
    public required string  Symbol    { get; init; }
    public          string? Exchange  { get; init; }
    public          string? ShortName { get; init; }
    public          string? LongName  { get; init; }
    public          string? Type      { get; init; }
    public          string? Sector    { get; init; }
    public          string? Industry  { get; init; }
    public          double? Score     { get; init; }
}
```

| Property | Type | Description |
|---|---|---|
| `Symbol` | `string` | Ticker symbol |
| `Exchange` | `string?` | Exchange code (e.g. `"NMS"`, `"NYQ"`) |
| `ShortName` | `string?` | Abbreviated company/fund name |
| `LongName` | `string?` | Full company/fund name |
| `Type` | `string?` | Instrument type: `"EQUITY"`, `"ETF"`, `"INDEX"`, `"FUTURE"`, `"CURRENCY"` |
| `Sector` | `string?` | GICS sector (equities only) |
| `Industry` | `string?` | GICS industry (equities only) |
| `Score` | `double?` | Yahoo Finance relevance score (higher = more relevant) |

---

## `Period` Enum

Namespace: `YahooFinanceService.Domain`

Controls the candle interval for `GetHistoricalAsync`.

```csharp
public enum Period
{
    Daily,    // 1d  — one candle per trading day
    Weekly,   // 1wk — one candle per trading week
    Monthly,  // 1mo — one candle per calendar month
}
```

### `PeriodExtensions`

```csharp
public static class PeriodExtensions
{
    /// <summary>Returns the Yahoo Finance API interval string for the period.</summary>
    public static string ToApiValue(this Period period);
    // Period.Daily   → "1d"
    // Period.Weekly  → "1wk"
    // Period.Monthly → "1mo"
}
```
