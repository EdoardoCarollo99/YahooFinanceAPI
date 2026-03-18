using YahooFinanceService.Domain;
using YahooFinanceService.Exceptions;
using YahooFinanceService.Services;

namespace YahooService.Demo;

/// <summary>
/// Classe che esegue i test dimostrativi per tutte le funzionalità di YahooFinanceService.
/// </summary>
public class DemoRunner
{
    private readonly IYahooFinanceService _yahooService;

    public DemoRunner(IYahooFinanceService yahooService)
    {
        _yahooService = yahooService;
    }

    /// <summary>
    /// Test 1: Recupera dati storici di prezzo (OHLCV) per un simbolo.
    /// </summary>
    public async Task TestHistoricalDataAsync()
    {
        PrintHeader("📊 TEST: DATI STORICI (Historical Data)");

        Console.Write("Inserisci il simbolo (default: AAPL): ");
        var symbol = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(symbol)) symbol = "AAPL";

        Console.WriteLine("\nScegli il periodo:");
        Console.WriteLine("  1. Ultimi 7 giorni");
        Console.WriteLine("  2. Ultimo mese");
        Console.WriteLine("  3. Ultimi 3 mesi");
        Console.WriteLine("  4. Ultimo anno");
        Console.Write("\nScelta (default: 2): ");
        var periodoScelta = Console.ReadLine();

        var (startDate, endDate) = periodoScelta switch
        {
            "1" => (DateTime.UtcNow.AddDays(-7), DateTime.UtcNow),
            "3" => (DateTime.UtcNow.AddMonths(-3), DateTime.UtcNow),
            "4" => (DateTime.UtcNow.AddYears(-1), DateTime.UtcNow),
            _ => (DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow)
        };

        Console.WriteLine("\nScegli l'intervallo:");
        Console.WriteLine("  1. Giornaliero (Daily)");
        Console.WriteLine("  2. Settimanale (Weekly)");
        Console.WriteLine("  3. Mensile (Monthly)");
        Console.Write("\nScelta (default: 1): ");
        var intervalloScelta = Console.ReadLine();

        var period = intervalloScelta switch
        {
            "2" => Period.Weekly,
            "3" => Period.Monthly,
            _ => Period.Daily
        };

        Console.WriteLine($"\n🔄 Recupero dati storici per {symbol.ToUpper()}...");
        Console.WriteLine($"   Periodo: {startDate:yyyy-MM-dd} → {endDate:yyyy-MM-dd}");
        Console.WriteLine($"   Intervallo: {period}");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var candles = await _yahooService.GetHistoricalAsync(symbol, startDate, endDate, period);
        stopwatch.Stop();

