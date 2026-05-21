namespace Talaryon.WebKit.Services.Options;

/// <summary>
/// Options for configuring rate limiting in WebKit applications.
/// </summary>
public class WebKitRateLimitOptions : IWebKitOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether rate limiting is enabled.
    /// Default is false.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of requests allowed per window.
    /// Default is 100.
    /// </summary>
    public int MaxRequests { get; set; } = 100;

    /// <summary>
    /// Gets or sets the time window in seconds for rate limiting.
    /// Default is 60 seconds (1 minute).
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the HTTP status code to return when rate limit is exceeded.
    /// Default is 429 (Too Many Requests).
    /// </summary>
    public int StatusCode { get; set; } = 429;

    /// <summary>
    /// Gets or sets the message to return when rate limit is exceeded.
    /// </summary>
    public string? Message { get; set; } = "Too many requests. Please try again later.";

    /// <summary>
    /// Gets or sets a value indicating whether to include the Retry-After header.
    /// Default is true.
    /// </summary>
    public bool IncludeRetryAfter { get; set; } = true;

    /// <summary>
    /// Gets or sets the paths to exclude from rate limiting (e.g., "/health", "/static/*").
    /// </summary>
    public List<string>? ExcludedPaths { get; set; } = new()
    {
        "/health",
        "/webkit/status-code/*",
        "/static/*",
        "/css/*",
        "/js/*",
        "/favicon.png"
    };
}
