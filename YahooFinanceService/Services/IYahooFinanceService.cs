using YahooFinanceService.Domain;

namespace YahooFinanceService.Services;

/// <summary>
/// Service for retrieving financial data from Yahoo Finance.
/// </summary>
public interface IYahooFinanceService
{
    /// <summary>
    /// Gets historical price data for a symbol.
    /// </summary>
    /// <param name="symbol">The stock symbol (e.g., "AAPL").</param>
    /// <param name="startDate">The start date for historical data. If null, defaults to 1 year ago.</param>
    /// <param name="endDate">The end date for historical data. If null, defaults to today.</param>
    /// <param name="period">The data period (Daily, Weekly, or Monthly).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of historical candles.</returns>
    /// <exception cref="Exceptions.InvalidSymbolException">Thrown when the symbol is invalid.</exception>
    /// <exception cref="Exceptions.RateLimitException">Thrown when rate limit is exceeded.</exception>
    Task<IReadOnlyList<Candle>> GetHistoricalAsync(
        string symbol,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Period period = Period.Daily,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets dividend history for a symbol.
    /// </summary>
    /// <param name="symbol">The stock symbol (e.g., "AAPL").</param>
    /// <param name="startDate">The start date. If null, defaults to 1 year ago.</param>
    /// <param name="endDate">The end date. If null, defaults to today.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of dividend payments.</returns>
    /// <exception cref="Exceptions.InvalidSymbolException">Thrown when the symbol is invalid.</exception>
    Task<IReadOnlyList<DividendTick>> GetDividendsAsync(
        string symbol,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets stock split history for a symbol.
    /// </summary>
    /// <param name="symbol">The stock symbol (e.g., "AAPL").</param>
    /// <param name="startDate">The start date. If null, defaults to 10 years ago.</param>
    /// <param name="endDate">The end date. If null, defaults to today.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of stock splits.</returns>
    /// <exception cref="Exceptions.InvalidSymbolException">Thrown when the symbol is invalid.</exception>
    Task<IReadOnlyList<SplitTick>> GetSplitsAsync(
        string symbol,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current quote data for one or more symbols.
    /// </summary>
    /// <param name="symbols">The stock symbols to query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dictionary of quotes keyed by symbol.</returns>
    /// <exception cref="Exceptions.InvalidSymbolException">Thrown when a symbol is invalid.</exception>
    Task<IReadOnlyDictionary<string, Quote>> GetQuotesAsync(
        IEnumerable<string> symbols,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current quote data for a single symbol.
    /// </summary>
    /// <param name="symbol">The stock symbol.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The quote data.</returns>
    Task<Quote?> GetQuoteAsync(
        string symbol,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for stock symbols matching a query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of search results.</returns>
    Task<IReadOnlyList<SearchResult>> SearchSymbolsAsync(
        string query,
        int maxResults = 10,
        CancellationToken cancellationToken = default);
}
