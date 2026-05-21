using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Talaryon.WebKit.Services;
using Talaryon.WebKit.Services.Options;

namespace Talaryon.WebKit;

/// <summary>
/// Extension methods for configuring rate limiting in WebKit applications.
/// Rate limiting is configured through the WebKit configuration in BuildWithWebKit.
/// </summary>
public static class WebKitRateLimitExtensions
{
    /// <summary>
    /// Configures rate limiting for WebKit.
    /// Call this inside the BuildWithWebKit configuration callback.
    /// </summary>
    /// <param name="webkit">The WebKit service.</param>
    /// <param name="maxRequests">Maximum requests per window. Default is 100.</param>
    /// <param name="windowSeconds">Time window in seconds. Default is 60.</param>
    /// <param name="message">Message to return when rate limited.</param>
    /// <returns>The WebKit service for chaining.</returns>
    public static IWebKit ConfigureRateLimiting(
        this IWebKit webkit,
        int maxRequests = 100,
        int windowSeconds = 60,
        string? message = null)
    {
        webkit.Configure<WebKitRateLimitOptions>(options =>
        {
            options.Enabled = true;
            options.MaxRequests = maxRequests;
            options.WindowSeconds = windowSeconds;
            if (message != null)
                options.Message = message;
        });
        return webkit;
    }

    /// <summary>
    /// Configures rate limiting for WebKit with full options.
    /// Call this inside the BuildWithWebKit configuration callback.
    /// </summary>
    /// <param name="webkit">The WebKit service.</param>
    /// <param name="optionsConfigurator">Configuration action for rate limit options.</param>
    /// <returns>The WebKit service for chaining.</returns>
    public static IWebKit ConfigureRateLimiting(
        this IWebKit webkit,
        Action<WebKitRateLimitOptions> optionsConfigurator)
    {
        optionsConfigurator(new WebKitRateLimitOptions { Enabled = true });
        webkit.Configure<WebKitRateLimitOptions>(optionsConfigurator);
        return webkit;
    }

    /// <summary>
    /// Adds rate limiting middleware to the application.
    /// This is called automatically by BuildWithWebKit if rate limiting is configured.
    /// </summary>
    /// <param name="app">The WebApplication.</param>
    /// <returns>The WebApplication for chaining.</returns>
    internal static WebApplication UseRateLimitingInternal(this WebApplication app)
    {
        var webkit = app.Services.GetService<IWebKit>();
        var rateLimitOptions = webkit?.GetOptions<WebKitRateLimitOptions>();

        if (rateLimitOptions == null || !rateLimitOptions.Enabled)
            return app;

        var rateLimitService = app.Services.GetService<IWebKitRateLimitService>()
            ?? throw new InvalidOperationException("IWebKitRateLimitService is not registered.");

        app.Use(async (context, next) =>
        {
            // Check if the request path should be excluded
            if (ShouldExcludePath(context.Request.Path, rateLimitOptions.ExcludedPaths))
            {
                await next(context);
                return;
            }

            // Get client identifier (IP address)
            var identifier = GetClientIdentifier(context);

            // Check rate limit
            if (!rateLimitService.IsAllowed(identifier, rateLimitOptions))
            {
                var retryAfter = rateLimitService.GetRetryAfterSeconds(identifier, rateLimitOptions);
                context.Response.StatusCode = rateLimitOptions.StatusCode;

                if (rateLimitOptions.IncludeRetryAfter && retryAfter > 0)
                {
                    context.Response.Headers.RetryAfter = retryAfter.ToString();
                }

                if (!string.IsNullOrEmpty(rateLimitOptions.Message))
                {
                    await context.Response.WriteAsync(rateLimitOptions.Message);
                }

                return;
            }

            await next(context);
        });

        return app;
    }

    private static bool ShouldExcludePath(PathString path, List<string>? excludedPaths)
    {
        if (excludedPaths == null || excludedPaths.Count == 0)
            return false;

        foreach (var excluded in excludedPaths)
        {
            if (excluded.EndsWith("/*"))
            {
                var prefix = excluded[..^2];
                if (path.StartsWithSegments(prefix))
                    return true;
            }
            else if (path.Equals(excluded, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Try to get the real IP address from forwarded headers
        var ipHeader = context.Request.Headers["X-Forwarded-For"];
        if (!StringValues.IsNullOrEmpty(ipHeader))
        {
            // X-Forwarded-For can contain multiple IPs, take the first one
            var firstIp = ipHeader.ToString().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrEmpty(firstIp))
                return firstIp;
        }

        // Fall back to Connection.RemoteIpAddress
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
