namespace BlazorFrame;

/// <summary>
/// Configuration options for iframe auto-resize behavior
/// </summary>
public class ResizeOptions
{
    /// <summary>
    /// Minimum height in pixels for the iframe. Default is 100.
    /// </summary>
    public int MinHeight { get; set; } = 100;

    /// <summary>
    /// Maximum height in pixels for the iframe. Default is 50000.
    /// </summary>
    public int MaxHeight { get; set; } = 50000;

    /// <summary>
    /// Resize polling interval in milliseconds (when ResizeObserver is not available). Default is 500.
    /// </summary>
    public int PollingInterval { get; set; } = 500;

    /// <summary>
    /// Whether to use ResizeObserver API when available. Default is true.
    /// </summary>
    public bool UseResizeObserver { get; set; } = true;

    /// <summary>
    /// Debounce delay in milliseconds to prevent excessive resize events. Default is 100.
    /// Set to 0 to disable debouncing.
    /// </summary>
    public int DebounceMs { get; set; } = 100;

    /// <summary>
    /// Creates the default resize options
    /// </summary>
    public static ResizeOptions Default => new();

    /// <summary>
    /// Creates resize options optimized for performance (less frequent updates)
    /// </summary>
    public static ResizeOptions Performance => new()
    {
        PollingInterval = 1000,
        DebounceMs = 250
    };

    /// <summary>
    /// Creates resize options optimized for responsiveness (more frequent updates)
    /// </summary>
    public static ResizeOptions Responsive => new()
    {
        PollingInterval = 250,
        DebounceMs = 50
    };

    /// <summary>
    /// Validates the resize options and returns any errors
    /// </summary>
    /// <returns>List of validation error messages, empty if valid</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (MinHeight < 0)
            errors.Add("MinHeight must be >= 0.");

        if (MaxHeight <= 0)
            errors.Add("MaxHeight must be > 0.");

        if (MinHeight >= MaxHeight)
            errors.Add($"MinHeight ({MinHeight}) must be less than MaxHeight ({MaxHeight}).");

        if (PollingInterval <= 0)
            errors.Add("PollingInterval must be > 0.");

        if (DebounceMs < 0)
            errors.Add("DebounceMs must be >= 0.");

        return errors;
    }
}