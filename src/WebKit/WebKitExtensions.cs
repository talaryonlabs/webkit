using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Talaryon.Toolbox.Extensions;
using Talaryon.WebKit.Services;
using Talaryon.WebKit.Services.Options;

namespace Talaryon.WebKit;

/// <summary>
/// Extension methods for configuring WebKit in ASP.NET Core applications.
/// </summary>
public static class WebKitExtensions
{
    private const string DefaultStatusCodePath = "/webkit/status-code/{0}";
    private const string DefaultErrorPath = "/Error";

    /// <summary>
    /// Adds WebKit components and services to the specified <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="optionsConfigurator">Optional configuration action for WebKit settings.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddWebKitComponents(this IServiceCollection services, Action<WebKitSettings>? optionsConfigurator = null)
    {
        if (optionsConfigurator is not null)
            services.AddSingleton<IWebKit, Services.WebKit, WebKitSettings>(optionsConfigurator);
        else
            services.AddSingleton<IWebKit, Services.WebKit>();
        
        services
            .AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
            
        services.AddHttpContextAccessor()
            .AddLocalization()
            .AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
        
        services.AddHealthChecks();
        
        services.AddScoped<IWebKitConfig, WebKitConfig>();
        services.AddScoped<IWebKitNavigation, WebKitNavigation>();
        services.AddScoped<IWebKitSession, WebKitSession>();
        
        return services;
    }
    
    /// <summary>
    /// Builds and configures the WebApplication with WebKit middleware.
    /// </summary>
    /// <typeparam name="TRootComponent">The root Razor component type.</typeparam>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to build.</param>
    /// <param name="optionsConfigurator">Optional configuration action for WebKit.</param>
    /// <param name="statusCodePath">Optional custom path for status code handling. Defaults to "/webkit/status-code/{0}".</param>
    /// <returns>The configured <see cref="WebApplication"/>. </returns>
    /// <exception cref="WebKitNotFound">Thrown when WebKit service is not registered.</exception>
    public static WebApplication BuildWithWebKit<TRootComponent>(
        this WebApplicationBuilder builder,
        Action<IWebKit>? optionsConfigurator = null,
        string? statusCodePath = null)
    {
        var app = builder.Build();
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(WebKitExtensions).FullName!);
        
        var webkit = app.Services.GetService<IWebKit>() ?? throw new WebKitNotFound();

        // Safe cast with proper null checking
        if (webkit is not Services.WebKit webKitService)
        {
            logger.LogError("WebKit service is not of expected type");
            throw new WebKitNotFound("WebKit service implementation not found");
        }
        webKitService.UseApplication(app);
        
        logger.LogInformation("WebKit application initialized");
        
        // Configure the WebKit
        optionsConfigurator?.Invoke(webkit);
        
        // Validate configured options
        ValidateWebKitOptions(webkit, logger);
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
            logger.LogDebug("WebAssembly debugging enabled for development");
        }
        else
        {
            app.UseExceptionHandler(DefaultErrorPath, createScopeForErrors: false);
            logger.LogInformation("Exception handler configured for production");
        }

        // Add security headers middleware
        app.Use((context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
            return next();
        });

        // Get i18n options before setting up pipeline
        var i18NOptions = webkit.GetOptions<WebKitI18NOptions>();
        
        // UseRequestLocalization must be first for localized static files
        app.UseRequestLocalization(i18NOptions?.RequestCultureProviders ?? ["de-AT"]);
        
        // UseResponseCompression must be before UseStaticFiles for compression to work
        app.UseResponseCompression();
        
        // Configure static files with cache headers
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = context =>
            {
                context.Context.Response.Headers["Cache-Control"] = "public,max-age=3600";
            }
        });
        
        app.UseAntiforgery();

        // Health checks endpoint
        app.MapHealthChecks("/health");
        
        // Status code handling with configurable path
        app.UseStatusCodePagesWithReExecute(statusCodePath ?? DefaultStatusCodePath);

        app.MapStaticAssets();
        app.MapRazorComponents<TRootComponent>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(WebKitExtensions).Assembly);

        logger.LogInformation("WebKit middleware pipeline configured");
        
        return app;
    }

    /// <summary>
    /// Validates configured WebKit options for common misconfigurations.
    /// </summary>
    /// <param name="webkit">The WebKit service instance.</param>
    /// <param name="logger">The logger for reporting validation issues.</param>
    private static void ValidateWebKitOptions(IWebKit webkit, ILogger logger)
    {
        var i18NOptions = webkit.GetOptions<WebKitI18NOptions>();
        if (i18NOptions == null)
        {
            logger.LogWarning("WebKitI18NOptions not configured, using defaults");
            return;
        }
        
        if (i18NOptions.RequestCultureProviders == null || i18NOptions.RequestCultureProviders.Length == 0)
        {
            logger.LogWarning("WebKitI18NOptions.RequestCultureProviders is empty, using default 'de-AT'");
        }
        
        if (i18NOptions.CultureInfo == null)
        {
            logger.LogWarning("WebKitI18NOptions.CultureInfo is null");
        }
    }

    /// <summary>
    /// Configures global WebKit options for the application.
    /// </summary>
    /// <typeparam name="T">The options type to configure.</typeparam>
    /// <param name="app">The <see cref="WebApplication"/>.</param>
    /// <param name="optionsConfigurator">The configuration action.</param>
    public static void ConfigureGlobal<T>(this WebApplication app, Action<T> optionsConfigurator) where T : IWebKitOptions =>
        app
            .Services
            .GetService<IWebKit>()?
            .Configure(optionsConfigurator);
}