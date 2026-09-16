using System.Text.Json;
using System.Net;

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

        var messageSize = System.Text.Encoding.UTF8.GetByteCount(messageJson);
        if (messageSize > options.MaxMessageSize)
        {
            return CreateInvalidMessage(origin, messageJson, 
                $"Message size ({messageSize} bytes) exceeds maximum allowed size ({options.MaxMessageSize} bytes)");
        }

        var normalizedOrigin = NormalizeOrigin(origin) ?? (origin.Equals("null", StringComparison.OrdinalIgnoreCase) ? "null" : null);
        if (normalizedOrigin is null || !IsOriginAllowed(normalizedOrigin, allowedOrigins))
        {
            return CreateInvalidMessage(origin, messageJson, 
                $"Origin '{origin}' is not in the allowed origins list");
        }

        string? messageType = null;
        if (options.EnableStrictValidation)
        {
            var validationResult = ValidateJsonStructure(messageJson, options); // Pass options instead of hardcoded depth
            if (!validationResult.IsValid)
            {
                return CreateInvalidMessage(origin, messageJson, validationResult.Error);
            }
            messageType = validationResult.MessageType;
        }

        // Apply custom validator if provided
        if (options.CustomValidator != null)
        {
            try
            {
                if (!options.CustomValidator(normalizedOrigin, messageJson))
                {
                    return CreateInvalidMessage(origin, messageJson, "Custom validation failed");
                }
            }
            catch (Exception ex)
            {
                return CreateInvalidMessage(origin, messageJson, $"Custom validation error: {ex.Message}");
            }
        }

        return new IframeMessage
        {
            Origin = normalizedOrigin,
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
            if (url.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
            {
                var blobUrl = url[5..];
                return ExtractOrigin(blobUrl);
            }

            var uri = new Uri(url);
            if (!IsHttpScheme(uri.Scheme) || !string.IsNullOrEmpty(uri.UserInfo))
                return null;

            return NormalizeOrigin(uri);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validates a URL against security requirements
    /// </summary>
    /// <param name="url">The URL to validate</param>
    /// <param name="options">Security options containing validation rules</param>
    /// <returns>Validation result</returns>
    public (bool IsValid, string? ErrorMessage) ValidateUrl(string url, MessageSecurityOptions options)
    {
        if (string.IsNullOrEmpty(url))
            return (false, "URL cannot be null or empty");

        try
        {
            if (IsRelativeUrl(url))
            {
                if (url.StartsWith("//", StringComparison.Ordinal) && options.RequireHttps && !options.AllowInsecureConnections)
                    return (false, "Protocol-relative URLs cannot be verified as HTTPS. Use an explicit https:// URL.");

                return (true, null);
            }

            if (url.Equals("about:blank", StringComparison.OrdinalIgnoreCase))
                return (true, null);

            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return options.AllowDataUrls
                    ? (true, null)
                    : (false, "Data URLs are disabled. Set AllowDataUrls=true only for trusted, non-communicating content.");
            }

            if (url.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
            {
                if (!options.AllowBlobUrls)
                    return (false, "Blob URLs are disabled. Set AllowBlobUrls=true to allow browser-generated content.");

                var blobOrigin = ExtractOrigin(url);
                if (blobOrigin is null)
                    return (false, "The blob URL must contain an http or https origin.");

                if (options.RequireHttps && blobOrigin.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !options.AllowInsecureConnections)
                    return (false, "HTTPS is required but the blob URL belongs to an HTTP origin.");

                return (true, null);
            }

            var uri = new Uri(url, UriKind.Absolute);
            if (!IsHttpScheme(uri.Scheme) || !string.IsNullOrEmpty(uri.UserInfo))
                return (false, $"URL scheme '{uri.Scheme}' is not allowed. Allowed schemes: http, https, relative URLs, about:blank, and configured blob/data URLs.");

            // Check HTTPS requirement for HTTP URLs
            if (uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
            {
                if (options.RequireHttps)
                {
                    if (!options.AllowInsecureConnections)
                        return (false, "HTTPS is required but URL uses HTTP protocol. Set AllowInsecureConnections=true to allow HTTP in development scenarios.");
                    // If AllowInsecureConnections is true, allow HTTP even though HTTPS is required
                    // This is for development scenarios where HTTPS might not be available
                }
                // If RequireHttps is false, HTTP is always allowed
            }

            return (true, null);
        }
        catch (UriFormatException ex)
        {
            return (false, $"Invalid URL format: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"URL validation error: {ex.Message}");
        }
    }

    private bool IsOriginAllowed(string origin, IReadOnlyList<string> allowedOrigins)
    {
        if (allowedOrigins.Count == 0)
            return false;

        return allowedOrigins.Any(allowed =>
            string.Equals(allowed, "null", StringComparison.OrdinalIgnoreCase)
                ? origin == "null"
                : NormalizeOrigin(allowed) is { } normalized &&
                  string.Equals(normalized, origin, StringComparison.Ordinal));
    }

    private static (bool IsValid, string? Error, string? MessageType) ValidateJsonStructure(string messageJson, MessageSecurityOptions options)
    {
        try
        {
            var jsonOptions = new JsonDocumentOptions
            {
                MaxDepth = options.MaxJsonDepth, // Use configured max depth instead of hardcoded value
                AllowTrailingCommas = false
            };

            using var document = JsonDocument.Parse(messageJson, jsonOptions);
            var root = document.RootElement;

            if (!ValidateElementComplexity(root, 0, options))
            {
                return (false, "JSON structure is too complex or deeply nested", null);
            }

            string? messageType = null;
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("type", out var typeElement))
            {
                if (typeElement.ValueKind != JsonValueKind.String)
                    return (false, "Message type must be a string", null);

                messageType = typeElement.GetString();
                
                if (!string.IsNullOrEmpty(messageType) && !IsValidMessageType(messageType))
                {
                    return (false, $"Invalid message type: {messageType}", null);
                }
            }

            return (true, null, messageType);
        }
        catch (JsonException ex)
        {
            return (false, $"Invalid JSON format: {ex.Message}", null);
        }
    }

    private static bool ValidateElementComplexity(JsonElement element, int currentDepth, MessageSecurityOptions options)
    {
        if (currentDepth >= options.MaxJsonDepth)
            return false;

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                // Use configured MaxObjectProperties instead of hardcoded value
                var propertyCount = 0;
                foreach (var property in element.EnumerateObject())
                {
                    propertyCount++;
                    if (propertyCount > options.MaxObjectProperties)
                        return false;
                        
                    if (!ValidateElementComplexity(property.Value, currentDepth + 1, options))
                        return false;
                }
                break;

            case JsonValueKind.Array:
                // Use configured MaxArrayElements instead of hardcoded value
                if (element.GetArrayLength() > options.MaxArrayElements)
                    return false;
                    
                foreach (var item in element.EnumerateArray())
                {
                    if (!ValidateElementComplexity(item, currentDepth + 1, options))
                        return false;
                }
                break;
        }

        return true;
    }

    private static bool IsRelativeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || url.Any(char.IsControl) || url.Any(char.IsWhiteSpace))
            return false;

        return url.StartsWith("/", StringComparison.Ordinal)
            || url.StartsWith("./", StringComparison.Ordinal)
            || url.StartsWith("../", StringComparison.Ordinal)
            || url.StartsWith("?", StringComparison.Ordinal)
            || url.StartsWith("#", StringComparison.Ordinal)
            || url.StartsWith("//", StringComparison.Ordinal);
    }

    private static bool IsHttpScheme(string scheme)
    {
        return scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
            || scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
    }

    public string? NormalizeOrigin(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
            return null;

        if (origin.Equals("null", StringComparison.OrdinalIgnoreCase))
            return "null";

        return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            ? NormalizeOrigin(uri)
            : null;
    }

    private static string? NormalizeOrigin(Uri uri)
    {
        if (!IsHttpScheme(uri.Scheme) || !string.IsNullOrEmpty(uri.UserInfo))
            return null;

        var host = uri.Host;
        if (IPAddress.TryParse(host, out var address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            host = $"[{host}]";
        else
            host = uri.IdnHost.ToLowerInvariant();

        var origin = $"{uri.Scheme.ToLowerInvariant()}://{host}";
        if (!uri.IsDefaultPort)
            origin += $":{uri.Port}";

        return origin;
    }

    private static bool IsValidMessageType(string messageType)
    {
        return messageType.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.');
    }

    private static IframeMessage CreateInvalidMessage(string origin, string messageJson, string? error)
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