        Console.WriteLine($"\n✅ Recuperati {candles.Count} record in {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine();

        if (candles.Count > 0)
        {
            // Mostra i primi 5 e gli ultimi 5
            var toShow = Math.Min(5, candles.Count);
            
            Console.WriteLine("┌────────────┬──────────┬──────────┬──────────┬──────────┬─────────────┬──────────┐");
            Console.WriteLine("│    Data    │   Open   │   High   │   Low    │  Close   │   Volume    │  Adj.Cl  │");
            Console.WriteLine("├────────────┼──────────┼──────────┼──────────┼──────────┼─────────────┼──────────┤");

            for (int i = 0; i < toShow; i++)
            {
                var c = candles[i];
                Console.WriteLine($"│ {c.DateTime:yyyy-MM-dd} │ {c.Open,8:F2} │ {c.High,8:F2} │ {c.Low,8:F2} │ {c.Close,8:F2} │ {c.Volume,11:N0} │ {c.AdjustedClose,8:F2} │");
            }

            if (candles.Count > 10)
            {
                Console.WriteLine("│     ...    │   ...    │   ...    │   ...    │   ...    │     ...     │   ...    │");
                
                for (int i = candles.Count - toShow; i < candles.Count; i++)
                {
                    var c = candles[i];
                    Console.WriteLine($"│ {c.DateTime:yyyy-MM-dd} │ {c.Open,8:F2} │ {c.High,8:F2} │ {c.Low,8:F2} │ {c.Close,8:F2} │ {c.Volume,11:N0} │ {c.AdjustedClose,8:F2} │");
                }
            }

            Console.WriteLine("└────────────┴──────────┴──────────┴──────────┴──────────┴─────────────┴──────────┘");

            // Statistiche
            var firstCandle = candles.First();
            var lastCandle = candles.Last();
            var priceChange = lastCandle.Close - firstCandle.Close;
            var priceChangePercent = (priceChange / firstCandle.Close) * 100;

            Console.WriteLine();
            Console.WriteLine("📈 STATISTICHE:");
            Console.WriteLine($"   Prezzo Iniziale: ${firstCandle.Close:F2} ({firstCandle.DateTime:yyyy-MM-dd})");
            Console.WriteLine($"   Prezzo Finale:   ${lastCandle.Close:F2} ({lastCandle.DateTime:yyyy-MM-dd})");
            Console.WriteLine($"   Variazione:      ${priceChange:+0.00;-0.00} ({priceChangePercent:+0.00;-0.00}%)");
            Console.WriteLine($"   Massimo:         ${candles.Max(c => c.High):F2}");
            Console.WriteLine($"   Minimo:          ${candles.Min(c => c.Low):F2}");
            Console.WriteLine($"   Volume Medio:    {candles.Average(c => c.Volume):N0}");
        }
        else
        {
            Console.WriteLine("⚠️  Nessun dato trovato per il periodo selezionato.");
        }
    }

    /// <summary>
    /// Test 2: Recupera la storia dei dividendi.
    /// </summary>
    public async Task TestDividendsAsync()
    {
        PrintHeader("💰 TEST: STORIA DIVIDENDI (Dividend History)");

        Console.Write("Inserisci il simbolo (default: MSFT): ");
        var symbol = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(symbol)) symbol = "MSFT";

        var startDate = DateTime.UtcNow.AddYears(-2);
        var endDate = DateTime.UtcNow;

        Console.WriteLine($"\n🔄 Recupero dividendi per {symbol.ToUpper()}...");
        Console.WriteLine($"   Periodo: {startDate:yyyy-MM-dd} → {endDate:yyyy-MM-dd}");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var dividends = await _yahooService.GetDividendsAsync(symbol, startDate, endDate);
        stopwatch.Stop();

        Console.WriteLine($"\n✅ Recuperati {dividends.Count} dividendi in {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine();

        if (dividends.Count > 0)
        {
            Console.WriteLine("┌──────────────┬──────────────┐");
            Console.WriteLine("│     Data     │  Dividendo   │");
            Console.WriteLine("├──────────────┼──────────────┤");

            foreach (var dividend in dividends)
            {
                Console.WriteLine($"│ {dividend.DateTime:yyyy-MM-dd}   │   ${dividend.Dividend,8:F4}  │");
            }

            Console.WriteLine("└──────────────┴──────────────┘");

            // Statistiche
            var totalDividends = dividends.Sum(d => d.Dividend);
            var avgDividend = dividends.Average(d => d.Dividend);
            var yearlyDividends = dividends
                .GroupBy(d => d.DateTime.Year)
                .Select(g => new { Year = g.Key, Total = g.Sum(d => d.Dividend) })
                .ToList();

            Console.WriteLine();
            Console.WriteLine("📊 STATISTICHE:");
            Console.WriteLine($"   Totale Dividendi:    ${totalDividends:F4}");
            Console.WriteLine($"   Media per Pagamento: ${avgDividend:F4}");
            Console.WriteLine($"   Numero Pagamenti:    {dividends.Count}");
            
            if (yearlyDividends.Count > 0)
            {
                Console.WriteLine("\n   Dividendi per Anno:");
                foreach (var year in yearlyDividends.OrderByDescending(y => y.Year))
                {
                    Console.WriteLine($"   - {year.Year}: ${year.Total:F4}");
                }
            }
        }
        else
        {
            Console.WriteLine("⚠️  Nessun dividendo trovato per questo simbolo.");
        }
    }

