namespace YahooFinanceService.Configuration;

/// <summary>
/// Represents Yahoo Finance session data containing authentication information.
/// </summary>
public sealed record YahooSessionData
{
    /// <summary>
    /// Gets the crumb token used for CSRF protection.
    /// </summary>
    public required string Crumb { get; init; }

    /// <summary>
    /// Gets the session cookie value.
    /// </summary>
    public required string CookieValue { get; init; }

    /// <summary>
    /// Gets the session cookie name.
    /// </summary>
    public required string CookieName { get; init; }

    /// <summary>
    /// Gets the timestamp when the session was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Determines whether the session is valid and not expired.
    /// </summary>
    /// <param name="maxAge">Maximum age of the session before it's considered expired.</param>
    /// <returns>True if the session is still valid; otherwise, false.</returns>
    public bool IsValid(TimeSpan maxAge)
    {
        return !string.IsNullOrWhiteSpace(Crumb) 
            && !string.IsNullOrWhiteSpace(CookieValue)
            && DateTimeOffset.UtcNow - CreatedAt < maxAge;
    }
}
