namespace BlazorFrame;

/// <summary>
/// Represents a validated message received from an iframe
/// </summary>
public class IframeMessage
{
    /// <summary>
    /// The origin (protocol + domain + port) of the sender
    /// </summary>
    public required string Origin { get; init; }

    /// <summary>
    /// The raw message data as JSON string
    /// </summary>
    public required string Data { get; init; }

    /// <summary>
    /// Whether this message passed all security validations
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Any validation errors encountered
    /// </summary>
    public string? ValidationError { get; init; }

    /// <summary>
    /// The type of message (if specified in the data)
    /// </summary>
    public string? MessageType { get; init; }
}

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
}

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