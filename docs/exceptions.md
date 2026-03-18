# Exception Handling

The library defines a typed exception hierarchy so callers can react precisely to different failure conditions without catching generic `Exception` types.

---

## Exception Hierarchy

```
Exception
└── YahooFinanceException          (base — any API or HTTP error)
    ├── InvalidSymbolException     (404 — symbol not found)
    ├── RateLimitException         (429 — too many requests)
    └── SessionException           (401 / cookie-crumb failure)
```

---

## `YahooFinanceException`

Namespace: `YahooFinanceService.Exceptions`

Base class for all library exceptions. Catch this type when you want a single handler for any Yahoo Finance error.

```csharp
public class YahooFinanceException : Exception
{
    /// <summary>HTTP status code returned by Yahoo Finance, or null for non-HTTP errors.</summary>
    public int? StatusCode { get; }
}
```

**Constructors**

```csharp
YahooFinanceException(string message)
YahooFinanceException(string message, Exception innerException)
YahooFinanceException(string message, int statusCode)
YahooFinanceException(string message, int statusCode, Exception innerException)
```

---

## `InvalidSymbolException`

Thrown when Yahoo Finance returns a **404** response for a given symbol.

```csharp
public sealed class InvalidSymbolException : YahooFinanceException
{
    /// <summary>The ticker symbol that was not found.</summary>
    public string Symbol { get; }
}
```

**When is it thrown?**

- `GetHistoricalAsync` — symbol not found in chart API
- `GetDividendsAsync` — symbol not found in chart API
- `GetSplitsAsync` — symbol not found in chart API
- `GetQuotesAsync` — symbol present in response but marked as invalid

**Example**

```csharp
try
{
    var candles = await yahoo.GetHistoricalAsync("XXXINVALID");
}
catch (InvalidSymbolException ex)
{
    Console.WriteLine($"'{ex.Symbol}' is not a valid ticker.");
}
```

---

## `RateLimitException`

Thrown when Yahoo Finance returns **HTTP 429 Too Many Requests**. The library does **not** retry on 429 — it propagates immediately so callers can implement their own back-off strategy.

```csharp
public sealed class RateLimitException : YahooFinanceException
{
    /// <summary>When to retry the request, if provided in the Retry-After header.</summary>
    public DateTimeOffset? RetryAfter { get; }

    // StatusCode is always 429
}
```

**Example**

```csharp
try
{
    var quote = await yahoo.GetQuoteAsync("AAPL");
}
catch (RateLimitException ex)
{
    var wait = ex.RetryAfter.HasValue
        ? ex.RetryAfter.Value - DateTimeOffset.UtcNow
        : TimeSpan.FromSeconds(60);

    Console.WriteLine($"Rate limit hit. Retrying in {wait.TotalSeconds:F0}s…");
    await Task.Delay(wait);
}
```

---

## `SessionException`

Thrown when the library cannot establish or maintain a Yahoo Finance session (i.e., cookie and crumb token acquisition fails).

```csharp
public sealed class SessionException : YahooFinanceException { }
```

**Common causes**

| Cause | Description |
|---|---|
| IP blocked | Yahoo Finance has blocked the requesting IP address |
| Network issue | `fc.yahoo.com` or the crumb endpoint is unreachable |
| API change | Yahoo Finance changed its authentication flow |
| Expired credentials | Session cookie expired and refresh failed |

**Example**

```csharp
try
{
    var quote = await yahoo.GetQuoteAsync("AAPL");
}
catch (SessionException ex)
{
    // Log and alert — nothing the caller can do except retry later
    logger.LogError(ex, "Failed to establish Yahoo Finance session");
}
```

---

## Recommended Handler Pattern

Order your catch blocks from most specific to least specific:

```csharp
try
{
    var candles = await yahoo.GetHistoricalAsync(
        symbol, startDate, endDate, period, cancellationToken);

    // … process candles …
}
catch (OperationCanceledException)
{
    // Request was cancelled — do not log as an error
}
catch (InvalidSymbolException ex)
{
    logger.LogWarning("Symbol {Symbol} not found on Yahoo Finance", ex.Symbol);
}
catch (RateLimitException ex)
{
    logger.LogWarning("Yahoo Finance rate limit hit. RetryAfter={RetryAfter}", ex.RetryAfter);
    // Schedule a retry with exponential back-off
}
catch (SessionException ex)
{
    logger.LogError(ex, "Yahoo Finance session could not be established");
    // Alert ops — possibly an IP block or API change
}
catch (YahooFinanceException ex)
{
    logger.LogError(ex, "Yahoo Finance API error (HTTP {StatusCode})", ex.StatusCode);
}
```

---

## Retry Behaviour

The library automatically retries **transient** errors up to `MaxRetryAttempts` times (default: 3), with a `RetryDelay` (default: 1 second) between attempts and a session refresh on each retry.

| Exception | Auto-Retried? |
|---|---|
| `InvalidSymbolException` | ❌ No — permanent error |
| `RateLimitException` | ❌ No — propagated immediately |
| `SessionException` | ✅ Yes — session is refreshed |
| `YahooFinanceException` (other) | ✅ Yes |

To disable auto-retry, set `MaxRetryAttempts = 0` in [configuration](configuration.md).
