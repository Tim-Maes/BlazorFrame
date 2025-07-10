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
