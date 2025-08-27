<h1>
<div align="center" style="margin:0; padding:0;">
<img src="https://github.com/Tim-Maes/BlazorFrame/blob/master/assets/BlazorFrameLogo.png"
     alt="BlazorFrame Logo"
     width="600" />
</div>
</h1>
     
A security-first Blazor iframe component with automatic resizing, cross-frame messaging, and comprehensive Content Security Policy integration.

[![NuGet](https://img.shields.io/nuget/v/BlazorFrame.svg)](https://www.nuget.org/packages/BlazorFrame)
[![Downloads](https://img.shields.io/nuget/dt/BlazorFrame.svg)](https://www.nuget.org/packages/BlazorFrame)
[![GitHub](https://img.shields.io/github/license/Tim-Maes/BlazorFrame.svg)](https://github.com/Tim-Maes/BlazorFrame/blob/main/LICENSE.txt)
[![CI](https://github.com/Tim-Maes/BlazorFrame/actions/workflows/ci.yml/badge.svg)](https://github.com/Tim-Maes/BlazorFrame/actions/workflows/ci.yml)
[![CD](https://github.com/Tim-Maes/BlazorFrame/actions/workflows/cd.yml/badge.svg)](https://github.com/Tim-Maes/BlazorFrame/actions/workflows/cd.yml)

## Features

- **Security-First Design** - Built-in origin validation, message filtering, and sandbox isolation
- **Content Security Policy** - Comprehensive CSP integration with fluent configuration API
- **Bidirectional Communication** - Secure postMessage communication with validation for both directions
- **Sandbox Support** - Multiple security levels from permissive to paranoid isolation
- **Environment-Aware** - Different configurations for development vs production
- **Automatic Resizing** - Smart height adjustment based on iframe content

## Documentation

**[Complete Documentation](https://github.com/Tim-Maes/BlazorFrame/blob/master/docs/readme.md)**

- [Quick Start Guide](https://github.com/Tim-Maes/BlazorFrame/tree/master/docs/getting-started/quick-start.md)
- [Security Features](https://github.com/Tim-Maes/BlazorFrame/tree/master/docs/security)
  - [Configuration Validation](https://github.com/Tim-Maes/BlazorFrame/blob/master/docs/security/configuration-validation.md)
  - [Sandbox](https://github.com/Tim-Maes/BlazorFrame/blob/master/docs/security/sandbox.md)
- [Configuration Guide](https://github.com/Tim-Maes/BlazorFrame/tree/master/docs/configuration/readme.md) 
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

<!-- Production-ready configuration with bidirectional communication -->
<BlazorFrame @ref="iframeRef"
            Src="https://widget.example.com"
            SecurityOptions="@securityOptions"
            OnValidatedMessage="HandleMessage"
            OnSecurityViolation="HandleViolation" />

<button @onclick="SendDataToIframe">Send Data</button>

@code {
    private BlazorFrame? iframeRef;
    
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
    }
    
    private async Task SendDataToIframe()
    {
        if (iframeRef != null)
        {
            await iframeRef.SendTypedMessageAsync("user-data", new { userId = 123, name = "John" });
        }
    }
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

## Security

### Content Security Policy

Comprehensive CSP integration for defense-in-depth security:
- **Automatic header generation** - CSP headers built from iframe requirements
- **Environment-aware policies** - Different rules for development vs production
- **Fluent configuration API** - Easy-to-use builder pattern for CSP rules
- **Violation monitoring** - Real-time CSP violation reporting and analysis
- **Nonce and hash support** - Modern CSP techniques for script security

### Message Validation

All iframe messages are automatically validated for:
- **Origin verification** - Ensures messages come from allowed domains
- **Content validation** - JSON structure and size limits
- **Security filtering** - Blocks malicious patterns and script injection
- **Custom validation** - Extensible validation pipeline

### Sandbox Security Levels

| Level | Description | Use Case |
|-------|-------------|----------|
| **None** | No restrictions | Trusted content only |
| **Basic** | Scripts + same-origin | Most trusted widgets |
| **Permissive** | + forms + popups | Interactive widgets |
| **Strict** | Scripts + same-origin only | Display widgets |
| **Paranoid** | Scripts only | Untrusted content |

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
