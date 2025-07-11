namespace BlazorFrame;

/// <summary>
/// Options for configuring iframe message security
/// </summary>
public class MessageSecurityOptions
{
    /// <summary>
    /// List of allowed origins. If null or empty, will auto-derive from Src URL.
    /// </summary>
    public List<string>? AllowedOrigins { get; set; }

    /// <summary>
    /// Whether to perform strict message format validation
    /// </summary>
    public bool EnableStrictValidation { get; set; } = true;

    /// <summary>
    /// Maximum message size in bytes to prevent DoS attacks
    /// </summary>
    public int MaxMessageSize { get; set; } = 64 * 1024; // 64KB

    /// <summary>
    /// Whether to log security violations for monitoring
    /// </summary>
    public bool LogSecurityViolations { get; set; } = true;

    /// <summary>
    /// Maximum JSON nesting depth allowed
    /// </summary>
    public int MaxJsonDepth { get; set; } = 10;

    /// <summary>
    /// Maximum number of properties allowed in a JSON object
    /// </summary>
    public int MaxObjectProperties { get; set; } = 100;

    /// <summary>
    /// Maximum number of elements allowed in a JSON array
    /// </summary>
    public int MaxArrayElements { get; set; } = 1000;

    /// <summary>
    /// Whether to allow JavaScript protocol URLs (javascript:, vbscript:, etc.)
    /// </summary>
    public bool AllowScriptProtocols { get; set; } = false;

    /// <summary>
    /// Custom validation function for additional security checks
    /// </summary>
    public Func<string, string, bool>? CustomValidator { get; set; }

    /// <summary>
    /// Iframe sandbox attributes for content isolation. 
    /// If null, no sandbox attribute is applied (maintains backward compatibility).
    /// Common values: "allow-scripts allow-same-origin", "allow-forms", etc.
    /// Takes precedence over SandboxPreset if both are set.
    /// </summary>
    public string? Sandbox { get; set; } = null;

    /// <summary>
    /// Predefined sandbox configuration preset.
    /// Ignored if Sandbox property is explicitly set.
    /// </summary>
    public SandboxPreset SandboxPreset { get; set; } = SandboxPreset.None;

    /// <summary>
    /// Enable automatic sandbox with safe defaults for iframe content isolation.
    /// When true, applies SandboxPreset.Basic unless Sandbox or SandboxPreset is explicitly set.
    /// </summary>
    public bool EnableSandbox { get; set; } = false;

    /// <summary>
    /// Require HTTPS for iframe sources to ensure transport security
    /// </summary>
    public bool RequireHttps { get; set; } = false;

    /// <summary>
    /// Allow insecure (HTTP) connections in development scenarios
    /// </summary>
    public bool AllowInsecureConnections { get; set; } = false;

    /// <summary>
    /// Gets the effective sandbox value based on configuration priority:
    /// 1. Explicit Sandbox property
    /// 2. SandboxPreset (if not None)
    /// 3. EnableSandbox with Basic preset
    /// 4. null (no sandbox)
    /// </summary>
    /// <returns>The sandbox attribute value to use, or null if no sandbox should be applied</returns>
    public string? GetEffectiveSandboxValue()
    {
        if (!string.IsNullOrEmpty(Sandbox))
            return Sandbox;

        if (SandboxPreset != SandboxPreset.None)
            return SandboxHelper.GetSandboxValue(SandboxPreset);

        if (EnableSandbox)
            return SandboxHelper.GetSandboxValue(SandboxPreset.Basic);

        return null;
    }
}