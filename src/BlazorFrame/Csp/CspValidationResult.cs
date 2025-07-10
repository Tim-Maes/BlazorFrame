namespace BlazorFrame;

/// <summary>
/// Represents CSP validation result
/// </summary>
public class CspValidationResult
{
    /// <summary>
    /// Whether the CSP configuration is valid
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// List of validation warnings
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// List of validation errors
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Suggestions for improving the CSP configuration
    /// </summary>
    public List<string> Suggestions { get; init; } = new();
}