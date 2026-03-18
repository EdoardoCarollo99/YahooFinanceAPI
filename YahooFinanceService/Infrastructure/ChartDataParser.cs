using System.Text.Json;
using YahooFinanceService.Domain;

namespace YahooFinanceService.Infrastructure;

/// <summary>
/// Parser for Yahoo Finance chart data responses.
/// </summary>
public static class ChartDataParser
{
    /// <summary>
    /// Parses historical candle data from Yahoo Finance chart response.
    /// </summary>
    public static IReadOnlyList<Candle> ParseCandles(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!TryGetChartResult(root, out var result))
        {
            return [];
        }

        var timestamps = result.GetProperty("timestamp").EnumerateArray()
            .Select(t => DateTimeOffset.FromUnixTimeSeconds(t.GetInt64()).DateTime)
            .ToArray();

        var indicators = result.GetProperty("indicators");
        var quote = indicators.GetProperty("quote")[0];
        var adjClose = indicators.GetProperty("adjclose")[0].GetProperty("adjclose");

        var opens = GetDecimalArray(quote, "open");
        var highs = GetDecimalArray(quote, "high");
        var lows = GetDecimalArray(quote, "low");
        var closes = GetDecimalArray(quote, "close");
        var volumes = GetLongArray(quote, "volume");
        var adjCloses = GetDecimalArrayDirect(adjClose);

        var candles = new List<Candle>();
        for (int i = 0; i < timestamps.Length; i++)
        {
            if (opens[i] == null || highs[i] == null || lows[i] == null || 
                closes[i] == null || volumes[i] == null || adjCloses[i] == null)
            {
                continue;
            }

            candles.Add(new Candle
            {
                DateTime = timestamps[i],
                Open = opens[i]!.Value,
                High = highs[i]!.Value,
                Low = lows[i]!.Value,
                Close = closes[i]!.Value,
                Volume = volumes[i]!.Value,
                AdjustedClose = adjCloses[i]!.Value
            });
        }

        return candles;
    }

    /// <summary>
    /// Parses dividend data from Yahoo Finance chart response.
    /// </summary>
    public static IReadOnlyList<DividendTick> ParseDividends(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!TryGetChartResult(root, out var result) || 
            !result.TryGetProperty("events", out var events) ||
            !events.TryGetProperty("dividends", out var dividendsObj))
        {
            return [];
        }

        var dividends = new List<DividendTick>();
        foreach (var dividend in dividendsObj.EnumerateObject())
        {
            var data = dividend.Value;
            dividends.Add(new DividendTick
            {
                DateTime = DateTimeOffset.FromUnixTimeSeconds(data.GetProperty("date").GetInt64()).DateTime,
                Dividend = data.GetProperty("amount").GetDecimal()
            });
        }

        return dividends.OrderBy(d => d.DateTime).ToList();
    }

    /// <summary>
    /// Parses stock split data from Yahoo Finance chart response.
    /// </summary>
    public static IReadOnlyList<SplitTick> ParseSplits(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!TryGetChartResult(root, out var result) || 
            !result.TryGetProperty("events", out var events) ||
            !events.TryGetProperty("splits", out var splitsObj))
        {
            return [];
        }

        var splits = new List<SplitTick>();
        foreach (var split in splitsObj.EnumerateObject())
        {
            var data = split.Value;
            splits.Add(new SplitTick
            {
                DateTime = DateTimeOffset.FromUnixTimeSeconds(data.GetProperty("date").GetInt64()).DateTime,
                BeforeSplit = data.GetProperty("denominator").GetDecimal(),
                AfterSplit = data.GetProperty("numerator").GetDecimal()
            });
        }

        return splits.OrderBy(s => s.DateTime).ToList();
    }

    private static bool TryGetChartResult(JsonElement root, out JsonElement result)
    {
        result = default;
        
        if (!root.TryGetProperty("chart", out var chart) ||
            !chart.TryGetProperty("result", out var results) ||
            results.GetArrayLength() == 0)
        {
            return false;
        }

        result = results[0];
        return true;
    }

    private static decimal?[] GetDecimalArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var array))
        {
            return [];
        }

        return array.EnumerateArray()
            .Select(e => e.ValueKind == JsonValueKind.Null ? null : (decimal?)e.GetDecimal())
            .ToArray();
    }

    private static decimal?[] GetDecimalArrayDirect(JsonElement array)
    {
        return array.EnumerateArray()
            .Select(e => e.ValueKind == JsonValueKind.Null ? null : (decimal?)e.GetDecimal())
            .ToArray();
    }

    private static long?[] GetLongArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var array))
        {
            return [];
        }

        return array.EnumerateArray()
            .Select(e => e.ValueKind == JsonValueKind.Null ? null : (long?)e.GetInt64())
            .ToArray();
    }
}
