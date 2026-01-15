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
}