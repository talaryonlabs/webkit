using System.Collections.Concurrent;
using Talaryon.WebKit.Services.Options;

namespace Talaryon.WebKit.Services;

/// <summary>
/// Service for tracking and enforcing rate limits in WebKit applications.
/// Uses a sliding window algorithm for accurate rate limiting.
/// </summary>
public interface IWebKitRateLimitService
{
    /// <summary>
    /// Checks if the current request should be rate limited.
    /// </summary>
    /// <param name="identifier">The client identifier (e.g., IP address).</param>
    /// <param name="options">The rate limit options.</param>
    /// <returns>True if the request is allowed; false if rate limited.</returns>
    bool IsAllowed(string identifier, WebKitRateLimitOptions options);

    /// <summary>
    /// Gets the remaining number of requests allowed for the given identifier.
    /// </summary>
    /// <param name="identifier">The client identifier.</param>
    /// <param name="options">The rate limit options.</param>
    /// <returns>The number of remaining requests.</returns>
    int GetRemainingRequests(string identifier, WebKitRateLimitOptions options);

    /// <summary>
    /// Gets the time in seconds until the rate limit resets.
    /// </summary>
    /// <param name="identifier">The client identifier.</param>
    /// <param name="options">The rate limit options.</param>
    /// <returns>Seconds until reset.</returns>
    int GetRetryAfterSeconds(string identifier, WebKitRateLimitOptions options);
}

/// <summary>
/// Implementation of <see cref="IWebKitRateLimitService"/> using a sliding window algorithm.
/// </summary>
public class WebKitRateLimitService : IWebKitRateLimitService, IDisposable
{
    private readonly ConcurrentDictionary<string, RequestTracker> _trackers = new();
    private readonly Timer? _cleanupTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebKitRateLimitService"/> class.
    /// </summary>
    public WebKitRateLimitService()
    {
        // Set up periodic cleanup of old trackers
        _cleanupTimer = new Timer(CleanupOldTrackers, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    /// <inheritdoc />
    public bool IsAllowed(string identifier, WebKitRateLimitOptions options)
    {
        if (!options.Enabled)
            return true;

        var tracker = _trackers.GetOrAdd(identifier, _ => new RequestTracker(options));
        return tracker.IsAllowed();
    }

    /// <inheritdoc />
    public int GetRemainingRequests(string identifier, WebKitRateLimitOptions options)
    {
        if (!options.Enabled)
            return int.MaxValue;

        if (_trackers.TryGetValue(identifier, out var tracker))
            return tracker.GetRemainingRequests();

        return options.MaxRequests;
    }

    /// <inheritdoc />
    public int GetRetryAfterSeconds(string identifier, WebKitRateLimitOptions options)
    {
        if (!options.Enabled)
            return 0;

        if (_trackers.TryGetValue(identifier, out var tracker))
            return tracker.GetRetryAfterSeconds();

        return 0;
    }

    private void CleanupOldTrackers(object? state)
    {
        var now = DateTime.UtcNow;
        var keysToRemove = _trackers
            .Where(kvp => now - kvp.Value.LastRequestTime > TimeSpan.FromSeconds(120)) // 2 minutes
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _trackers.TryRemove(key, out _);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cleanupTimer?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Tracks request timestamps for a single client using a sliding window algorithm.
    /// </summary>
    private class RequestTracker
    {
        private readonly LinkedList<DateTime> _requestTimestamps = new();
        private readonly int _maxRequests;
        private readonly int _windowSeconds;
        private readonly object _lock = new();

        public DateTime LastRequestTime { get; private set; } = DateTime.UtcNow;

        public RequestTracker(WebKitRateLimitOptions options)
        {
            _maxRequests = options.MaxRequests;
            _windowSeconds = options.WindowSeconds;
        }

        public bool IsAllowed()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                LastRequestTime = now;

                // Remove timestamps outside the current window
                var cutoff = now.AddSeconds(-_windowSeconds);
                while (_requestTimestamps.Count > 0 && _requestTimestamps.First!.Value < cutoff)
                {
                    _requestTimestamps.RemoveFirst();
                }

                // Check if we're under the limit
                if (_requestTimestamps.Count < _maxRequests)
                {
                    _requestTimestamps.AddLast(now);
                    return true;
                }

                return false;
            }
        }

        public int GetRemainingRequests()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var cutoff = now.AddSeconds(-_windowSeconds);

                // Remove old timestamps
                while (_requestTimestamps.Count > 0 && _requestTimestamps.First!.Value < cutoff)
                {
                    _requestTimestamps.RemoveFirst();
                }

                var remaining = Math.Max(0, _maxRequests - _requestTimestamps.Count);
                return remaining;
            }
        }

        public int GetRetryAfterSeconds()
        {
            lock (_lock)
            {
                if (_requestTimestamps.Count < _maxRequests)
                    return 0;

                // The oldest request in the window determines when we can make another request
                var oldestInWindow = _requestTimestamps.First!.Value;
                var windowEnd = oldestInWindow.AddSeconds(_windowSeconds);
                var retryAfter = (int)Math.Ceiling((windowEnd - DateTime.UtcNow).TotalSeconds);
                return Math.Max(0, retryAfter);
            }
        }
    }
}
