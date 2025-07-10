namespace BlazorFrame;

/// <summary>
/// Represents a built CSP header ready for use
/// </summary>
public class CspHeader
{
    /// <summary>
    /// The CSP header name (Content-Security-Policy or Content-Security-Policy-Report-Only)
    /// </summary>
    public required string HeaderName { get; init; }

    /// <summary>
    /// The CSP header value
    /// </summary>
    public required string HeaderValue { get; init; }

    /// <summary>
    /// Whether this is a report-only CSP
    /// </summary>
    public bool IsReportOnly { get; init; }

    /// <summary>
    /// The individual directives that make up this CSP
    /// </summary>
    public Dictionary<string, List<string>> Directives { get; init; } = new();
}