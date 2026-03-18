# Getting Started

This guide walks you through installing **YahooFinanceAPI**, registering its services, and making your first API call.

---

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- An internet connection (the library calls the Yahoo Finance public API)

---

## Installation

### From NuGet (once published)

```bash
dotnet add package YahooFinanceAPI
```

### From source

```bash
git clone https://github.com/<your-org>/YahooFinanceAPI.git
```

Then add a project reference in your `.csproj`:

```xml
<ProjectReference Include="../YahooFinanceAPI/YahooFinanceService/YahooFinanceService.csproj" />
```

---

## Service Registration

The library integrates with Microsoft's dependency injection system via the `AddYahooFinance()` extension method.

### Minimal (recommended defaults)

```csharp
using YahooFinanceService.Extensions;

services.AddYahooFinance();
```

### With ASP.NET Core / Generic Host

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddYahooFinance();
var app = builder.Build();
```

### With a Worker Service

```csharp
Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddYahooFinance();
        services.AddHostedService<MyWorker>();
    })
    .Build()
    .Run();
```

### Standalone (no host)

```csharp
using Microsoft.Extensions.DependencyInjection;
using YahooFinanceService.Extensions;

var services = new ServiceCollection();
services.AddYahooFinance();
var provider = services.BuildServiceProvider();

var yahoo = provider.GetRequiredService<IYahooFinanceService>();
```

> All registered services are **singletons**. Create the service provider once and reuse it throughout your application.

---

## Your First API Call

### Get a real-time quote

```csharp
using YahooFinanceService.Services;

public class QuoteService(IYahooFinanceService yahoo)
{
    public async Task PrintAsync()
    {
        var quote = await yahoo.GetQuoteAsync("AAPL");

        if (quote is null)
        {
            Console.WriteLine("Symbol not found.");
            return;
        }

        Console.WriteLine($"Symbol  : {quote.Symbol}");
        Console.WriteLine($"Name    : {quote.LongName}");
        Console.WriteLine($"Price   : ${quote.RegularMarketPrice:F2}");
        Console.WriteLine($"Change  : {quote.RegularMarketChangePercent:+0.00%;-0.00%}");
        Console.WriteLine($"Volume  : {quote.RegularMarketVolume:N0}");
    }
}
```

### Get historical OHLCV data

```csharp
using YahooFinanceService.Domain;

var candles = await yahoo.GetHistoricalAsync(
    symbol:    "AAPL",
    startDate: DateTime.UtcNow.AddMonths(-1),
    endDate:   DateTime.UtcNow,
    period:    Period.Daily);

foreach (var candle in candles)
{
    Console.WriteLine(
        $"{candle.DateTime:yyyy-MM-dd}  " +
        $"O:{candle.Open:F2}  H:{candle.High:F2}  " +
        $"L:{candle.Low:F2}  C:{candle.Close:F2}  " +
        $"V:{candle.Volume:N0}");
}
```

---

## Running the Demo App

The repository ships with an interactive console demo that exercises every feature:

```bash
cd YahooService.Demo
dotnet run
```

A menu will appear:

```
1. 📊 Historical Data (OHLCV prices)
2. 💰 Dividend History
3. 🔀 Stock Split History
4. 📈 Single Quote (Real-time data)
5. 📊 Multiple Quotes
6. 🔍 Search Symbols
7. 🎯 Run All Tests
8. ⚠️  Error Handling Demo
0. ❌ Exit
```

---

## Next Steps

| Topic | Link |
|---|---|
| Customise timeouts, retries, and endpoints | [Configuration](configuration.md) |
| Full list of methods and return types | [API Reference](api-reference.md) |
| All data model properties | [Domain Models](domain-models.md) |
| Handle errors gracefully | [Exception Handling](exceptions.md) |
| Advanced DI patterns | [Dependency Injection](dependency-injection.md) |
| More code snippets | [Examples](examples.md) |
