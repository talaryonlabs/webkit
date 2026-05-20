using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Talaryon.WebKit.Services;

/// <summary>
/// Service for handling token-based authentication in WebKit applications.
/// </summary>
public interface IWebKitTokenAuthenticationService
{
    /// <summary>
    /// Validates a token against the configured tokens list.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    bool ValidateToken(string token);

    /// <summary>
    /// Signs in a user with a valid token.
    /// </summary>
    /// <param name="token">The authentication token.</param>
    /// <param name="returnUrl">Optional return URL after sign-in.</param>
    /// <returns>True if sign-in was successful; otherwise, false.</returns>
    Task<bool> SignInWithTokenAsync(string token, string? returnUrl = null);

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    Task SignOutAsync();
}

/// <summary>
/// Default implementation of token authentication service for WebKit.
/// </summary>
public class WebKitTokenAuthenticationService : IWebKitTokenAuthenticationService
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptions<WebKitTokenAuthenticationOptions> _options;

    /// <summary>
    /// Options for token authentication.
    /// </summary>
    public class WebKitTokenAuthenticationOptions
    {
        /// <summary>
        /// List of valid authentication tokens.
        /// </summary>
        public List<string> Tokens { get; set; } = new();

        /// <summary>
        /// Name of the authentication cookie.
        /// </summary>
        public string CookieName { get; set; } = "webkit_token_auth";

        /// <summary>
        /// Expiration time in hours for the authentication cookie.
        /// </summary>
        public int ExpireHours { get; set; } = 8;

        /// <summary>
        /// Name of the authenticated user.
        /// </summary>
        public string UserName { get; set; } = "Application Admin";

        /// <summary>
        /// Role to assign to authenticated users.
        /// </summary>
        public string Role { get; set; } = "Administrator";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebKitTokenAuthenticationService"/> class.
    /// </summary>
    /// <param name="authenticationService">The authentication service.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="options">The token authentication options.</param>
    public WebKitTokenAuthenticationService(
        IAuthenticationService authenticationService,
        IHttpContextAccessor httpContextAccessor,
        IOptions<WebKitTokenAuthenticationOptions> options)
    {
        _authenticationService = authenticationService;
        _httpContextAccessor = httpContextAccessor;
        _options = options;
    }

    /// <inheritdoc/>
    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        return _options.Value.Tokens.Contains(token);
    }

    /// <inheritdoc/>
    public async Task<bool> SignInWithTokenAsync(string token, string? returnUrl = null)
    {
        if (!ValidateToken(token))
        {
            return false;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return false;
        }

        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, _options.Value.UserName),
            new(ClaimTypes.Role, _options.Value.Role),
            new("auth_type", "token")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(_options.Value.ExpireHours)
        };

        try
        {
            await _authenticationService.SignInAsync(
                httpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                properties);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task SignOutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return;

        var properties = new AuthenticationProperties { RedirectUri = "/login" };
        await _authenticationService.SignOutAsync(
            httpContext,
            CookieAuthenticationDefaults.AuthenticationScheme,
            properties);
    }
}
