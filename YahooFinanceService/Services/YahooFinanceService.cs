using Microsoft.Extensions.Options;
using System.Text.Json;
using YahooFinanceService.Configuration;
using YahooFinanceService.Domain;
using YahooFinanceService.Exceptions;
using YahooFinanceService.Infrastructure;

namespace YahooFinanceService.Services;

/// <summary>
/// Implementation of Yahoo Finance service.
/// </summary>
public sealed class YahooFinanceService(
    IYahooClient client,
    IOptions<YahooFinanceOptions> options) : IYahooFinanceService
{
    private static readonly DateTime DefaultStartDate = DateTime.UtcNow.AddYears(-1);
    private static readonly DateTime DefaultEndDate = DateTime.UtcNow;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Candle>> GetHistoricalAsync(
        string symbol,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Period period = Period.Daily,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        var start = startDate ?? DefaultStartDate;
        var end = endDate ?? DefaultEndDate;

        var url = BuildChartUrl(symbol, start, end, period.ToApiValue(), includeEvents: false);

        try
        {
            var response = await client.GetAsync(url, cancellationToken);
            return ChartDataParser.ParseCandles(response);
        }
        catch (YahooFinanceException ex) when (ex.StatusCode == 404)
        {
            throw new InvalidSymbolException(symbol);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DividendTick>> GetDividendsAsync(
        string symbol,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        var start = startDate ?? DefaultStartDate;
        var end = endDate ?? DefaultEndDate;

        var url = BuildChartUrl(symbol, start, end, "1d", includeEvents: true, events: "div");

        try
        {
            var response = await client.GetAsync(url, cancellationToken);
            return ChartDataParser.ParseDividends(response);
        }
        catch (YahooFinanceException ex) when (ex.StatusCode == 404)
        {
            throw new InvalidSymbolException(symbol);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SplitTick>> GetSplitsAsync(
        string symbol,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        var start = startDate ?? DateTime.UtcNow.AddYears(-10);
        var end = endDate ?? DefaultEndDate;

        var url = BuildChartUrl(symbol, start, end, "1d", includeEvents: true, events: "split");

        try
        {
            var response = await client.GetAsync(url, cancellationToken);
            return ChartDataParser.ParseSplits(response);
        }
        catch (YahooFinanceException ex) when (ex.StatusCode == 404)
        {
            throw new InvalidSymbolException(symbol);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, Quote>> GetQuotesAsync(
        IEnumerable<string> symbols,
        CancellationToken cancellationToken = default)
    {
        var symbolList = symbols.ToList();
        if (symbolList.Count == 0)
        {
            return new Dictionary<string, Quote>();
        }

        var symbolsParam = string.Join(",", symbolList.Select(Uri.EscapeDataString));
        var url = $"{options.Value.BaseUrl}/v7/finance/quote?symbols={symbolsParam}";

        var response = await client.GetAsync(url, cancellationToken);
        return ParseQuotes(response);
    }

    /// <inheritdoc/>
    public async Task<Quote?> GetQuoteAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        var quotes = await GetQuotesAsync([symbol], cancellationToken);
        return quotes.TryGetValue(symbol, out var quote) ? quote : null;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SearchResult>> SearchSymbolsAsync(
        string query,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var encodedQuery = Uri.EscapeDataString(query);
        var url = $"{options.Value.BaseUrl}/v1/finance/search?q={encodedQuery}&quotesCount={maxResults}";

        var response = await client.GetAsync(url, cancellationToken);
        return ParseSearchResults(response);
    }

    private string BuildChartUrl(
        string symbol,
        DateTime startDate,
        DateTime endDate,
        string interval,
        bool includeEvents,
        string? events = null)
    {
        var startTimestamp = new DateTimeOffset(startDate).ToUnixTimeSeconds();
        var endTimestamp = new DateTimeOffset(endDate).ToUnixTimeSeconds();

        var url = $"{options.Value.BaseUrl}/v8/finance/chart/{Uri.EscapeDataString(symbol)}" +
                  $"?period1={startTimestamp}&period2={endTimestamp}&interval={interval}";

        if (includeEvents && !string.IsNullOrEmpty(events))
        {
            url += $"&events={events}";
        }

        return url;
    }

    private static IReadOnlyDictionary<string, Quote> ParseQuotes(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!root.TryGetProperty("quoteResponse", out var quoteResponse) ||
            !quoteResponse.TryGetProperty("result", out var results))
        {
            return new Dictionary<string, Quote>();
        }

        var quotes = new Dictionary<string, Quote>();
        foreach (var result in results.EnumerateArray())
        {
            var quote = ParseQuote(result);
            if (quote != null)
            {
                quotes[quote.Symbol] = quote;
            }
        }

        return quotes;
    }

    private static Quote? ParseQuote(JsonElement element)
    {
        if (!element.TryGetProperty("symbol", out var symbolElement))
        {
            return null;
        }

        return new Quote
        {
            Symbol = symbolElement.GetString() ?? string.Empty,
            RegularMarketPrice = GetNullableDecimal(element, "regularMarketPrice"),
            RegularMarketTime = GetNullableUnixDateTime(element, "regularMarketTime"),
            RegularMarketChange = GetNullableDecimal(element, "regularMarketChange"),
            RegularMarketChangePercent = GetNullableDecimal(element, "regularMarketChangePercent"),
            RegularMarketOpen = GetNullableDecimal(element, "regularMarketOpen"),
            RegularMarketDayHigh = GetNullableDecimal(element, "regularMarketDayHigh"),
            RegularMarketDayLow = GetNullableDecimal(element, "regularMarketDayLow"),
            RegularMarketVolume = GetNullableLong(element, "regularMarketVolume"),
            RegularMarketPreviousClose = GetNullableDecimal(element, "regularMarketPreviousClose"),
            Bid = GetNullableDecimal(element, "bid"),
            Ask = GetNullableDecimal(element, "ask"),
            BidSize = GetNullableLong(element, "bidSize"),
            AskSize = GetNullableLong(element, "askSize"),
            MarketCap = GetNullableLong(element, "marketCap"),
            FiftyTwoWeekHigh = GetNullableDecimal(element, "fiftyTwoWeekHigh"),
            FiftyTwoWeekLow = GetNullableDecimal(element, "fiftyTwoWeekLow"),
            FiftyDayAverage = GetNullableDecimal(element, "fiftyDayAverage"),
            TwoHundredDayAverage = GetNullableDecimal(element, "twoHundredDayAverage"),
            TrailingPE = GetNullableDecimal(element, "trailingPE"),
            ForwardPE = GetNullableDecimal(element, "forwardPE"),
            TrailingAnnualDividendRate = GetNullableDecimal(element, "trailingAnnualDividendRate"),
            TrailingAnnualDividendYield = GetNullableDecimal(element, "trailingAnnualDividendYield"),
            Currency = GetNullableString(element, "currency"),
            Exchange = GetNullableString(element, "exchange"),
            ShortName = GetNullableString(element, "shortName"),
            LongName = GetNullableString(element, "longName")
        };
    }

    private static IReadOnlyList<SearchResult> ParseSearchResults(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!root.TryGetProperty("quotes", out var quotes))
        {
            return [];
        }

        var results = new List<SearchResult>();
        foreach (var quote in quotes.EnumerateArray())
        {
            if (quote.TryGetProperty("symbol", out var symbolElement))
            {
                results.Add(new SearchResult
                {
                    Symbol = symbolElement.GetString() ?? string.Empty,
                    Exchange = GetNullableString(quote, "exchange"),
                    ShortName = GetNullableString(quote, "shortname"),
                    LongName = GetNullableString(quote, "longname"),
                    Type = GetNullableString(quote, "quoteType"),
                    Sector = GetNullableString(quote, "sector"),
                    Industry = GetNullableString(quote, "industry"),
                    Score = GetNullableDouble(quote, "score")
                });
            }
        }

        return results;
    }

    private static decimal? GetNullableDecimal(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.GetDecimal()
            : null;
    }

    private static long? GetNullableLong(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.GetInt64()
            : null;
    }

    private static double? GetNullableDouble(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.GetDouble()
            : null;
    }

    private static string? GetNullableString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.GetString()
            : null;
    }

    private static DateTime? GetNullableUnixDateTime(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var value) && value.ValueKind != JsonValueKind.Null)
        {
            var timestamp = value.GetInt64();
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }
        return null;
    }
}
