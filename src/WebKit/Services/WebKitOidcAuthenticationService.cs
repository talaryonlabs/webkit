using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Talaryon.WebKit.Services;

/// <summary>
/// Marker service for OIDC authentication in WebKit applications.
/// The actual OIDC configuration is handled by WebKitExtensions.BuildWithWebKit.
/// </summary>
public interface IWebKitOidcAuthenticationService
{
}

/// <summary>
/// Default implementation of OIDC authentication marker service for WebKit.
/// </summary>
public class WebKitOidcAuthenticationService : IWebKitOidcAuthenticationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebKitOidcAuthenticationService"/> class.
    /// </summary>
    public WebKitOidcAuthenticationService()
    {
    }

    /// <summary>
    /// Options for OIDC authentication.
    /// </summary>
    public class WebKitOidcAuthenticationOptions
    {
        /// <summary>
        /// Whether OIDC authentication is enabled.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// The OIDC authority URL.
        /// </summary>
        public string Authority { get; set; } = "";

        /// <summary>
        /// The client ID for OIDC authentication.
        /// </summary>
        public string ClientId { get; set; } = "";

        /// <summary>
        /// The client secret for OIDC authentication.
        /// </summary>
        public string ClientSecret { get; set; } = "";

        /// <summary>
        /// The response type for OIDC authentication.
        /// </summary>
        public string ResponseType { get; set; } = "code";

        /// <summary>
        /// The callback path for OIDC authentication.
        /// </summary>
        public PathString CallbackPath { get; set; } = "/webkit/oidc/callback";

        /// <summary>
        /// The signed out callback path for OIDC authentication.
        /// </summary>
        public PathString SignedOutCallbackPath { get; set; } = "/webkit/oidc/signout-callback";

        /// <summary>
        /// The remote sign out path for OIDC authentication.
        /// </summary>
        public PathString RemoteSignOutPath { get; set; } = "/webkit/oidc/remote-signout";

        /// <summary>
        /// Whether to save tokens in the authentication session.
        /// </summary>
        public bool SaveTokens { get; set; } = true;

        /// <summary>
        /// Whether to get claims from the user info endpoint.
        /// </summary>
        public bool GetClaimsFromUserInfoEndpoint { get; set; } = true;

        /// <summary>
        /// Whether to require HTTPS metadata.
        /// </summary>
        public bool RequireHttpsMetadata { get; set; } = true;

        /// <summary>
        /// List of scopes to request.
        /// </summary>
        public List<string> Scopes { get; set; } = new() { "openid", "profile", "email" };

        /// <summary>
        /// The name claim type.
        /// </summary>
        public string NameClaimType { get; set; } = "name";

        /// <summary>
        /// Name of the authentication cookie.
        /// </summary>
        public string CookieName { get; set; } = "webkit_auth";

        /// <summary>
        /// Expiration time in hours for the authentication cookie.
        /// </summary>
        public int CookieExpireHours { get; set; } = 8;

        /// <summary>
        /// Whether sliding expiration is enabled for the cookie.
        /// </summary>
        public bool SlidingExpiration { get; set; } = true;

        /// <summary>
        /// SameSite mode for the authentication cookie.
        /// </summary>
        public SameSiteMode SameSiteMode { get; set; } = SameSiteMode.Lax;
    }
}
