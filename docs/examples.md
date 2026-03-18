# Examples

Real-world code examples covering all features of `IYahooFinanceService`.

---

## Setup

All examples assume the following bootstrap code:

```csharp
using Microsoft.Extensions.DependencyInjection;
using YahooFinanceService.Extensions;
using YahooFinanceService.Services;
using YahooFinanceService.Domain;
using YahooFinanceService.Exceptions;

var services = new ServiceCollection();
services.AddYahooFinance();
var provider = services.BuildServiceProvider();

var yahoo = provider.GetRequiredService<IYahooFinanceService>();
```

---

## Historical Data

### Get the last month of daily candles

```csharp
var candles = await yahoo.GetHistoricalAsync(
    "AAPL",
    startDate: DateTime.UtcNow.AddMonths(-1),
    period: Period.Daily);

Console.WriteLine($"{"Date",-12} {"Open",8} {"High",8} {"Low",8} {"Close",8} {"Volume",12}");
foreach (var c in candles)
{
    Console.WriteLine(
        $"{c.DateTime:yyyy-MM-dd}  {c.Open,8:F2} {c.High,8:F2} {c.Low,8:F2} {c.Close,8:F2} {c.Volume,12:N0}");
}
```

### Get weekly candles for a custom date range

```csharp
var candles = await yahoo.GetHistoricalAsync(
    "MSFT",
    startDate: new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    endDate:   new DateTime(2023, 12, 31, 0, 0, 0, DateTimeKind.Utc),
    period:    Period.Weekly);

Console.WriteLine($"Fetched {candles.Count} weekly bars.");
```

### Calculate simple statistics

```csharp
var candles = await yahoo.GetHistoricalAsync("GOOGL", startDate: DateTime.UtcNow.AddYears(-1));

if (candles.Count > 0)
{
    var high      = candles.Max(c => c.High);
    var low       = candles.Min(c => c.Low);
    var avgVol    = candles.Average(c => c.Volume);
    var priceChg  = candles[^1].Close - candles[0].Open;
    var pctChange = priceChg / candles[0].Open * 100m;

    Console.WriteLine($"52-week high  : ${high:F2}");
    Console.WriteLine($"52-week low   : ${low:F2}");
    Console.WriteLine($"Avg volume    : {avgVol:N0}");
    Console.WriteLine($"Price change  : {pctChange:+0.00;-0.00}%");
}
```

---

## Real-time Quotes

### Get a single quote

```csharp
var quote = await yahoo.GetQuoteAsync("NVDA");

if (quote is null)
{
    Console.WriteLine("Symbol not found.");
    return;
}

Console.WriteLine($"Symbol    : {quote.Symbol}");
Console.WriteLine($"Name      : {quote.LongName}");
Console.WriteLine($"Exchange  : {quote.Exchange}");
Console.WriteLine($"Price     : ${quote.RegularMarketPrice:F2}");
Console.WriteLine($"Change    : {quote.RegularMarketChange:+0.00;-0.00} ({quote.RegularMarketChangePercent:+0.00%;-0.00%})");
Console.WriteLine($"Open      : ${quote.RegularMarketOpen:F2}");
Console.WriteLine($"Day Range : ${quote.RegularMarketDayLow:F2} – ${quote.RegularMarketDayHigh:F2}");
Console.WriteLine($"Volume    : {quote.RegularMarketVolume:N0}");
Console.WriteLine($"Mkt Cap   : ${quote.MarketCap:N0}");
Console.WriteLine($"P/E       : {quote.TrailingPE:F2}");
Console.WriteLine($"52w High  : ${quote.FiftyTwoWeekHigh:F2}");
Console.WriteLine($"52w Low   : ${quote.FiftyTwoWeekLow:F2}");
```

### Get multiple quotes and rank by market cap

```csharp
var symbols = new[] { "AAPL", "MSFT", "GOOGL", "AMZN", "NVDA", "META", "TSLA" };
var quotes  = await yahoo.GetQuotesAsync(symbols);

Console.WriteLine($"{"Symbol",-8} {"Name",-30} {"Price",10} {"Chg%",8} {"Mkt Cap",18}");
Console.WriteLine(new string('-', 80));

foreach (var (symbol, q) in quotes.OrderByDescending(x => x.Value.MarketCap))
{
    var capStr = q.MarketCap switch
    {
        >= 1_000_000_000_000 => $"${q.MarketCap / 1_000_000_000_000m:F2}T",
        >= 1_000_000_000     => $"${q.MarketCap / 1_000_000_000m:F2}B",
        >= 1_000_000         => $"${q.MarketCap / 1_000_000m:F2}M",
        _                    => "—"
    };

    Console.WriteLine(
        $"{symbol,-8} {q.LongName ?? q.ShortName ?? "—",-30} " +
        $"${q.RegularMarketPrice,9:F2} " +
        $"{q.RegularMarketChangePercent,8:+0.00%;-0.00%} " +
        $"{capStr,18}");
}
```

### Check pre-market / post-market data

```csharp
var quote = await yahoo.GetQuoteAsync("AAPL");

if (quote?.PreMarketPrice.HasValue == true)
    Console.WriteLine($"Pre-market : ${quote.PreMarketPrice:F2} ({quote.PreMarketChangePercent:+0.00%;-0.00%})");

if (quote?.PostMarketPrice.HasValue == true)
    Console.WriteLine($"Post-market: ${quote.PostMarketPrice:F2} ({quote.PostMarketChangePercent:+0.00%;-0.00%})");
```

---

## Dividends

### List dividend history

