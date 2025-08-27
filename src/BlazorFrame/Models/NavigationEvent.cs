namespace BlazorFrame.Models;

/// <summary>
/// Represents a navigation event from an iframe with URL details and query parameters
/// </summary>
public class NavigationEvent
{
    /// <summary>
    /// The complete URL that was navigated to
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// The pathname portion of the URL (without query parameters or hash)
    /// </summary>
    public required string Pathname { get; init; }

    /// <summary>
    /// The query string portion of the URL (including the '?' prefix)
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// The hash/fragment portion of the URL (including the '#' prefix)
    /// </summary>
    public string? Hash { get; init; }

    /// <summary>
    /// Parsed query parameters as key-value pairs
    /// </summary>
    public Dictionary<string, string> QueryParameters { get; init; } = new();

    /// <summary>
    /// The origin (protocol + domain + port) of the navigation target
    /// </summary>
    public required string Origin { get; init; }

    /// <summary>
    /// Timestamp when the navigation event occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Whether this navigation is to the same origin as the iframe source
    /// </summary>
    public bool IsSameOrigin { get; init; }

    /// <summary>
    /// The type of navigation event (load, pushstate, popstate, hashchange)
    /// </summary>
    public string NavigationType { get; init; } = "unknown";
}