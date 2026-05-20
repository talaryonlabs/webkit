using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Talaryon.WebKit.Services;

namespace Talaryon.WebKit;

/// <summary>
/// Extension methods for adding OIDC authentication to WebKit applications.
/// </summary>
public static class WebKitOidcAuthenticationExtensions
{
    /// <summary>
    /// Adds OIDC authentication services to the WebKit application.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure OIDC authentication options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWebKitOidcAuthentication(
        this IServiceCollection services,
        Action<WebKitOidcAuthenticationService.WebKitOidcAuthenticationOptions>? configureOptions = null)
    {
        // Configure options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<WebKitOidcAuthenticationService.WebKitOidcAuthenticationOptions>(options =>
            {
                // Default empty configuration
            });
        }

        // Register the OIDC authentication service
        services.AddScoped<IWebKitOidcAuthenticationService, WebKitOidcAuthenticationService>();

        return services;
    }
}
