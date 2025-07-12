using System.Text.Json;
using System.Text.RegularExpressions;

namespace BlazorFrame.Services;

/// <summary>
/// Service for validating iframe messages and origins
/// </summary>
internal class MessageValidationService
{
    private static readonly Regex JsonCommentRegex = new(@"\/\*[\s\S]*?\*\/|\/\/.*", RegexOptions.Compiled);
    private const int DefaultMaxDepth = 10;

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

        if (ContainsSuspiciousContent(messageJson))
        {
            return CreateInvalidMessage(origin, messageJson, "Message contains potentially malicious content");
        }

        if (!IsOriginAllowed(origin, allowedOrigins))
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
                if (!options.CustomValidator(origin, messageJson))
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
            
            // Validate URI scheme
            if (!IsValidScheme(uri.Scheme))
                return null;

            // Build origin manually to preserve explicit ports (even default ones)
            var origin = $"{uri.Scheme}://{uri.Host}";
            
            // Always include port if it was explicitly specified in the URL
            // This preserves explicit default ports (443 for HTTPS, 80 for HTTP)
            if (!uri.IsDefaultPort || url.Contains($":{uri.Port}"))
            {
                origin += $":{uri.Port}";
            }
                
            return origin;
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
            // Handle relative URLs - they inherit the parent page's protocol
            if (url.StartsWith("/"))
            {
                if (options.RequireHttps && !options.AllowInsecureConnections)
                {
                    // Note: We can't validate relative URLs without knowing the parent page protocol
                    // This is a limitation that should be documented
                    return (true, null); // Allow relative URLs but log a warning
                }
                return (true, null);
            }

            // Handle special schemes - these bypass normal HTTP/HTTPS validation
            if (url.StartsWith("data:"))
            {
                // Data URLs are considered safe as they don't make network requests
                return (true, null);
            }

            if (url.StartsWith("blob:"))
            {
                // Blob URLs are browser-generated and considered safe
                return (true, null);
            }

            var uri = new Uri(url);

            // Check script protocols first (more specific check)
            var scriptProtocols = new[] { "javascript", "vbscript", "livescript" };
            if (scriptProtocols.Contains(uri.Scheme.ToLowerInvariant()))
            {
                if (!options.AllowScriptProtocols)
                    return (false, $"Script protocol '{uri.Scheme}' is not allowed. Enable AllowScriptProtocols if this is intended.");
                else
                    return (true, null); // Script protocols are explicitly allowed
            }

            // Validate scheme is allowed (after script protocol check)
            if (!IsValidScheme(uri.Scheme))
                return (false, $"URL scheme '{uri.Scheme}' is not allowed. Allowed schemes: http, https, data, blob");

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

    private static bool IsOriginAllowed(string origin, IReadOnlyList<string> allowedOrigins)
    {
        if (allowedOrigins.Count == 0)
            return false;

        return allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
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

    private static bool ContainsSuspiciousContent(string messageJson)
    {
        var cleanJson = JsonCommentRegex.Replace(messageJson, "");
        
        var suspiciousPatterns = new[]
        {
            "<script",
            "javascript:",
            "vbscript:",
            "onload=",
            "onerror=",
            "eval(",
            "Function(",
            "setTimeout(",
            "setInterval("
        };

        return suspiciousPatterns.Any(pattern => 
            cleanJson.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsValidScheme(string scheme)
    {
        var validSchemes = new[] { "http", "https", "data", "blob" };
        return validSchemes.Contains(scheme.ToLowerInvariant());
    }

    private static bool IsValidMessageType(string messageType)
    {
        return messageType.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.');
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