namespace BlazorFrame;


/// <summary>
/// Configuration options for iframe auto-resize behavior
/// </summary>
public class ResizeOptions
{
    /// <summary>
    /// Minimum height for the iframe
    /// </summary>
    public int MinHeight { get; set; } = 100;

    /// <summary>
    /// Maximum height for the iframe
    /// </summary>
    public int MaxHeight { get; set; } = 50000;

    /// <summary>
    /// Resize polling interval in milliseconds (when ResizeObserver is not available)
    /// </summary>
    public int PollingInterval { get; set; } = 500;

    /// <summary>
    /// Whether to use ResizeObserver API when available
    /// </summary>
    public bool UseResizeObserver { get; set; } = true;
}