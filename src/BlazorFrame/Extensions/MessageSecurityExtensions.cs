namespace BlazorFrame;

/// <summary>
/// Extension methods for easier MessageSecurityOptions configuration
/// </summary>
public static class MessageSecurityExtensions
{
    /// <summary>
    /// Configures basic sandbox with scripts and same-origin allowed
    /// </summary>
    /// <param name="options">Security options to configure</param>
    /// <returns>Configured security options for chaining</returns>
    public static MessageSecurityOptions WithBasicSandbox(this MessageSecurityOptions options)
    {
        options.SandboxPreset = SandboxPreset.Basic;
        return options;
    }

    /// <summary>
    /// Configures permissive sandbox with scripts, same-origin, forms, and popups allowed
    /// </summary>
    /// <param name="options">Security options to configure</param>
    /// <returns>Configured security options for chaining</returns>
    public static MessageSecurityOptions WithPermissiveSandbox(this MessageSecurityOptions options)
    {
        options.SandboxPreset = SandboxPreset.Permissive;
        return options;
    }

    /// <summary>
    /// Configures strict sandbox with limited permissions
    /// </summary>
    /// <param name="options">Security options to configure</param>
    /// <returns>Configured security options for chaining</returns>
    public static MessageSecurityOptions WithStrictSandbox(this MessageSecurityOptions options)
    {
        options.SandboxPreset = SandboxPreset.Strict;
        return options;
    }

    /// <summary>
    /// Configures paranoid sandbox with maximum isolation
    /// </summary>
    /// <param name="options">Security options to configure</param>
    /// <returns>Configured security options for chaining</returns>
    public static MessageSecurityOptions WithParanoidSandbox(this MessageSecurityOptions options)
    {
        options.SandboxPreset = SandboxPreset.Paranoid;
        return options;
    }

    /// <summary>
    /// Sets custom sandbox attributes
    /// </summary>
    /// <param name="options">Security options to configure</param>
    /// <param name="sandbox">Custom sandbox attribute value</param>
    /// <returns>Configured security options for chaining</returns>
    public static MessageSecurityOptions WithCustomSandbox(this MessageSecurityOptions options, string sandbox)
    {
        options.Sandbox = sandbox;
        return options;
    }

    /// <summary>
    /// Disables sandbox (removes all sandbox restrictions)
    /// </summary>
    /// <param name="options">Security options to configure</param>
    /// <returns>Configured security options for chaining</returns>
    public static MessageSecurityOptions WithoutSandbox(this MessageSecurityOptions options)
    {
        options.SandboxPreset = SandboxPreset.None;
        options.EnableSandbox = false;
        options.Sandbox = null;
        return options;
    }

    /// <summary>
    /// Requires HTTPS for iframe sources
    /// </summary>
    /// <param name="options">Security options to configure</param>
    /// <param name="allowInsecureInDevelopment">Allow HTTP in development scenarios</param>
    /// <returns>Configured security options for chaining</returns>
    public static MessageSecurityOptions RequireHttps(this MessageSecurityOptions options, bool allowInsecureInDevelopment = false)
    {
        options.RequireHttps = true;
        options.AllowInsecureConnections = allowInsecureInDevelopment;
        return options;
    }

    /// <summary>
    /// Configures security options for development environments with relaxed restrictions
    /// </summary>
    /// <param name="options">Security options to configure</param>
    /// <returns>Configured security options for chaining</returns>
    public static MessageSecurityOptions ForDevelopment(this MessageSecurityOptions options)
    {
        options.EnableStrictValidation = false;
        options.LogSecurityViolations = true;
        options.AllowInsecureConnections = true;
        options.RequireHttps = false;
        options.SandboxPreset = SandboxPreset.Permissive;
        options.MaxMessageSize = 128 * 1024; // 128KB for development
        return options;
    }

    /// <summary>
    /// Configures security options for production environments with strict restrictions
    /// </summary>
    /// <param name="options">Security options to configure</param>
    /// <returns>Configured security options for chaining</returns>
    public static MessageSecurityOptions ForProduction(this MessageSecurityOptions options)
    {
        options.EnableStrictValidation = true;
        options.LogSecurityViolations = true;
        options.AllowInsecureConnections = false;
        options.RequireHttps = true;
        options.SandboxPreset = SandboxPreset.Strict;
        options.MaxMessageSize = 32 * 1024; // 32KB for production
        options.AllowScriptProtocols = false;
        return options;
    }

    /// <summary>
    /// Creates security options preset for payment widgets with maximum security
    /// </summary>
    /// <param name="options">Security options to configure</param>
    /// <returns>Configured security options for chaining</returns>
    public static MessageSecurityOptions ForPaymentWidget(this MessageSecurityOptions options)
    {
        return options
            .ForProduction()
            .WithStrictSandbox()
            .RequireHttps(false);
    }

    /// <summary>
    /// Creates security options preset for trusted content with balanced security
    /// </summary>
    /// <param name="options">Security options to configure</param>
    /// <returns>Configured security options for chaining</returns>
    public static MessageSecurityOptions ForTrustedContent(this MessageSecurityOptions options)
    {
        options.EnableStrictValidation = true;
        options.SandboxPreset = SandboxPreset.Basic;
        options.RequireHttps = true;
        options.AllowInsecureConnections = false;
        options.MaxMessageSize = 64 * 1024; // 64KB
        return options;
    }
}