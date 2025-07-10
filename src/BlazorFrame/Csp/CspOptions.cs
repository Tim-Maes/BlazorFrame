namespace BlazorFrame;

/// <summary>
/// Content Security Policy configuration options for iframe security
/// </summary>
public class CspOptions
{
    /// <summary>
    /// List of allowed sources for child-src directive (controls iframe sources)
    /// </summary>
    public List<string> ChildSrc { get; set; } = new();

    /// <summary>
    /// List of allowed sources for frame-src directive (controls iframe sources - modern alternative to child-src)
    /// </summary>
    public List<string> FrameSrc { get; set; } = new();

    /// <summary>
    /// List of allowed sources for script-src directive
    /// </summary>
    public List<string> ScriptSrc { get; set; } = new();

    /// <summary>
    /// List of allowed sources for frame-ancestors directive (controls what can embed this page)
    /// </summary>
    public List<string> FrameAncestors { get; set; } = new();

    /// <summary>
    /// Whether to automatically derive frame-src from iframe Src URLs
    /// </summary>
    public bool AutoDeriveFrameSrc { get; set; } = true;

    /// <summary>
    /// Whether to include 'unsafe-inline' in script-src for BlazorFrame functionality
    /// </summary>
    public bool AllowInlineScripts { get; set; } = false;

    /// <summary>
    /// Whether to include 'unsafe-eval' in script-src
    /// </summary>
    public bool AllowEval { get; set; } = false;

    /// <summary>
    /// Nonce value for inline scripts (recommended over unsafe-inline)
    /// </summary>
    public string? ScriptNonce { get; set; }

    /// <summary>
    /// Whether to use strict-dynamic for script-src
    /// </summary>
    public bool UseStrictDynamic { get; set; } = false;

    /// <summary>
    /// Custom CSP directives not covered by the above options
    /// </summary>
    public Dictionary<string, List<string>> CustomDirectives { get; set; } = new();

    /// <summary>
    /// Whether to generate CSP in report-only mode
    /// </summary>
    public bool ReportOnly { get; set; } = false;

    /// <summary>
    /// URL to send CSP violation reports to
    /// </summary>
    public string? ReportUri { get; set; }
}