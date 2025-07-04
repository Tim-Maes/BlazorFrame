# BlazorFrame

A Blazor component that provides an enhanced iframe wrapper with automatic resizing, cross-frame communication, and seamless JavaScript interop with built-in security features.

## Features

- **Security-First Design** - Origin validation, message filtering, and comprehensive security options
- **Responsive** - Dynamically resizes iframe based on content height (can be enabled/disabled)
- **Cross-Frame Messaging** - Built-in support for postMessage communication with validation
- **Event Callbacks** - OnLoad, OnMessage, and security event handling
- **Flexible Styling** - Customizable width, height, and additional attributes
- **JavaScript Interop** - Seamless integration with Blazor's JS interop
- **Scrolling Control** - Enable or disable scrolling within the iframe wrapper
- **Disposal Pattern** - Proper cleanup of resources and event listeners

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package BlazorFrame
```

Or via Package Manager Console:

```bash
Install-Package BlazorFrame
```

## Quick Start (Secure by Default)

```razor
@using BlazorFrame

<BlazorFrame Src="https://example.com" />
```

The component automatically derives allowed origins from the `Src` URL for secure messaging.

## Usage Examples

### Basic Usage with Event Handling

```razor
@using BlazorFrame
@using BlazorFrame.Models

<BlazorFrame Src="@iframeUrl"
            Width="100%"
            Height="400px"
            EnableAutoResize="true"
            EnableScroll="false"
            OnLoad="HandleIframeLoad"
            OnValidatedMessage="HandleValidatedMessage"
            OnSecurityViolation="HandleSecurityViolation"
            class="my-custom-iframe" />

@code {
    private string iframeUrl = "https://example.com";

    private Task HandleIframeLoad()
    {
        Console.WriteLine("Iframe loaded successfully!");
        return Task.CompletedTask;
    }

    private Task HandleValidatedMessage(IframeMessage message)
    {
        Console.WriteLine($"Secure message from {message.Origin}: {message.Data}");
        return Task.CompletedTask;
    }

    private Task HandleSecurityViolation(IframeMessage violation)
    {
        Console.WriteLine($"Security violation: {violation.ValidationError}");
        return Task.CompletedTask;
    }
}
```

### Advanced Security Configuration

```razor
@using BlazorFrame
@using BlazorFrame.Models

<BlazorFrame Src="@iframeUrl"
            AllowedOrigins="@allowedOrigins"
            SecurityOptions="@securityOptions"
            OnValidatedMessage="HandleValidatedMessage"
            OnSecurityViolation="HandleSecurityViolation" />

@code {
    private string iframeUrl = "https://widget.example.com";
    
    private List<string> allowedOrigins = new() 
    { 
        "https://widget.example.com", 
        "https://api.example.com" 
    };
    
    private MessageSecurityOptions securityOptions = new()
    {
        EnableStrictValidation = true,
        MaxMessageSize = 32 * 1024, // 32KB
        LogSecurityViolations = true
    };

    private Task HandleValidatedMessage(IframeMessage message)
    {
        // Handle validated, secure messages
        return Task.CompletedTask;
    }

    private Task HandleSecurityViolation(IframeMessage violation)
    {
        // Log, alert, or take corrective action
        Logger.LogWarning("Security violation: {Error}", violation.ValidationError);
        return Task.CompletedTask;
    }
}
```

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Src` | `string` | `""` | The URL to load in the iframe |
| `Width` | `string` | `"100%"` | The width of the iframe |
| `Height` | `string` | `"600px"` | The initial height of the iframe |
| `EnableAutoResize` | `bool` | `true` | Whether to automatically resize the iframe based on content height |
| `EnableScroll` | `bool` | `false` | Whether to enable scrolling within the iframe wrapper |
| `AllowedOrigins` | `List<string>?` | `null` | Explicit list of allowed origins. If null, auto-derives from Src |
| `SecurityOptions` | `MessageSecurityOptions` | `new()` | Security configuration options |
| `OnLoad` | `EventCallback` | - | Callback fired when the iframe loads |
| `OnMessage` | `EventCallback<string>` | - | Callback fired when receiving valid postMessage (legacy) |
| `OnValidatedMessage` | `EventCallback<IframeMessage>` | - | Callback fired with full message validation details |
| `OnSecurityViolation` | `EventCallback<IframeMessage>` | - | Callback fired when security violations occur |
| `AdditionalAttributes` | `Dictionary<string, object>` | - | Additional HTML attributes to apply |

## Security Features

### Origin Validation

BlazorFrame automatically validates message origins to prevent unauthorized communication:

- **Auto-derived origins**: Automatically allows messages from the iframe's source domain
- **Explicit allowlist**: Override with custom allowed origins for multi-domain scenarios
- **Protocol enforcement**: Ensures HTTPS origins when possible

### Message Validation

All incoming messages are validated before reaching your application:

```csharp
public class MessageSecurityOptions
{
    /// <summary>List of allowed origins (null = auto-derive from Src)</summary>
    public List<string>? AllowedOrigins { get; set; }
    
    /// <summary>Enable strict JSON format validation</summary>
    public bool EnableStrictValidation { get; set; } = true;
    
    /// <summary>Maximum message size (default: 64KB)</summary>
    public int MaxMessageSize { get; set; } = 64 * 1024;
    
    /// <summary>Log security violations</summary>
    public bool LogSecurityViolations { get; set; } = true;
}
```

