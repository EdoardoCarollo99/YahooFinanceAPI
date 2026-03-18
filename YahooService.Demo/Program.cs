using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YahooFinanceService.Extensions;
using YahooFinanceService.Services;
using YahooService.Demo;

Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║        YahooService - Demo Console Application                 ║");
Console.WriteLine("║        Libreria .NET 10 per Yahoo Finance API                  ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// Configura Dependency Injection
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Registra YahooFinanceService con configurazione di default
        services.AddYahooFinance();
    })
    .Build();

// Ottieni il servizio
var yahooService = host.Services.GetRequiredService<IYahooFinanceService>();

// Crea l'istanza del demo runner
var demo = new DemoRunner(yahooService);

// Menu principale
bool continua = true;
while (continua)
{
    Console.WriteLine();
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("                    MENU PRINCIPALE");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine();
    Console.WriteLine("  1. 📊 Dati Storici (Historical Data)");
    Console.WriteLine("  2. 💰 Storia Dividendi (Dividend History)");
    Console.WriteLine("  3. 🔀 Storia Stock Split (Split History)");
    Console.WriteLine("  4. 📈 Quote in Tempo Reale - Singola (Single Quote)");
    Console.WriteLine("  5. 📊 Quote Multiple (Multiple Quotes)");
    Console.WriteLine("  6. 🔍 Ricerca Simbolo (Search Symbol)");
    Console.WriteLine("  7. 🎯 Test Completo (All Methods)");
    Console.WriteLine("  8. ⚠️  Test Gestione Errori (Error Handling)");
    Console.WriteLine();
    Console.WriteLine("  0. ❌ Esci");
    Console.WriteLine();
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.Write("\nScegli un'opzione: ");

    var scelta = Console.ReadLine();
    Console.WriteLine();

    try
    {
        switch (scelta)
        {
            case "1":
                await demo.TestHistoricalDataAsync();
                break;
            case "2":
                await demo.TestDividendsAsync();
                break;
            case "3":
                await demo.TestSplitsAsync();
                break;
            case "4":
                await demo.TestSingleQuoteAsync();
                break;
            case "5":
                await demo.TestMultipleQuotesAsync();
                break;
            case "6":
                await demo.TestSearchAsync();
                break;
            case "7":
                await demo.RunAllTestsAsync();
                break;
            case "8":
                await demo.TestErrorHandlingAsync();
                break;
            case "0":
                continua = false;
                Console.WriteLine("👋 Grazie per aver utilizzato YahooService Demo!");
                break;
            default:
                Console.WriteLine("⚠️  Opzione non valida. Riprova.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n❌ ERRORE: {ex.Message}");
        Console.ResetColor();
    }

    if (continua)
    {
        Console.WriteLine("\nPremi un tasto per continuare...");
        Console.ReadKey();
        Console.Clear();
    }
}
