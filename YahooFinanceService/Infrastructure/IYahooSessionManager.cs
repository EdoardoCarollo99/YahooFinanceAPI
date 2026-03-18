using YahooFinanceService.Configuration;

namespace YahooFinanceService.Infrastructure;

/// <summary>
/// Manages Yahoo Finance session including crumb and cookie authentication.
/// </summary>
public interface IYahooSessionManager
{
    /// <summary>
    /// Gets the current session data, initializing if necessary.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The session data containing crumb and cookie.</returns>
    /// <exception cref="Exceptions.SessionException">Thrown when session initialization fails.</exception>
    Task<YahooSessionData> GetSessionDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Forces re-initialization of the session.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new session data.</returns>
    Task<YahooSessionData> RefreshSessionAsync(CancellationToken cancellationToken = default);
}
