using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using YahooFinanceService.Configuration;
using YahooFinanceService.Infrastructure;
using YahooFinanceService.Services;

namespace YahooFinanceService.Extensions;

/// <summary>
/// Extension methods for configuring Yahoo Finance services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Yahoo Finance services to the service collection with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddYahooFinance();
    /// </code>
    /// </example>
    public static IServiceCollection AddYahooFinance(this IServiceCollection services)
    {
        return services.AddYahooFinance(YahooFinanceOptions.Default);
    }

    /// <summary>
    /// Adds Yahoo Finance services to the service collection with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure Yahoo Finance options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddYahooFinance(options => new YahooFinanceOptions
    /// {
    ///     BaseUrl = "https://query1.finance.yahoo.com",
    ///     CookieUrl = "https://fc.yahoo.com",
    ///     CrumbUrl = "https://query1.finance.yahoo.com/v1/test/getcrumb",
    ///     UserAgent = "Mozilla/5.0...",
    ///     RequestTimeout = TimeSpan.FromSeconds(60),
    ///     MaxRetryAttempts = 5
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddYahooFinance(
        this IServiceCollection services,
        Func<YahooFinanceOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = configureOptions();
        return services.AddYahooFinance(options);
    }

    /// <summary>
    /// Adds Yahoo Finance services to the service collection with options instance.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The Yahoo Finance options instance.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// var options = YahooFinanceOptions.Default;
    /// services.AddYahooFinance(options);
    /// </code>
    /// </example>
    public static IServiceCollection AddYahooFinance(
        this IServiceCollection services,
        YahooFinanceOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.TryAddSingleton(Options.Create(options));
        services.TryAddSingleton<IYahooSessionManager, YahooSessionManager>();
        services.TryAddSingleton<IYahooClient, YahooClient>();
        services.TryAddSingleton<IYahooFinanceService, Services.YahooFinanceService>();

        services.AddHttpClient();

        return services;
    }
}