### Validated Message Model

```csharp
public class IframeMessage
{
    public required string Origin { get; init; }        // Verified sender origin
    public required string Data { get; init; }          // Validated JSON string
    public bool IsValid { get; init; }                  // Security validation result
    public string? ValidationError { get; init; }       // Error details (if any)
    public string? MessageType { get; init; }           // Extracted message type
}
```

### Security Event Handling

Monitor and respond to security events:

```razor
<BlazorFrame OnSecurityViolation="HandleViolation" />

@code {
    private Task HandleViolation(IframeMessage violation)
    {
        // Log, alert, or take corrective action
        Logger.LogWarning("Security violation: {Error}", violation.ValidationError);
        return Task.CompletedTask;
    }
}
```

## Automatic Resizing

BlazorFrame can automatically adjust the iframe height based on the content inside when `EnableAutoResize` is set to `true` (default). The component:

1. Monitors the iframe content document every 500ms
2. Calculates the maximum height from various document properties
3. Updates the iframe height dynamically
4. Handles cross-origin restrictions gracefully
5. Can be disabled by setting `EnableAutoResize="false"`

## Cross-Frame Communication

### Sending Messages from Iframe (Secure)

```javascript
// Inside your iframe content - specify target origin for security
window.parent.postMessage({ 
    type: 'custom', 
    data: 'Hello from iframe!' 
}, 'https://your-parent-domain.com');
```

### Special Resize Messages

```javascript
// Send resize messages (validated automatically)
window.parent.postMessage({ 
    type: 'resize', 
    height: 800 
}, 'https://your-parent-domain.com');
```

### Receiving Validated Messages

```javascript
private Task HandleValidatedMessage(IframeMessage message)
{
    // message.Origin - verified sender origin
    // message.Data - validated JSON string
    // message.MessageType - extracted message type (if present)
    // message.IsValid - always true in this callback
    
    return Task.CompletedTask;
}
```

## Styling and CSS

The component includes built-in CSS styling with wrapper classes:

- **iframe-wrapper** - Applied to all iframe wrappers
- **iframe-wrapper scrollable** - Applied when `EnableScroll="true"`

The wrapper provides:

- 100% width by default
- Hidden overflow (unless scrollable)
- Borderless iframe display

## Examples

### Loading Different Content Types

```razor
<!-- Web pages -->
<BlazorFrame Src="https://docs.microsoft.com" />

<!-- Local HTML files -->
<BlazorFrame Src="./local-content.html" />

<!-- Data URLs -->
<BlazorFrame Src="data:text/html,<h1>Hello World!</h1>" />
```

### Multi-Domain Setup

```razor
<BlazorFrame Src="https://widget.example.com"
            AllowedOrigins="@(new List<string> 
            { 
                "https://widget.example.com", 
                "https://api.example.com" 
            })" />
```

### High-Security Configuration

```razor
<BlazorFrame Src="@secureUrl"
            SecurityOptions="@(new MessageSecurityOptions
            {
                EnableStrictValidation = true,
                MaxMessageSize = 16 * 1024, // 16KB limit
                LogSecurityViolations = true
            })"
            OnSecurityViolation="HandleViolation" />
```

### Responsive Design

```razor
<div class="container-fluid">
    <BlazorFrame Src="@contentUrl"
                Width="100%"
                Height="calc(100vh - 200px)"
                EnableAutoResize="false"
                style="min-height: 400px;" />
</div>
```

### Disabling Auto-Resize for Fixed Height

```razor
<BlazorFrame Src="@contentUrl"
            Width="100%"
            Height="500px"
            EnableAutoResize="false"
            EnableScroll="true" />
```

### Custom Styling and Attributes

```razor
<BlazorFrame Src="@iframeUrl"
            Width="800px"
            Height="600px"
            EnableAutoResize="false"
            EnableScroll="true"
            class="border rounded shadow"
            style="margin: 20px;"
            sandbox="allow-scripts allow-same-origin" />
```

## Best Practices

1. **Always specify target origin** when sending messages from iframe content
2. **Use OnValidatedMessage** for new implementations instead of legacy OnMessage
3. **Monitor security violations** in production environments
4. **Set appropriate MaxMessageSize** based on your use case
5. **Enable logging** for security auditing
6. **Use HTTPS** for iframe sources when possible
7. **Implement proper error handling** for security violations

## Requirements

- .NET 8.0 or later
- Blazor Server or Blazor WebAssembly

## Browser Support

BlazorFrame works in all modern browsers that support:

- ES6 modules
- postMessage API with origin validation
- Blazor JavaScript interop

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.txt) file for details.

## Support

If you encounter any issues or have questions:

- [Open an issue](https://github.com/Tim-Maes/BlazorFrame/issues) on GitHub
- Check existing issues for solutions
- Suggest new features or improvements

## Version History

### v1.1.0
- Added comprehensive security features
- Origin validation and message filtering
- Security event callbacks
- Enhanced logging support
- Backward compatibility maintained

### v1.0.1
- Initial release
- Basic iframe wrapper
- Auto-resize functionality
- PostMessage support
