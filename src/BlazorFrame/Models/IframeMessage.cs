namespace BlazorFrame.Models;

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
}