# Configuration

`YahooFinanceOptions` is a sealed record that controls every aspect of the library's HTTP behaviour.

---

## Default Configuration

Calling `services.AddYahooFinance()` with no arguments uses `YahooFinanceOptions.Default`:

| Property | Default Value |
|---|---|
| `BaseUrl` | `https://query2.finance.yahoo.com` |
| `CookieUrl` | `https://guce.yahoo.com/v1/consentRecord?consentTypes=iab%2CiabCCPA%2Cgpp%2CgppSid` |
| `CrumbUrl` | `https://query2.finance.yahoo.com/v1/test/getcrumb` |
| `UserAgent` | `Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 ...` |
| `RequestTimeout` | `30` seconds |
| `MaxRetryAttempts` | `3` |
| `RetryDelay` | `1` second |

---

## `YahooFinanceOptions` Reference

```csharp
public sealed record YahooFinanceOptions
{
    /// <summary>Base URL for Yahoo Finance API (e.g. https://query2.finance.yahoo.com).</summary>
    public required string BaseUrl { get; init; }

    /// <summary>URL used to obtain the initial session cookie.</summary>
    public required string CookieUrl { get; init; }

    /// <summary>URL used to obtain the crumb CSRF token.</summary>
    public required string CrumbUrl { get; init; }

    /// <summary>HTTP User-Agent header sent with every request.</summary>
    public required string UserAgent { get; init; }

    /// <summary>Per-request timeout. Default: 30 seconds.</summary>
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>Maximum number of retry attempts on transient failures. Default: 3.</summary>
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>Delay between consecutive retry attempts. Default: 1 second.</summary>
    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>Creates a <see cref="YahooFinanceOptions"/> instance pre-populated with defaults.</summary>
    public static YahooFinanceOptions Default { get; }
}
```

---

## Overriding Configuration

### Via factory delegate

```csharp
services.AddYahooFinance(() => new YahooFinanceOptions
{
    BaseUrl            = "https://query1.finance.yahoo.com",
    CookieUrl          = "https://fc.yahoo.com/",
    CrumbUrl           = "https://query1.finance.yahoo.com/v1/test/getcrumb",
    UserAgent          = "MyApp/1.0",
    RequestTimeout     = TimeSpan.FromSeconds(60),
    MaxRetryAttempts   = 5,
    RetryDelay         = TimeSpan.FromSeconds(2),
});
```

### Via pre-built instance

```csharp
var options = new YahooFinanceOptions
{
    BaseUrl          = "https://query2.finance.yahoo.com",
    CookieUrl        = "https://guce.yahoo.com/v1/consentRecord?consentTypes=iab%2CiabCCPA%2Cgpp%2CgppSid",
    CrumbUrl         = "https://query2.finance.yahoo.com/v1/test/getcrumb",
    UserAgent        = "Mozilla/5.0",
    RequestTimeout   = TimeSpan.FromSeconds(45),
    MaxRetryAttempts = 2,
    RetryDelay       = TimeSpan.FromMilliseconds(500),
};

services.AddYahooFinance(options);
```

### Binding from `appsettings.json`

```json
{
  "YahooFinance": {
    "BaseUrl": "https://query2.finance.yahoo.com",
    "CookieUrl": "https://guce.yahoo.com/v1/consentRecord?consentTypes=iab%2CiabCCPA%2Cgpp%2CgppSid",
    "CrumbUrl": "https://query2.finance.yahoo.com/v1/test/getcrumb",
    "UserAgent": "Mozilla/5.0",
    "RequestTimeout": "00:00:30",
    "MaxRetryAttempts": 3,
    "RetryDelay": "00:00:01"
  }
}
```

```csharp
var options = builder.Configuration
    .GetSection("YahooFinance")
    .Get<YahooFinanceOptions>()!;

services.AddYahooFinance(options);
```

---

## Session Configuration (`YahooSessionData`)

`YahooSessionData` is an internal record managed by `YahooSessionManager`. It is **not** configured directly by consumers, but its lifetime is relevant:

| Property | Description |
|---|---|
| `Crumb` | CSRF protection token obtained from Yahoo Finance |
| `CookieValue` | Session cookie value |
| `CookieName` | Cookie name (`A3` or `B`) |
| `CreatedAt` | UTC timestamp of session creation |
| `IsValid(maxAge)` | `true` if the session has not exceeded `maxAge` |

**Session lifetime**: 1 hour. The manager automatically refreshes the session when it expires or when a `401 Unauthorized` response is received.

---

## Retry Behaviour

The built-in `YahooClient` applies the following retry strategy:

1. Execute the request.
2. If a `RateLimitException` (HTTP 429) is thrown → **do not retry**, propagate immediately.
3. On any other transient failure → wait `RetryDelay`, then refresh the session and retry.
4. After `MaxRetryAttempts` failures → throw the last exception.

To disable retries entirely, set `MaxRetryAttempts` to `0`:

```csharp
services.AddYahooFinance(() => YahooFinanceOptions.Default with { MaxRetryAttempts = 0 });
```
