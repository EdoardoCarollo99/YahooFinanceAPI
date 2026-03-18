# API Reference

Full reference for all public methods exposed by `IYahooFinanceService`.

---

## Interface: `IYahooFinanceService`

Namespace: `YahooFinanceService.Services`

```csharp
public interface IYahooFinanceService
{
    Task<IReadOnlyList<Candle>>              GetHistoricalAsync(string symbol, DateTime? startDate = null, DateTime? endDate = null, Period period = Period.Daily, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DividendTick>>        GetDividendsAsync(string symbol, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SplitTick>>           GetSplitsAsync(string symbol, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, Quote>> GetQuotesAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default);
    Task<Quote?>                             GetQuoteAsync(string symbol, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SearchResult>>        SearchSymbolsAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default);
}
```

---

## Methods

### `GetHistoricalAsync`

Retrieves OHLCV (Open, High, Low, Close, Volume) candle data for a symbol.

```csharp
Task<IReadOnlyList<Candle>> GetHistoricalAsync(
    string            symbol,
    DateTime?         startDate         = null,
    DateTime?         endDate           = null,
    Period            period            = Period.Daily,
    CancellationToken cancellationToken = default);
```

**Parameters**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `symbol` | `string` | — | Ticker symbol (e.g. `"AAPL"`) |
| `startDate` | `DateTime?` | 1 year ago | Start of the date range (UTC) |
| `endDate` | `DateTime?` | Today (UTC) | End of the date range (UTC) |
| `period` | `Period` | `Period.Daily` | Candle interval — `Daily`, `Weekly`, or `Monthly` |
| `cancellationToken` | `CancellationToken` | `default` | Optional cancellation token |

**Returns**: `IReadOnlyList<Candle>` — ordered chronologically; empty list if no data in range.

**Throws**

| Exception | When |
|---|---|
| `InvalidSymbolException` | The symbol does not exist on Yahoo Finance |
| `RateLimitException` | HTTP 429 — too many requests |
| `SessionException` | Session/cookie acquisition failed |
| `YahooFinanceException` | Any other API error |

**Example**

```csharp
var candles = await yahoo.GetHistoricalAsync(
    "TSLA",
    startDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    endDate:   new DateTime(2024, 6, 30, 0, 0, 0, DateTimeKind.Utc),
    period:    Period.Weekly);
```

---

### `GetDividendsAsync`

Retrieves dividend payment history for a symbol.

```csharp
Task<IReadOnlyList<DividendTick>> GetDividendsAsync(
    string            symbol,
    DateTime?         startDate         = null,
    DateTime?         endDate           = null,
    CancellationToken cancellationToken = default);
```

**Parameters**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `symbol` | `string` | — | Ticker symbol |
| `startDate` | `DateTime?` | 1 year ago | Start of the date range |
| `endDate` | `DateTime?` | Today | End of the date range |
| `cancellationToken` | `CancellationToken` | `default` | Optional cancellation token |

**Returns**: `IReadOnlyList<DividendTick>` — sorted by date ascending; empty list if no dividends.

**Example**

```csharp
var dividends = await yahoo.GetDividendsAsync(
    "MSFT",
    startDate: DateTime.UtcNow.AddYears(-2));

decimal total = dividends.Sum(d => d.Dividend);
Console.WriteLine($"Total dividends paid: ${total:F4}");
```

---

### `GetSplitsAsync`

Retrieves stock split history for a symbol.

```csharp
Task<IReadOnlyList<SplitTick>> GetSplitsAsync(
    string            symbol,
    DateTime?         startDate         = null,
    DateTime?         endDate           = null,
    CancellationToken cancellationToken = default);
```

**Parameters**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `symbol` | `string` | — | Ticker symbol |
| `startDate` | `DateTime?` | 10 years ago | Start of the date range |
| `endDate` | `DateTime?` | Today | End of the date range |
| `cancellationToken` | `CancellationToken` | `default` | Optional cancellation token |

**Returns**: `IReadOnlyList<SplitTick>` — sorted by date ascending; empty list if no splits in range.

**Example**

