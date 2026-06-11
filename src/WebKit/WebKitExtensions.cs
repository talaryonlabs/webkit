using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
        services.AddSingleton<IWebKitRateLimitService, WebKitRateLimitService>();
        
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
        // Check if OIDC authentication is configured and set up authentication
        var hasOidc = builder.Services.Any(sd => sd.ServiceType == typeof(IWebKitOidcAuthenticationService));
        var hasToken = builder.Services.Any(sd => sd.ServiceType == typeof(IWebKitTokenAuthenticationService));
        
        if (hasOidc || hasToken)
        {
            var oidcOptions = builder.Services.BuildServiceProvider()
                .GetService<IOptions<WebKitOidcAuthenticationService.WebKitOidcAuthenticationOptions>>()?.Value;
            
            if (hasOidc && oidcOptions?.Enabled == true)
            {
                // Configure authentication with OIDC and Cookie schemes
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                    options.DefaultSignInScheme = "Cookies";
                    options.DefaultSignOutScheme = "Cookies";
                })
                .AddCookie("Cookies", cookieOptions =>
                {
                    cookieOptions.Cookie.Name = oidcOptions.CookieName;
                    cookieOptions.Cookie.SameSite = oidcOptions.SameSiteMode;
                    cookieOptions.ExpireTimeSpan = TimeSpan.FromHours(oidcOptions.CookieExpireHours);
                    cookieOptions.SlidingExpiration = oidcOptions.SlidingExpiration;
                    cookieOptions.LoginPath = new PathString("/login");
                    cookieOptions.AccessDeniedPath = new PathString("/login");
                    cookieOptions.ReturnUrlParameter = "ReturnUrl";
                })
                .AddOpenIdConnect("oidc", oidcAuthOptions =>
                {
                    oidcAuthOptions.Authority = oidcOptions.Authority;
                    oidcAuthOptions.ClientId = oidcOptions.ClientId;
                    oidcAuthOptions.ClientSecret = oidcOptions.ClientSecret;
                    oidcAuthOptions.ResponseType = oidcOptions.ResponseType;
                    oidcAuthOptions.CallbackPath = oidcOptions.CallbackPath;
                    oidcAuthOptions.SignedOutCallbackPath = oidcOptions.SignedOutCallbackPath;
                    oidcAuthOptions.RemoteSignOutPath = oidcOptions.RemoteSignOutPath;
                    oidcAuthOptions.SaveTokens = oidcOptions.SaveTokens;
                    oidcAuthOptions.GetClaimsFromUserInfoEndpoint = oidcOptions.GetClaimsFromUserInfoEndpoint;
                    oidcAuthOptions.RequireHttpsMetadata = oidcOptions.RequireHttpsMetadata;

                    // Configure scopes
                    foreach (var scope in oidcOptions.Scopes)
                    {
                        oidcAuthOptions.Scope.Add(scope);
                    }

                    // Claim actions
                    oidcAuthOptions.ClaimActions.MapJsonKey("sub", "sub");
                    oidcAuthOptions.ClaimActions.MapJsonKey("name", "name");
                    oidcAuthOptions.ClaimActions.MapJsonKey("email", "email");
                    oidcAuthOptions.ClaimActions.MapJsonKey("preferred_username", "preferred_username");

                    // Token validation
                    oidcAuthOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = oidcOptions.NameClaimType,
                        ValidateIssuer = true,
                        ValidIssuer = oidcAuthOptions.Authority,
                        ValidateAudience = true,
                        ValidAudience = oidcAuthOptions.ClientId
                    };
                });
                
                builder.Services.AddAuthorization();
                builder.Services.AddCascadingAuthenticationState();
            }
            else if (hasToken)
            {
                // Configure Cookie authentication for Token auth only
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "Cookies";
                    options.DefaultSignInScheme = "Cookies";
                    options.DefaultSignOutScheme = "Cookies";
                })
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = new PathString("/login");
                    options.AccessDeniedPath = new PathString("/login");
                    options.ReturnUrlParameter = "ReturnUrl";
                });
                
                builder.Services.AddAuthorization();
                builder.Services.AddCascadingAuthenticationState();
            }
        }
        
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
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
            return next();
        });

        // Add rate limiting middleware (internal - uses WebKit configuration)
        app.UseRateLimitingInternal();

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
                context.Context.Response.Headers["Cache-Control"] = "public,max-age=86400";
            }
        });

        // Add authentication and authorization middleware if OIDC or Token auth is configured
        if (app.Services.GetService<IWebKitTokenAuthenticationService>() != null
            || app.Services.GetService<IWebKitOidcAuthenticationService>() != null)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        // Health checks endpoint
        app.MapHealthChecks("/health");
        
        // Status code handling with configurable path
        app.UseStatusCodePagesWithReExecute(statusCodePath ?? DefaultStatusCodePath);

        app.MapStaticAssets();
        app.MapRazorComponents<TRootComponent>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(WebKitExtensions).Assembly);

        // Conditionally map authentication endpoints if AddWebKitTokenAuthentication or AddWebKitOidcAuthentication was called
        var hasTokenAuth = app.Services.GetService<IWebKitTokenAuthenticationService>() != null;
        var hasOidcAuth = app.Services.GetService<IWebKitOidcAuthenticationService>() != null;
        
        if (hasTokenAuth || hasOidcAuth)
        {
            // Token login endpoint (only if token auth is registered)
            if (hasTokenAuth)
            {
                app.MapGet("/webkit/login-token", async (
                    string token,
                    string? returnUrl,
                    Services.IWebKitTokenAuthenticationService tokenAuthService,
                    HttpContext httpContext) =>
                {
                    if (!tokenAuthService.ValidateToken(token))
                    {
                        return Results.BadRequest("Invalid token");
                    }

                    var success = await tokenAuthService.SignInWithTokenAsync(token, returnUrl);
                    if (!success)
                    {
                        return Results.BadRequest("Login failed");
                    }

                    return Results.Redirect(returnUrl ?? "/");
                });

                // Also map /login-token as an alias for backward compatibility
                app.MapGet("/login-token", (string token, string? returnUrl, HttpContext httpContext) =>
                    Results.Redirect($"/webkit/login-token?token={Uri.EscapeDataString(token)}&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}"));
            }

            // Logout endpoint - works for both Token and OIDC auth (both use cookie scheme "Cookies")
            app.MapGet("/webkit/logout", async (
                string? returnUrl,
                IAuthenticationService authService,
                HttpContext httpContext) =>
            {
                var properties = new AuthenticationProperties { RedirectUri = returnUrl ?? "/login" };
                await authService.SignOutAsync(httpContext, CookieAuthenticationDefaults.AuthenticationScheme, properties);
                return Results.Redirect(returnUrl ?? "/login");
            });

            logger.LogDebug("WebKit authentication endpoints mapped: /webkit/login-token, /webkit/logout");
        }

        app.UseAntiforgery();
        
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