    /// <summary>
    /// Test 3: Recupera la storia degli stock split.
    /// </summary>
    public async Task TestSplitsAsync()
    {
        PrintHeader("🔀 TEST: STORIA STOCK SPLIT (Split History)");

        Console.Write("Inserisci il simbolo (default: TSLA): ");
        var symbol = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(symbol)) symbol = "TSLA";

        var startDate = DateTime.UtcNow.AddYears(-5);
        var endDate = DateTime.UtcNow;

        Console.WriteLine($"\n🔄 Recupero stock splits per {symbol.ToUpper()}...");
        Console.WriteLine($"   Periodo: {startDate:yyyy-MM-dd} → {endDate:yyyy-MM-dd}");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var splits = await _yahooService.GetSplitsAsync(symbol, startDate, endDate);
        stopwatch.Stop();

        Console.WriteLine($"\n✅ Recuperati {splits.Count} split in {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine();

        if (splits.Count > 0)
        {
            Console.WriteLine("┌──────────────┬──────────────┬──────────────┬──────────────┐");
            Console.WriteLine("│     Data     │    Prima     │     Dopo     │    Ratio     │");
            Console.WriteLine("├──────────────┼──────────────┼──────────────┼──────────────┤");

            foreach (var split in splits)
            {
                Console.WriteLine($"│ {split.DateTime:yyyy-MM-dd}   │      {split.BeforeSplit,5:F0}     │      {split.AfterSplit,5:F0}     │    {split.SplitRatio,-8}  │");
            }

            Console.WriteLine("└──────────────┴──────────────┴──────────────┴──────────────┘");

            Console.WriteLine();
            Console.WriteLine("📊 INFORMAZIONE:");
            Console.WriteLine("   Un split aumenta il numero di azioni riducendo il prezzo proporzionalmente.");
            Console.WriteLine("   Esempio: split 2:1 significa che ogni azione diventa 2 azioni al 50% del prezzo.");
        }
        else
        {
            Console.WriteLine("⚠️  Nessun split trovato per questo simbolo nel periodo selezionato.");
        }
    }

    /// <summary>
    /// Test 4: Recupera una quote in tempo reale per un singolo simbolo.
    /// </summary>
    public async Task TestSingleQuoteAsync()
    {
        PrintHeader("📈 TEST: QUOTE IN TEMPO REALE - SINGOLA (Single Quote)");

        Console.Write("Inserisci il simbolo (default: AAPL): ");
        var symbol = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(symbol)) symbol = "AAPL";

        Console.WriteLine($"\n🔄 Recupero quote per {symbol.ToUpper()}...");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var quote = await _yahooService.GetQuoteAsync(symbol);
        stopwatch.Stop();

        if (quote != null)
        {
            Console.WriteLine($"\n✅ Quote recuperata in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine();

            PrintQuoteDetails(quote);
        }
        else
        {
            Console.WriteLine($"\n⚠️  Quote non trovata per il simbolo {symbol.ToUpper()}");
        }
    }

