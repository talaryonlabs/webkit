using Microsoft.Extensions.DependencyInjection;
using Talaryon.WebKit.Services;

namespace Talaryon.WebKit;

/// <summary>
/// Extension methods for adding token authentication to WebKit applications.
/// </summary>
public static class WebKitTokenAuthenticationExtensions
{
    /// <summary>
    /// Adds token-based authentication services to the WebKit application.
    /// Note: This does not configure cookie authentication - the application must configure
    /// cookie authentication separately if needed.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure token authentication options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWebKitTokenAuthentication(
        this IServiceCollection services,
        Action<WebKitTokenAuthenticationService.WebKitTokenAuthenticationOptions>? configureOptions = null)
    {
        // Configure options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<WebKitTokenAuthenticationService.WebKitTokenAuthenticationOptions>(options =>
            {
                // Default empty configuration
            });
        }

        // Add required services
        services.AddHttpContextAccessor();

        // Register the token authentication service
        services.AddScoped<IWebKitTokenAuthenticationService, WebKitTokenAuthenticationService>();

        return services;
    }
}
