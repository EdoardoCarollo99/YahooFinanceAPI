# Dependency Injection

`YahooFinanceAPI` is designed around Microsoft's DI abstractions (`Microsoft.Extensions.DependencyInjection`). All services are registered as **singletons** via a single extension method.

---

## Extension Methods

Namespace: `YahooFinanceService.Extensions`

### `AddYahooFinance()` — default configuration

```csharp
public static IServiceCollection AddYahooFinance(this IServiceCollection services)
```

Registers all services with `YahooFinanceOptions.Default`. This is the simplest option and suitable for most applications.

```csharp
services.AddYahooFinance();
```

---

### `AddYahooFinance(Func<YahooFinanceOptions>)` — factory delegate

```csharp
public static IServiceCollection AddYahooFinance(
    this IServiceCollection services,
    Func<YahooFinanceOptions> configureOptions)
```

Use when you need to construct options at registration time (e.g., reading values from a configuration source):

```csharp
services.AddYahooFinance(() => new YahooFinanceOptions
{
    BaseUrl          = configuration["YahooFinance:BaseUrl"]!,
    CookieUrl        = configuration["YahooFinance:CookieUrl"]!,
    CrumbUrl         = configuration["YahooFinance:CrumbUrl"]!,
    UserAgent        = configuration["YahooFinance:UserAgent"]!,
    RequestTimeout   = TimeSpan.FromSeconds(45),
    MaxRetryAttempts = 5,
    RetryDelay       = TimeSpan.FromSeconds(2),
});
```

---

### `AddYahooFinance(YahooFinanceOptions)` — pre-built instance

```csharp
public static IServiceCollection AddYahooFinance(
    this IServiceCollection services,
    YahooFinanceOptions options)
```

Use when you have already constructed the options object:

```csharp
var options = configuration
    .GetSection("YahooFinance")
    .Get<YahooFinanceOptions>()!;

services.AddYahooFinance(options);
```

---

## Registered Services

All services are registered with **Singleton** lifetime:

| Service | Implementation | Description |
|---|---|---|
| `IOptions<YahooFinanceOptions>` | — | Configuration accessor |
| `IHttpClientFactory` | Built-in | Manages `HttpClient` instances |
| `IYahooSessionManager` | `YahooSessionManager` | Cookie + crumb session management |
| `IYahooClient` | `YahooClient` | Authenticated HTTP requests with retry |
| `IYahooFinanceService` | `YahooFinanceService` | Public API surface |

> **Note:** `IHttpClientFactory` is registered by the library via `services.AddHttpClient()`. If your application already calls `AddHttpClient()`, this is idempotent and safe.

---

## Injecting the Service

### Constructor injection (recommended)

```csharp
public class MarketDataService(IYahooFinanceService yahoo)
{
    public async Task<decimal?> GetPriceAsync(string symbol)
    {
        var quote = await yahoo.GetQuoteAsync(symbol);
        return quote?.RegularMarketPrice;
    }
}
```

### Minimal API / lambda injection

```csharp
app.MapGet("/quote/{symbol}", async (string symbol, IYahooFinanceService yahoo) =>
{
    var quote = await yahoo.GetQuoteAsync(symbol);
    return quote is null ? Results.NotFound() : Results.Ok(quote);
});
```

### Manual resolution (non-DI contexts)

```csharp
var provider = new ServiceCollection()
    .AddYahooFinance()
    .BuildServiceProvider();

var yahoo = provider.GetRequiredService<IYahooFinanceService>();
```

---

## Lifetime Considerations

Because all services are **singletons**:

- The `HttpClient` pool is reused for the lifetime of the application — efficient and avoids socket exhaustion.
- The session (cookie + crumb) is cached inside `YahooSessionManager` and reused across all concurrent calls.
- `IYahooFinanceService` is thread-safe and safe to inject into any lifetime scope (Singleton, Scoped, or Transient).

---

## Advanced: Replacing Internal Services

If you need to swap out the HTTP layer or session management (e.g., for testing), register your own implementation **before** calling `AddYahooFinance()`, and use the overload that accepts a pre-built `YahooFinanceOptions`:

```csharp
services.AddSingleton<IYahooSessionManager, MyCustomSessionManager>();
services.AddYahooFinance(YahooFinanceOptions.Default);
// YahooClient will resolve IYahooSessionManager from the container,
// which will use MyCustomSessionManager.
```

Or mock at the `IYahooFinanceService` level for unit tests:

```csharp
var mock = new Mock<IYahooFinanceService>();
mock.Setup(s => s.GetQuoteAsync("AAPL", default))
    .ReturnsAsync(new Quote { Symbol = "AAPL", RegularMarketPrice = 150m });

services.AddSingleton(mock.Object);
```