    /// <summary>
    /// Test 5: Recupera quote multiple in una singola chiamata.
    /// </summary>
    public async Task TestMultipleQuotesAsync()
    {
        PrintHeader("📊 TEST: QUOTE MULTIPLE (Multiple Quotes)");

        Console.WriteLine("Inserisci i simboli separati da virgola (default: AAPL,MSFT,GOOGL,AMZN,TSLA): ");
        var input = Console.ReadLine();
        
        var symbols = string.IsNullOrWhiteSpace(input)
            ? new[] { "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA" }
            : input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        Console.WriteLine($"\n🔄 Recupero {symbols.Length} quote...");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var quotes = await _yahooService.GetQuotesAsync(symbols);
        stopwatch.Stop();

        Console.WriteLine($"\n✅ Recuperate {quotes.Count} quote in {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine();

        if (quotes.Count > 0)
        {
            Console.WriteLine("┌──────────┬────────────────────────────┬───────────┬────────────┬─────────────┬──────────────┐");
            Console.WriteLine("│  Simbolo │         Nome Completo      │   Prezzo  │  Variazione│   Volume    │  Market Cap  │");
            Console.WriteLine("├──────────┼────────────────────────────┼───────────┼────────────┼─────────────┼──────────────┤");

            foreach (var (symbol, quote) in quotes.OrderByDescending(q => q.Value.MarketCap))
            {
                var name = (quote.LongName ?? quote.ShortName ?? "N/A").PadRight(26);
                if (name.Length > 26) name = name.Substring(0, 23) + "...";

                var price = quote.RegularMarketPrice?.ToString("F2") ?? "N/A";
                var change = quote.RegularMarketChangePercent?.ToString("+0.00%;-0.00%") ?? "N/A";
                var volume = quote.RegularMarketVolume?.ToString("N0") ?? "N/A";
                var marketCap = FormatMarketCap(quote.MarketCap);

                Console.WriteLine($"│ {symbol,-8} │ {name} │ ${price,8} │ {change,10} │ {volume,11} │ {marketCap,12} │");
            }

            Console.WriteLine("└──────────┴────────────────────────────┴───────────┴────────────┴─────────────┴──────────────┘");
        }
        else
        {
            Console.WriteLine("⚠️  Nessuna quote trovata.");
        }
    }

    /// <summary>
    /// Test 6: Cerca simboli per nome o ticker.
    /// </summary>
    public async Task TestSearchAsync()
    {
        PrintHeader("🔍 TEST: RICERCA SIMBOLO (Search Symbol)");

        Console.Write("Inserisci il termine di ricerca (default: Tesla): ");
        var query = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(query)) query = "Tesla";

        Console.Write("Numero massimo di risultati (default: 10): ");
        var maxInput = Console.ReadLine();
        var maxResults = int.TryParse(maxInput, out var max) ? max : 10;

        Console.WriteLine($"\n🔄 Ricerca di '{query}'...");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var results = await _yahooService.SearchSymbolsAsync(query, maxResults);
        stopwatch.Stop();

        Console.WriteLine($"\n✅ Trovati {results.Count} risultati in {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine();

        if (results.Count > 0)
        {
            Console.WriteLine("┌──────────┬────────────────────────────┬──────────┬────────────────┬──────────────────────┐");
            Console.WriteLine("│  Simbolo │         Nome Lungo         │ Exchange │      Tipo      │       Settore        │");
            Console.WriteLine("├──────────┼────────────────────────────┼──────────┼────────────────┼──────────────────────┤");

            foreach (var result in results)
            {
                var longName = (result.LongName ?? result.ShortName ?? "N/A").PadRight(26);
                if (longName.Length > 26) longName = longName.Substring(0, 23) + "...";

                var exchange = (result.Exchange ?? "N/A").PadRight(8);
                if (exchange.Length > 8) exchange = exchange.Substring(0, 8);

                var type = (result.Type ?? "N/A").PadRight(14);
                if (type.Length > 14) type = type.Substring(0, 14);

                var sector = (result.Sector ?? "N/A").PadRight(20);
                if (sector.Length > 20) sector = sector.Substring(0, 17) + "...";

                Console.WriteLine($"│ {result.Symbol,-8} │ {longName} │ {exchange} │ {type} │ {sector} │");
            }

            Console.WriteLine("└──────────┴────────────────────────────┴──────────┴────────────────┴──────────────────────┘");
        }
        else
        {
            Console.WriteLine($"⚠️  Nessun risultato trovato per '{query}'.");
        }
    }

    /// <summary>
    /// Test 7: Esegue tutti i test in sequenza.
    /// </summary>
    public async Task RunAllTestsAsync()
    {
        PrintHeader("🎯 TEST COMPLETO: TUTTE LE FUNZIONALITÀ");

        Console.WriteLine("Questo test eseguirà tutte le funzionalità con simboli predefiniti.");
        Console.WriteLine("Attendere prego...");
        Console.WriteLine();

        // Test 1: Historical Data
        Console.WriteLine("1️⃣  Test Historical Data (AAPL - ultimo mese)...");
        var candles = await _yahooService.GetHistoricalAsync("AAPL", DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow, Period.Daily);
        Console.WriteLine($"   ✅ {candles.Count} candles recuperate");
        await Task.Delay(1000); // Pausa per evitare rate limiting

        // Test 2: Dividends
        Console.WriteLine("\n2️⃣  Test Dividends (MSFT - ultimi 2 anni)...");
        var dividends = await _yahooService.GetDividendsAsync("MSFT", DateTime.UtcNow.AddYears(-2), DateTime.UtcNow);
        Console.WriteLine($"   ✅ {dividends.Count} dividendi recuperati");
        await Task.Delay(1000);

        // Test 3: Splits
        Console.WriteLine("\n3️⃣  Test Splits (TSLA - ultimi 5 anni)...");
        var splits = await _yahooService.GetSplitsAsync("TSLA", DateTime.UtcNow.AddYears(-5), DateTime.UtcNow);
        Console.WriteLine($"   ✅ {splits.Count} split recuperati");
        await Task.Delay(1000);

        // Test 4: Single Quote
        Console.WriteLine("\n4️⃣  Test Single Quote (GOOGL)...");
        var quote = await _yahooService.GetQuoteAsync("GOOGL");
        Console.WriteLine($"   ✅ Quote recuperata: ${quote?.RegularMarketPrice:F2}");
        await Task.Delay(1000);

        // Test 5: Multiple Quotes
        Console.WriteLine("\n5️⃣  Test Multiple Quotes (AAPL, MSFT, GOOGL, AMZN)...");
        var quotes = await _yahooService.GetQuotesAsync(new[] { "AAPL", "MSFT", "GOOGL", "AMZN" });
        Console.WriteLine($"   ✅ {quotes.Count} quote recuperate");
        await Task.Delay(1000);

        // Test 6: Search
        Console.WriteLine("\n6️⃣  Test Search (Microsoft)...");
        var results = await _yahooService.SearchSymbolsAsync("Microsoft", 5);
        Console.WriteLine($"   ✅ {results.Count} risultati trovati");

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("✅ TUTTI I TEST COMPLETATI CON SUCCESSO!");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
    }

    /// <summary>
    /// Test 8: Test della gestione degli errori.
    /// </summary>
    public async Task TestErrorHandlingAsync()
    {
        PrintHeader("⚠️  TEST: GESTIONE ERRORI (Error Handling)");

        Console.WriteLine("Questo test dimostrerà come vengono gestiti gli errori.\n");

        // Test 1: Simbolo invalido
        Console.WriteLine("1️⃣  Test con simbolo invalido (XXXINVALIDXXX)...");
        try
        {
            var quote = await _yahooService.GetQuoteAsync("XXXINVALIDXXX");
            if (quote == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("   ⚠️  Nessun risultato (potrebbe essere valido se il simbolo non esiste)");
                Console.ResetColor();
            }
        }
        catch (InvalidSymbolException ex)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"   ✅ InvalidSymbolException catturata correttamente!");
            Console.WriteLine($"      Simbolo: {ex.Symbol}");
            Console.WriteLine($"      Messaggio: {ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"   ⚠️  Eccezione diversa: {ex.GetType().Name}");
            Console.WriteLine($"      Messaggio: {ex.Message}");
            Console.ResetColor();
        }

        await Task.Delay(1000);

        // Test 2: Date non valide
        Console.WriteLine("\n2️⃣  Test con date future (dovrebbe restituire lista vuota)...");
        try
        {
            var futureStart = DateTime.UtcNow.AddYears(1);
            var futureEnd = DateTime.UtcNow.AddYears(2);
            var candles = await _yahooService.GetHistoricalAsync("AAPL", futureStart, futureEnd, Period.Daily);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"   ✅ Gestito correttamente: {candles.Count} candles (lista vuota come previsto)");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"   ⚠️  Eccezione: {ex.GetType().Name}");
            Console.WriteLine($"      Messaggio: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("ℹ️  NOTE SULLA GESTIONE ERRORI:");
        Console.WriteLine();
        Console.WriteLine("  • InvalidSymbolException: Simbolo non trovato o invalido");
        Console.WriteLine("  • RateLimitException: Limite di richieste superato (HTTP 429)");
        Console.WriteLine("  • SessionException: Errore nell'autenticazione crumb/cookie");
        Console.WriteLine("  • YahooFinanceException: Errore generico dell'API");
        Console.WriteLine();
        Console.WriteLine("  La libreria implementa anche retry automatico configurabile.");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
    }

