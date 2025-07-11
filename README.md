# BlazorFrame

A security-first Blazor iframe component with automatic resizing, cross-frame messaging, and comprehensive Content Security Policy integration.

[![NuGet](https://img.shields.io/nuget/v/BlazorFrame.svg)](https://www.nuget.org/packages/BlazorFrame)
[![Downloads](https://img.shields.io/nuget/dt/BlazorFrame.svg)](https://www.nuget.org/packages/BlazorFrame)
[![GitHub](https://img.shields.io/github/license/Tim-Maes/BlazorFrame.svg)](https://github.com/Tim-Maes/BlazorFrame/blob/main/LICENSE.txt)

## Features

- **Security-First Design** - Built-in origin validation, message filtering, and sandbox isolation
- **Content Security Policy** - Comprehensive CSP integration with fluent configuration API
- **Cross-Frame Messaging** - Secure postMessage communication with validation
- **Sandbox Support** - Multiple security levels from permissive to paranoid isolation
- **Environment-Aware** - Different configurations for development vs production
- **Automatic Resizing** - Smart height adjustment based on iframe content

## Documentation

**[Complete Documentation](https://github.com/Tim-Maes/BlazorFrame/blob/master/docs/index.md)**

- [Quick Start Guide](https://github.com/Tim-Maes/BlazorFrame/tree/master/docs/getting-started/quick-start.md)

- [Security Features](https://github.com/Tim-Maes/BlazorFrame/tree/master/docs/security)
  - [Configuration Validation](https://github.com/Tim-Maes/BlazorFrame/blob/master/docs/security/configuration-validation.md)
  - [Sandbox](https://github.com/Tim-Maes/BlazorFrame/blob/master/docs/security/sandbox.md)

- [Configuration Guide](https://github.com/Tim-Maes/BlazorFrame/tree/master/docs/configuration) 
  - [Display options](https://github.com/Tim-Maes/BlazorFrame/tree/master/docs/configuration/display-options.md)
  - [Advanced options](https://github.com/Tim-Maes/BlazorFrame/tree/master/docs/configuration/advanced-options.md)
  - [Communication options](https://github.com/Tim-Maes/BlazorFrame/tree/master/docs/configuration/communication-options.md)
  - [CSP configuration](https://github.com/Tim-Maes/BlazorFrame/tree/master/docs/configuration/csp-configuration.md)
  - [Security options](https://github.com/Tim-Maes/BlazorFrame/tree/master/docs/configuration/security-options.md)

- [Real-world Examples](https://github.com/Tim-Maes/BlazorFrame/blob/master/docs/examples/common-scenarios.md)

- [API Reference](https://github.com/Tim-Maes/BlazorFrame/blob/master/docs/api/message-security-options.md)

- [Troubleshooting](https://github.com/Tim-Maes/BlazorFrame/tree/master/docs/advanced/troubleshooting.md)

## :rocket: Quick Start

### Installation

```bash
dotnet add package BlazorFrame
```

### Basic Usage

```razor
@using BlazorFrame

<!-- Simple iframe with automatic security -->
<BlazorFrame Src="https://example.com" />

<!-- Production-ready configuration -->
<BlazorFrame Src="https://widget.example.com"
            SecurityOptions="@securityOptions"
            OnValidatedMessage="HandleMessage"
            OnSecurityViolation="HandleViolation" />

@code {
    private readonly MessageSecurityOptions securityOptions = new MessageSecurityOptions()
        .ForProduction()        // Strict security settings
        .WithBasicSandbox()     // Enable iframe sandboxing
        .RequireHttps();        // Enforce HTTPS transport
        
    private Task HandleMessage(IframeMessage message)
    {
        Console.WriteLine($"Received message from {message.Origin}: {message.Data}");
        return Task.CompletedTask;
    }

    private Task HandleViolation(IframeMessage violation)
    {
        Console.WriteLine($"Security violation: {violation.ValidationError}");
        return Task.CompletedTask;
    };
}
```

### Configuration Examples

```csharp
// Development environment - relaxed security
var devOptions = new MessageSecurityOptions()
    .ForDevelopment()
    .WithPermissiveSandbox();

// Production environment - strict security
var prodOptions = new MessageSecurityOptions()
    .ForProduction()
    .WithStrictSandbox()
    .ValidateAndThrow();

// Payment widgets - maximum security
var paymentOptions = new MessageSecurityOptions()
    .ForPaymentWidget();
```

### Content Security Policy

```razor
<BlazorFrame Src="https://widget.example.com"
            CspOptions="@cspOptions"
            OnCspHeaderGenerated="HandleCspGenerated" />

@code {
    private readonly CspOptions cspOptions = new CspOptions()
        .ForProduction()
        .AllowFrameSources("https://widget.example.com")
        .WithScriptNonce("secure-nonce-123");
        
    private Task HandleCspGenerated(CspHeader cspHeader)
    {
        // Apply CSP header to HTTP response
        // HttpContext.Response.Headers.Add(cspHeader.HeaderName, cspHeader.HeaderValue);
        return Task.CompletedTask;
    }
}
```

## Security Features

### Sandbox Security Levels

| Level | Description | Use Case |
|-------|-------------|----------|
| **None** | No restrictions | Trusted content only |
| **Basic** | Scripts + same-origin | Most trusted widgets |
| **Permissive** | + forms + popups | Interactive widgets |
| **Strict** | Scripts + same-origin only | Display widgets |
| **Paranoid** | Scripts only | Untrusted content |

### Message Validation

All iframe messages are automatically validated for:
- **Origin verification** - Ensures messages come from allowed domains
- **Content validation** - JSON structure and size limits
- **Security filtering** - Blocks malicious patterns and script injection
- **Custom validation** - Extensible validation pipeline

## Demo

[![BlazorFrame Demo](https://github.com/user-attachments/assets/106e02f8-5b7a-4a02-9748-b5fc1f540168)](https://github.com/Tim-Maes/BlazorFrameDemo)

**[Interactive Demo](https://github.com/Tim-Maes/BlazorFrameDemo)** - Try different security configurations live

## Requirements

- **.NET 8.0** or later
- **Blazor Server** or **Blazor WebAssembly**
- Modern browser with ES6 modules support

## Browser Support

- Chrome 91+
- Firefox 90+  
- Safari 15+
- Edge 91+

## Support

- **Issues**: [GitHub Issues](https://github.com/Tim-Maes/BlazorFrame/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Tim-Maes/BlazorFrame/discussions)  
- **NuGet**: [BlazorFrame Package](https://www.nuget.org/packages/BlazorFrame)

## License

This project is licensed under the [MIT License](LICENSE.txt).

---

**Built with :heart: for the Blazor community**