```csharp
var dividends = await yahoo.GetDividendsAsync(
    "MSFT",
    startDate: DateTime.UtcNow.AddYears(-2));

Console.WriteLine($"{"Date",-14} {"Dividend/Share",15}");
foreach (var d in dividends)
    Console.WriteLine($"{d.DateTime:yyyy-MM-dd}    ${d.Dividend:F4}");

Console.WriteLine($"\nTotal paid over period: ${dividends.Sum(d => d.Dividend):F4}");
```

### Group dividends by year

```csharp
var dividends = await yahoo.GetDividendsAsync("JNJ", startDate: DateTime.UtcNow.AddYears(-5));

var byYear = dividends
    .GroupBy(d => d.DateTime.Year)
    .OrderByDescending(g => g.Key);

foreach (var group in byYear)
    Console.WriteLine($"{group.Key}: ${group.Sum(d => d.Dividend):F4} ({group.Count()} payments)");
```

---

## Stock Splits

### List split history

```csharp
var splits = await yahoo.GetSplitsAsync("TSLA", startDate: DateTime.UtcNow.AddYears(-5));

if (splits.Count == 0)
{
    Console.WriteLine("No splits in the requested range.");
    return;
}

foreach (var s in splits)
    Console.WriteLine($"{s.DateTime:yyyy-MM-dd}  {s.SplitRatio}  ({s.BeforeSplit}:{s.AfterSplit})");
```

---

## Symbol Search

### Search by company name

```csharp
var results = await yahoo.SearchSymbolsAsync("Tesla", maxResults: 10);

Console.WriteLine($"{"Symbol",-10} {"Name",-40} {"Exchange",-8} {"Type",-10} {"Sector"}");
Console.WriteLine(new string('-', 90));

foreach (var r in results)
{
    Console.WriteLine(
        $"{r.Symbol,-10} {r.LongName ?? r.ShortName ?? "—",-40} " +
        $"{r.Exchange,-8} {r.Type,-10} {r.Sector ?? "—"}");
}
```

### Filter search results by type

```csharp
var results = await yahoo.SearchSymbolsAsync("S&P 500", maxResults: 20);

var etfs = results.Where(r => r.Type == "ETF").ToList();
Console.WriteLine($"Found {etfs.Count} ETF(s) matching 'S&P 500':");

foreach (var etf in etfs)
    Console.WriteLine($"  {etf.Symbol} — {etf.LongName}");
```

---

## Error Handling

### Full handler pattern

```csharp
async Task SafeGetHistoricalAsync(string symbol)
{
    try
    {
        var candles = await yahoo.GetHistoricalAsync(
            symbol,
            startDate: DateTime.UtcNow.AddMonths(-3),
            period: Period.Daily);

        Console.WriteLine($"Retrieved {candles.Count} candles for {symbol}.");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Request was cancelled.");
    }
    catch (InvalidSymbolException ex)
    {
        Console.WriteLine($"Error: '{ex.Symbol}' is not a recognised ticker symbol.");
    }
    catch (RateLimitException ex)
    {
        var wait = ex.RetryAfter.HasValue
            ? ex.RetryAfter.Value - DateTimeOffset.UtcNow
            : TimeSpan.FromMinutes(1);

        Console.WriteLine($"Rate limit exceeded. Retry in {wait.TotalSeconds:F0}s.");
        await Task.Delay(wait);
        await SafeGetHistoricalAsync(symbol);   // retry once
    }
    catch (SessionException ex)
    {
        Console.WriteLine($"Session error: {ex.Message}");
    }
    catch (YahooFinanceException ex)
    {
        Console.WriteLine($"Yahoo Finance error (HTTP {ex.StatusCode}): {ex.Message}");
    }
}
```

---

## Cancellation

All methods accept a `CancellationToken`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

try
{
    var quote = await yahoo.GetQuoteAsync("AAPL", cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Request timed out after 10 seconds.");
}
```

---

## Custom Configuration

```csharp
services.AddYahooFinance(() => new YahooFinanceOptions
{
    BaseUrl          = "https://query1.finance.yahoo.com",   // alternate endpoint
    CookieUrl        = "https://fc.yahoo.com/",
    CrumbUrl         = "https://query1.finance.yahoo.com/v1/test/getcrumb",
    UserAgent        = "MyFinanceApp/2.0",
    RequestTimeout   = TimeSpan.FromSeconds(60),
    MaxRetryAttempts = 5,
    RetryDelay       = TimeSpan.FromSeconds(2),
});
```

---

## ASP.NET Core Integration

```csharp
// Program.cs
builder.Services.AddYahooFinance();

// MarketController.cs
[ApiController]
[Route("api/market")]
public class MarketController(IYahooFinanceService yahoo) : ControllerBase
{
    [HttpGet("quote/{symbol}")]
    public async Task<IActionResult> GetQuote(string symbol, CancellationToken ct)
    {
        try
        {
            var quote = await yahoo.GetQuoteAsync(symbol, ct);
            return quote is null ? NotFound() : Ok(quote);
        }
        catch (InvalidSymbolException)
        {
            return NotFound(new { error = $"Symbol '{symbol}' not found." });
        }
        catch (RateLimitException ex)
        {
            return StatusCode(429, new { error = "Rate limit exceeded.", retryAfter = ex.RetryAfter });
        }
    }

    [HttpGet("history/{symbol}")]
    public async Task<IActionResult> GetHistory(
        string symbol,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Period period = Period.Daily,
        CancellationToken ct = default)
    {
        var candles = await yahoo.GetHistoricalAsync(symbol, from, to, period, ct);
        return Ok(candles);
    }
}
```
