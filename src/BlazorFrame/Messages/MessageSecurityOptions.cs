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
    /// Require HTTPS for iframe sources to ensure transport security.
    /// When true, HTTP URLs will be rejected unless AllowInsecureConnections is also true.
    /// </summary>
    public bool RequireHttps { get; set; } = false;

    /// <summary>
    /// Allow insecure (HTTP) connections even when RequireHttps is true.
    /// This is typically used for development scenarios.
    /// Has no effect when RequireHttps is false.
    /// </summary>
    public bool AllowInsecureConnections { get; set; } = false;

    /// <summary>
    /// Gets the effective sandbox value based on configuration priority:
    /// 1. If EnableSandbox is false, returns null (no sandbox)
    /// 2. Explicit Sandbox property (if not null or empty)
    /// 3. SandboxPreset (if not None)
    /// 4. EnableSandbox with Basic preset
    /// 5. null (no sandbox)
    /// </summary>
    /// <returns>The sandbox attribute value to use, or null if no sandbox should be applied</returns>
    public string? GetEffectiveSandboxValue()
    {
        // Priority 1: If sandbox is explicitly disabled, return null
        if (!EnableSandbox && string.IsNullOrEmpty(Sandbox) && SandboxPreset == SandboxPreset.None)
            return null;

        // Priority 2: Explicit sandbox value (if not null or empty)
        if (!string.IsNullOrEmpty(Sandbox))
            return Sandbox;

        // Priority 3: Sandbox preset (if not None)
        if (SandboxPreset != SandboxPreset.None)
            return SandboxHelper.GetSandboxValue(SandboxPreset);

        // Priority 4: EnableSandbox with basic preset
        if (EnableSandbox)
            return SandboxHelper.GetSandboxValue(SandboxPreset.Basic);

        // Priority 5: No sandbox
        return null;
    }

    /// <summary>
    /// Validates the current configuration for conflicts and issues
    /// </summary>
    /// <returns>Validation result with errors, warnings, and suggestions</returns>
    public ConfigurationValidationResult ValidateConfiguration()
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var suggestions = new List<string>();

        // Check for HTTPS configuration conflicts
        if (RequireHttps && AllowInsecureConnections)
        {
            warnings.Add("RequireHttps is true but AllowInsecureConnections is also true. This allows HTTP connections despite requiring HTTPS, which may be confusing.");
            suggestions.Add("Consider setting AllowInsecureConnections=false for production or RequireHttps=false for development.");
        }

        // Check for sandbox configuration conflicts
        if (!EnableSandbox && !string.IsNullOrEmpty(Sandbox))
        {
            warnings.Add("EnableSandbox is false but Sandbox property is set. The sandbox attribute will still be applied.");
            suggestions.Add("Set EnableSandbox=true if you want to use the Sandbox property, or clear the Sandbox property to disable sandboxing.");
        }

        if (!EnableSandbox && SandboxPreset != SandboxPreset.None)
        {
            warnings.Add("EnableSandbox is false but SandboxPreset is set. The sandbox preset will be ignored.");
            suggestions.Add("Set EnableSandbox=true to use the SandboxPreset, or set SandboxPreset=None.");
        }

        // Check for security configuration issues
        if (MaxMessageSize <= 0)
        {
            errors.Add("MaxMessageSize must be greater than 0.");
        }
        else if (MaxMessageSize > 10 * 1024 * 1024) // 10MB
        {
            warnings.Add($"MaxMessageSize ({MaxMessageSize:N0} bytes) is very large and may allow DoS attacks.");
            suggestions.Add("Consider reducing MaxMessageSize to 64KB-1MB for most applications.");
        }

        if (MaxJsonDepth <= 0)
        {
            errors.Add("MaxJsonDepth must be greater than 0.");
        }
        else if (MaxJsonDepth > 50)
        {
            warnings.Add($"MaxJsonDepth ({MaxJsonDepth}) is very high and may allow DoS attacks through deeply nested JSON.");
        }

        if (MaxObjectProperties <= 0)
        {
            errors.Add("MaxObjectProperties must be greater than 0.");
        }

        if (MaxArrayElements <= 0)
        {
            errors.Add("MaxArrayElements must be greater than 0.");
        }

        // Check for potentially insecure configurations
        if (AllowScriptProtocols)
        {
            warnings.Add("AllowScriptProtocols=true allows javascript: and vbscript: URLs, which can be dangerous.");
            suggestions.Add("Only enable AllowScriptProtocols if absolutely necessary and you trust the content source.");
        }

        if (!EnableStrictValidation)
        {
            warnings.Add("EnableStrictValidation=false disables JSON structure validation, reducing security.");
            suggestions.Add("Enable strict validation for production environments unless you have specific requirements.");
        }

        // Check for development vs production misconfigurations
        var isLikelyProduction = RequireHttps && EnableStrictValidation && !AllowInsecureConnections;
        var isLikelyDevelopment = AllowInsecureConnections || !RequireHttps;

        if (isLikelyProduction && SandboxPreset == SandboxPreset.None && string.IsNullOrEmpty(Sandbox))
        {
            suggestions.Add("For production environments, consider enabling sandbox protection for additional security.");
        }

        if (isLikelyDevelopment && RequireHttps)
        {
            suggestions.Add("For development environments, consider setting RequireHttps=false or AllowInsecureConnections=true.");
        }

        return new ConfigurationValidationResult
        {
            Errors = errors,
            Warnings = warnings,
            Suggestions = suggestions
        };
    }
}