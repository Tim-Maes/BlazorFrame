namespace BlazorFrame;

/// <summary>
/// Common iframe sandbox presets for different security scenarios
/// </summary>
public enum SandboxPreset
{
    /// <summary>
    /// No sandbox restrictions (default behavior)
    /// </summary>
    None,

    /// <summary>
    /// Basic sandbox: allow-scripts allow-same-origin
    /// Suitable for trusted content that needs script execution
    /// </summary>
    Basic,

    /// <summary>
    /// Permissive sandbox: allow-scripts allow-same-origin allow-forms allow-popups
    /// Suitable for interactive widgets and forms
    /// </summary>
    Permissive,

    /// <summary>
    /// Strict sandbox: allow-scripts allow-same-origin (no forms, no popups)
    /// Suitable for display-only content with limited interaction
    /// </summary>
    Strict,

    /// <summary>
    /// Paranoid sandbox: allow-scripts only (different origin, no forms, no popups)
    /// Maximum isolation for untrusted content
    /// </summary>
    Paranoid
}

/// <summary>
/// Helper class for working with iframe sandbox attributes
/// </summary>
public static class SandboxHelper
{
    /// <summary>
    /// Gets the sandbox attribute value for a given preset
    /// </summary>
    /// <param name="preset">The sandbox preset to use</param>
    /// <returns>The sandbox attribute value or null for None</returns>
    public static string? GetSandboxValue(SandboxPreset preset)
    {
        return preset switch
        {
            SandboxPreset.None => null,
            SandboxPreset.Basic => "allow-scripts allow-same-origin",
            SandboxPreset.Permissive => "allow-scripts allow-same-origin allow-forms allow-popups",
            SandboxPreset.Strict => "allow-scripts allow-same-origin",
            SandboxPreset.Paranoid => "allow-scripts",
            _ => null
        };
    }

    /// <summary>
    /// Creates a sandbox value with custom permissions
    /// </summary>
    /// <param name="allowScripts">Allow JavaScript execution</param>
    /// <param name="allowSameOrigin">Allow same-origin access</param>
    /// <param name="allowForms">Allow form submission</param>
    /// <param name="allowPopups">Allow popups</param>
    /// <param name="allowModals">Allow modal dialogs</param>
    /// <param name="allowPointerLock">Allow pointer lock API</param>
    /// <param name="allowPresentation">Allow presentation API</param>
    /// <returns>Custom sandbox attribute value</returns>
    public static string CreateCustomSandbox(
        bool allowScripts = true,
        bool allowSameOrigin = true,
        bool allowForms = false,
        bool allowPopups = false,
        bool allowModals = false,
        bool allowPointerLock = false,
        bool allowPresentation = false)
    {
        var permissions = new List<string>();

        if (allowScripts) permissions.Add("allow-scripts");
        if (allowSameOrigin) permissions.Add("allow-same-origin");
        if (allowForms) permissions.Add("allow-forms");
        if (allowPopups) permissions.Add("allow-popups");
        if (allowModals) permissions.Add("allow-modals");
        if (allowPointerLock) permissions.Add("allow-pointer-lock");
        if (allowPresentation) permissions.Add("allow-presentation");

        return string.Join(" ", permissions);
    }
}