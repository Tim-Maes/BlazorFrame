using BlazorFrame.Models;
using System.Text.Json;

namespace BlazorFrame.Services;

/// <summary>
/// Service for validating iframe messages and origins
/// </summary>
internal class MessageValidationService
{
    /// <summary>
    /// Validates an iframe message against security rules
    /// </summary>
    /// <param name="origin">The origin of the message sender</param>
    /// <param name="messageJson">The message data as JSON</param>
    /// <param name="allowedOrigins">List of allowed origins</param>
    /// <param name="options">Security options</param>
    /// <returns>Validated message result</returns>
    public IframeMessage ValidateMessage(
        string origin, 
        string messageJson, 
        IReadOnlyList<string> allowedOrigins,
        MessageSecurityOptions options)
    {
        if (string.IsNullOrEmpty(origin))
        {
            return CreateInvalidMessage(origin, messageJson, "Origin is null or empty");
        }

        if (string.IsNullOrEmpty(messageJson))
        {
            return CreateInvalidMessage(origin, messageJson, "Message data is null or empty");
        }

        if (messageJson.Length > options.MaxMessageSize)
        {
            return CreateInvalidMessage(origin, messageJson, 
                $"Message size ({messageJson.Length} bytes) exceeds maximum allowed size ({options.MaxMessageSize} bytes)");
        }

        if (!IsOriginAllowed(origin, allowedOrigins))
        {
            return CreateInvalidMessage(origin, messageJson, 
                $"Origin '{origin}' is not in the allowed origins list");
        }

        string? messageType = null;
        if (options.EnableStrictValidation)
        {
            var validationResult = ValidateJsonStructure(messageJson);
            if (!validationResult.IsValid)
            {
                return CreateInvalidMessage(origin, messageJson, validationResult.Error);
            }
            messageType = validationResult.MessageType;
        }

        return new IframeMessage
        {
            Origin = origin,
            Data = messageJson,
            IsValid = true,
            ValidationError = null,
            MessageType = messageType
        };
    }

    /// <summary>
    /// Extracts the origin from a URL
    /// </summary>
    /// <param name="url">The URL to extract origin from</param>
    /// <returns>The origin (protocol + domain + port) or null if invalid</returns>
    public string? ExtractOrigin(string url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        try
        {
            if (url.StartsWith("/"))
                return null;

            if (url.StartsWith("data:"))
                return "data:";

            if (url.StartsWith("blob:"))
                return "blob:";

            var uri = new Uri(url);
            return uri.GetLeftPart(UriPartial.Authority);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsOriginAllowed(string origin, IReadOnlyList<string> allowedOrigins)
    {
        if (allowedOrigins.Count == 0)
            return false;

        return allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }

    private static (bool IsValid, string? Error, string? MessageType) ValidateJsonStructure(string messageJson)
    {
        try
        {
            using var document = JsonDocument.Parse(messageJson);
            var root = document.RootElement;

            string? messageType = null;
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("type", out var typeElement))
            {
                messageType = typeElement.GetString();
            }

            return (true, null, messageType);
        }
        catch (JsonException ex)
        {
            return (false, $"Invalid JSON format: {ex.Message}", null);
        }
    }

    private static IframeMessage CreateInvalidMessage(string origin, string messageJson, string error)
    {
        return new IframeMessage
        {
            Origin = origin ?? string.Empty,
            Data = messageJson ?? string.Empty,
            IsValid = false,
            ValidationError = error,
            MessageType = null
        };
    }
}