    // Helper methods

    private void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine($"  {title}");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
    }

    private void PrintQuoteDetails(Quote quote)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine($"║  {quote.Symbol,-10}  {(quote.LongName ?? quote.ShortName ?? "N/A"),-64} ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Prezzo:              ${quote.RegularMarketPrice,-10:F2}   Exchange: {quote.Exchange,-20} ║");
        Console.WriteLine($"║  Variazione:          {quote.RegularMarketChange,-10:+0.00;-0.00} ({quote.RegularMarketChangePercent:+0.00%;-0.00%})                      ║");
        Console.WriteLine($"║  Apertura:            ${quote.RegularMarketOpen,-10:F2}   Chiusura Prec: ${quote.RegularMarketPreviousClose,-10:F2}    ║");
        Console.WriteLine($"║  Max Giornata:        ${quote.RegularMarketDayHigh,-10:F2}   Min Giornata: ${quote.RegularMarketDayLow,-11:F2}   ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Bid/Ask:             ${quote.Bid,-10:F2} / ${quote.Ask,-10:F2}                             ║");
        Console.WriteLine($"║  Bid/Ask Size:        {quote.BidSize,-10:N0} / {quote.AskSize,-10:N0}                             ║");
        Console.WriteLine($"║  Volume:              {quote.RegularMarketVolume,-10:N0}                                       ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  52 Week High:        ${quote.FiftyTwoWeekHigh,-10:F2}                                       ║");
        Console.WriteLine($"║  52 Week Low:         ${quote.FiftyTwoWeekLow,-10:F2}                                       ║");
        Console.WriteLine($"║  50 Day Avg:          ${quote.FiftyDayAverage,-10:F2}                                       ║");
        Console.WriteLine($"║  200 Day Avg:         ${quote.TwoHundredDayAverage,-10:F2}                                       ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Market Cap:          {FormatMarketCap(quote.MarketCap),-68} ║");
        Console.WriteLine($"║  P/E Ratio:           {quote.TrailingPE,-10:F2} (trailing) / {quote.ForwardPE,-10:F2} (forward)      ║");
        Console.WriteLine($"║  Dividend Yield:      {quote.TrailingAnnualDividendYield,-10:P2}                                       ║");
        Console.WriteLine($"║  Dividend Rate:       ${quote.TrailingAnnualDividendRate,-10:F4}                                       ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
    }

    private string FormatMarketCap(long? marketCap)
    {
        if (!marketCap.HasValue) return "N/A";

        if (marketCap >= 1_000_000_000_000)
            return $"${marketCap / 1_000_000_000_000.0:F2}T";
        if (marketCap >= 1_000_000_000)
            return $"${marketCap / 1_000_000_000.0:F2}B";
        if (marketCap >= 1_000_000)
            return $"${marketCap / 1_000_000.0:F2}M";

        return $"${marketCap:N0}";
    }
}