```csharp
var splits = await yahoo.GetSplitsAsync("AAPL");

foreach (var split in splits)
    Console.WriteLine($"{split.DateTime:yyyy-MM-dd}  {split.SplitRatio}");
```

---

### `GetQuotesAsync`

Retrieves real-time market data for multiple symbols in a single request.

```csharp
Task<IReadOnlyDictionary<string, Quote>> GetQuotesAsync(
    IEnumerable<string> symbols,
    CancellationToken   cancellationToken = default);
```

**Parameters**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `symbols` | `IEnumerable<string>` | — | Collection of ticker symbols |
| `cancellationToken` | `CancellationToken` | `default` | Optional cancellation token |

**Returns**: `IReadOnlyDictionary<string, Quote>` — keyed by upper-case symbol; symbols not found by Yahoo Finance are omitted.

**Throws**

| Exception | When |
|---|---|
| `RateLimitException` | HTTP 429 |
| `SessionException` | Session failure |
| `YahooFinanceException` | Any other API error |

**Example**

```csharp
var quotes = await yahoo.GetQuotesAsync(["AAPL", "MSFT", "GOOGL", "AMZN"]);

foreach (var (symbol, q) in quotes.OrderByDescending(x => x.Value.MarketCap))
    Console.WriteLine($"{symbol,-6} ${q.RegularMarketPrice,10:F2}  Cap: {q.MarketCap:N0}");
```

---

### `GetQuoteAsync`

Retrieves real-time market data for a single symbol.

```csharp
Task<Quote?> GetQuoteAsync(
    string            symbol,
    CancellationToken cancellationToken = default);
```

**Parameters**

| Parameter | Type | Description |
|---|---|---|
| `symbol` | `string` | Ticker symbol |
| `cancellationToken` | `CancellationToken` | Optional cancellation token |

**Returns**: `Quote?` — `null` if Yahoo Finance does not return data for the symbol.

**Example**

```csharp
var quote = await yahoo.GetQuoteAsync("NVDA");

if (quote is not null)
    Console.WriteLine($"{quote.LongName}: ${quote.RegularMarketPrice:F2}");
```

---

### `SearchSymbolsAsync`

Searches for ticker symbols by name or ticker string.

```csharp
Task<IReadOnlyList<SearchResult>> SearchSymbolsAsync(
    string            query,
    int               maxResults        = 10,
    CancellationToken cancellationToken = default);
```

**Parameters**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `query` | `string` | — | Search term (name or partial ticker) |
| `maxResults` | `int` | `10` | Maximum number of results to return |
| `cancellationToken` | `CancellationToken` | `default` | Optional cancellation token |

**Returns**: `IReadOnlyList<SearchResult>` — ordered by relevance score descending; empty list if no results.

**Example**

```csharp
var results = await yahoo.SearchSymbolsAsync("Tesla", maxResults: 5);

foreach (var r in results)
    Console.WriteLine($"{r.Symbol,-10} {r.LongName,-40} {r.Exchange}  [{r.Type}]");
```

---

## HTTP Client Interface: `IYahooClient`

Namespace: `YahooFinanceService.Infrastructure`

Lower-level interface used internally. Available for injection if you need direct authenticated HTTP access.

```csharp
public interface IYahooClient
{
    /// <summary>Sends an authenticated GET request and returns the raw response body.</summary>
    Task<string> GetAsync(string requestUri, CancellationToken cancellationToken = default);

    /// <summary>Sends an authenticated GET request and deserialises the JSON response.</summary>
    Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default);
}
```

---

## Session Interface: `IYahooSessionManager`

Namespace: `YahooFinanceService.Infrastructure`

```csharp
public interface IYahooSessionManager
{
    /// <summary>Returns the current session, initialising it lazily on first call.</summary>
    Task<YahooSessionData> GetSessionDataAsync(CancellationToken cancellationToken = default);

    /// <summary>Forces a full session re-initialisation (new cookie + new crumb).</summary>
    Task<YahooSessionData> RefreshSessionAsync(CancellationToken cancellationToken = default);
}
```
