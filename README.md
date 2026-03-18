# YahooFinanceAPI

<div align="justify">

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

A modern, strongly-typed .NET 10 library for accessing **Yahoo Finance** market data — historical prices, real-time quotes, dividends, stock splits, and symbol search — with built-in retry logic, session management, and full dependency injection support.

</div>

---

## ✨ Features

| Feature | Details |
|---|---|
| 📊 **Historical Data** | Daily, weekly, monthly OHLCV candles with adjustments |
| 📈 **Real-time Quotes** | Single or batch quotes with 40+ fields per symbol |
| 💰 **Dividend History** | Ex-dividend dates and per-share amounts |
| 🔀 **Stock Splits** | Split dates and ratios |
| 🔍 **Symbol Search** | Search by name or ticker with relevance scoring |
| 🔄 **Auto-Retry** | Configurable retry logic with session refresh |
| 🛡️ **Rate Limit Handling** | `RateLimitException` with `RetryAfter` timestamp |
| 💉 **Dependency Injection** | First-class support via `IServiceCollection` |
| ⚡ **Fully Async** | Complete async/await support with `CancellationToken` |
| 🔒 **Nullable References** | Full C# nullable reference type annotations |

---

## 🚀 Quick Start

### 1. Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;
using YahooFinanceService.Extensions;

var services = new ServiceCollection();
services.AddYahooFinance();   // uses sensible defaults
var provider = services.BuildServiceProvider();
```

With `IHostBuilder` (ASP.NET Core, Worker Service, etc.):

```csharp
builder.Services.AddYahooFinance();
```

### 2. Inject and Use

```csharp
using YahooFinanceService.Services;
using YahooFinanceService.Domain;

public class MyService(IYahooFinanceService yahoo)
{
    public async Task PrintQuoteAsync(string symbol)
    {
        var quote = await yahoo.GetQuoteAsync(symbol);
        Console.WriteLine($"{quote?.Symbol}: ${quote?.RegularMarketPrice:F2}");
    }

    public async Task PrintHistoryAsync(string symbol)
    {
        var candles = await yahoo.GetHistoricalAsync(
            symbol,
            startDate: DateTime.UtcNow.AddMonths(-1),
            period: Period.Daily);

        foreach (var c in candles)
            Console.WriteLine($"{c.DateTime:yyyy-MM-dd}  O:{c.Open}  H:{c.High}  L:{c.Low}  C:{c.Close}  V:{c.Volume}");
    }
}
```

---

## 📚 Documentation

| Document | Description |
|---|---|
| [Getting Started](docs/getting-started.md) | Installation, setup, and first steps |
| [Configuration](docs/configuration.md) | All configuration options and defaults |
| [API Reference](docs/api-reference.md) | Full method signatures and return types |
| [Domain Models](docs/domain-models.md) | All data models and their properties |
| [Exception Handling](docs/exceptions.md) | Exception hierarchy and error handling guide |
| [Dependency Injection](docs/dependency-injection.md) | DI registration and lifetime management |
| [Examples](docs/examples.md) | Real-world code examples and patterns |

---

## 🗂️ Project Structure

```
YahooFinanceAPI/
├── YahooFinanceService/          # 📦 Main library
│   ├── Configuration/            # Options & session data
│   ├── Domain/                   # Data models (Candle, Quote, …)
│   ├── Exceptions/               # Typed exception hierarchy
│   ├── Extensions/               # IServiceCollection extensions
│   ├── Infrastructure/           # HTTP client & session manager
│   └── Services/                 # IYahooFinanceService + implementation
│
└── YahooService.Demo/            # 🖥️ Interactive console demo
    ├── DemoRunner.cs             # All feature demonstrations
    └── Program.cs                # Entry point & DI setup
```

---

## 🔑 Authentication

No API key required. The library authenticates automatically using Yahoo Finance's session mechanism:

1. Fetches a session cookie from `https://fc.yahoo.com/`
2. Retrieves a crumb token from Yahoo Finance
3. Attaches both to every subsequent API request

Sessions are cached for **1 hour** and refreshed automatically on expiry or 401 responses.

> ⚠️ Yahoo Finance may throttle or block IPs making excessive requests. Use the built-in retry and rate limit handling.

---

## ⚡ API Endpoints

| Endpoint | Used For |
|---|---|
| `GET /v8/finance/chart/{symbol}` | Historical prices, dividends, splits |
| `GET /v7/finance/quote` | Real-time quotes (single / batch) |
| `GET /v1/finance/search` | Symbol search |
| `GET /v1/test/getcrumb` | Session crumb token |
| `GET https://fc.yahoo.com/` | Session cookie |

---

## 🛠️ Error Handling

```csharp
try
{
    var candles = await yahoo.GetHistoricalAsync("INVALID");
}
catch (InvalidSymbolException ex)
{
    // ex.Symbol contains the offending ticker
}
catch (RateLimitException ex)
{
    // ex.RetryAfter tells you when to retry
}
catch (SessionException ex)
{
    // Cookie/crumb acquisition failed
}
catch (YahooFinanceException ex)
{
    // Base class — ex.StatusCode contains the HTTP status
}
```

See [Exception Handling](docs/exceptions.md) for the full guide.

---

## 📋 Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- `Microsoft.Extensions.DependencyInjection.Abstractions` ≥ 9.0.0
- `Microsoft.Extensions.Http` ≥ 9.0.0
- `Microsoft.Extensions.Options` ≥ 9.0.0

---

## 📄 License

This project is licensed under the **MIT License** — see [LICENSE.txt](LICENSE.txt) for details.

---

<div align="center">

Built with ❤️ on .NET 10 · [Documentation](docs/getting-started.md) · [Report a Bug](../../issues) · [Request a Feature](../../issues)

</div>
