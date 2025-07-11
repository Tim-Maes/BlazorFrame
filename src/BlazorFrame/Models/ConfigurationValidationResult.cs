namespace BlazorFrame;

/// <summary>
/// Result of configuration validation
/// </summary>
public class ConfigurationValidationResult
{
    /// <summary>
    /// Whether the configuration is valid (no errors)
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Configuration errors that prevent proper operation
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Configuration warnings that may indicate issues
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// Suggestions for improving the configuration
    /// </summary>
    public List<string> Suggestions { get; init; } = new();
}