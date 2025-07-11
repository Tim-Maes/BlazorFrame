# BlazorFrame Documentation

Welcome to the comprehensive documentation for **BlazorFrame** - the enhanced, secure Blazor iframe component with built-in security features, automatic resizing, and Content Security Policy integration.

## Available Documentation

### Getting Started
- [**Quick Start**](getting-started/quick-start.md) - Get up and running in minutes

### Configuration
- [**Configuration Guide**](configuration/index.md) - Complete configuration reference (New!)
  - [Security Configuration](configuration/security-options.md) - Security settings and presets
  - [Display Configuration](configuration/display-options.md) - Visual presentation options
  - [Communication Configuration](configuration/communication-options.md) - Cross-frame messaging
  - [CSP Configuration](configuration/csp-configuration.md) - Content Security Policy
  - [Advanced Configuration](configuration/advanced-options.md) - Performance and enterprise features

### Core Features  
- [**Component Parameters**](core-features/parameters.md) - Complete parameter reference

### Security Features
- [**Sandbox Attributes**](security/sandbox.md) - Iframe sandbox security (New in v2.1)
- [**Configuration Validation**](security/configuration-validation.md) - Real-time security validation (New in v2.1)

### Examples & Recipes
- [**Common Scenarios**](examples/common-scenarios.md) - Real-world usage examples

### API Reference
- [**MessageSecurityOptions**](api/message-security-options.md) - Security configuration API

### Advanced Topics
- [**Troubleshooting**](advanced/troubleshooting.md) - Common issues and solutions

---

## What's New in v2.1

BlazorFrame v2.1 introduces powerful new security features:

### Sandbox Security

- **Iframe Sandbox Attributes** - Comprehensive sandbox support with presets
- **Sandbox Presets** - None, Basic, Permissive, Strict, and Paranoid levels
- **Custom Sandbox Configuration** - Granular control over iframe permissions

### Enhanced Security

- **HTTPS Enforcement** - Configurable transport security requirements
- **Configuration Validation** - Real-time validation with conflict detection
- **URL Validation** - Security validation for iframe sources

### Developer Experience

- **Fluent Configuration API** - Easy-to-use extension methods
- **Environment-Aware Defaults** - Automatic development/production configuration
- **Comprehensive Validation** - Detailed error reporting and suggestions

---

## Quick Start

Get started with BlazorFrame in under 2 minutes:

# Install the package
```bash
dotnet add package BlazorFrame
```

```razor
@using BlazorFrame<BlazorFrame Src="https://example.com" />

<BlazorFrame Src="https://widget.example.com"
            SecurityOptions="@securityOptions"
            OnValidatedMessage="HandleMessage"
            OnSecurityViolation="HandleViolation" />

@code {
    private readonly MessageSecurityOptions securityOptions = new MessageSecurityOptions()
        .ForProduction()
        .WithBasicSandbox()
        .RequireHttps();
        
    private Task HandleMessage(IframeMessage message) => Task.CompletedTask;
    private Task HandleViolation(IframeMessage violation) => Task.CompletedTask;
}
```

## Core Features Overview

### Security-First Design

BlazorFrame is built with security as the primary concern:

- **Origin Validation** - Automatic validation of message origins
- **Message Filtering** - Comprehensive message content validation
- **Sandbox Support** - Iframe content isolation with multiple security levels
- **HTTPS Enforcement** - Configurable transport security requirements
- **Real-time Validation** - Configuration conflict detection and resolution

### Automatic Resizing

Smart iframe height management:

- **Content-based Resizing** - Automatically adjusts to iframe content height
- **Cross-origin Safe** - Handles cross-origin restrictions gracefully
- **Performance Optimized** - Uses ResizeObserver when available, falls back to polling
- **Configurable** - Can be disabled for fixed-height scenarios

### Cross-Frame Communication

Secure postMessage integration:

- **Validated Messages** - All messages undergo security validation
- **Type-safe Events** - Strongly-typed message and violation events
- **Origin Filtering** - Automatic origin-based message filtering
- **Custom Validation** - Extensible validation pipeline

### Content Security Policy

Built-in CSP support:

- **Header Generation** - Automatic CSP header creation
- **Fluent Configuration** - Easy-to-use configuration API
- **Environment-aware** - Different settings for development/production
- **Validation** - CSP configuration validation and suggestions

## Configuration Examples

### Development Configuration

```csharp
var devOptions = new MessageSecurityOptions()
    .ForDevelopment()           // Relaxed security for development
    .WithPermissiveSandbox()    // Allow most iframe interactions
    .Validate();               // Check configuration but don't throw### Production Configuration
var prodOptions = new MessageSecurityOptions()
    .ForProduction()           // Strict security for production
    .WithStrictSandbox()       // Limited iframe permissions
    .ValidateAndThrow();       // Throw on configuration errors
```

### Payment Widget Configuration

```csharp
var paymentOptions = new MessageSecurityOptions()
    .ForPaymentWidget()        // Maximum security preset
    .ValidateAndThrow();       // Critical to validate payment configs
```

### Custom Configuration

```csharp
var customOptions = new MessageSecurityOptions
{
    EnableStrictValidation = true,
    MaxMessageSize = 32 * 1024,
    SandboxPreset = SandboxPreset.Basic,
    EnableSandbox = true,
    RequireHttps = true,
    AllowInsecureConnections = false,
    LogSecurityViolations = true
};
```

## Browser Support

BlazorFrame works in all modern browsers that support:

- **ES6 modules** - Modern JavaScript module support
- **postMessage API** - Cross-frame communication with origin validation
- **Blazor JavaScript interop** - .NET to JavaScript communication
- **ResizeObserver** - Optimal auto-resize performance (fallback available)

## Requirements

- **.NET 8.0** or later
- **Blazor Server** or **Blazor WebAssembly** project
- Modern browser with ES6 modules support

---

## Support & Community

- **Issues**: [GitHub Issues](https://github.com/Tim-Maes/BlazorFrame/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Tim-Maes/BlazorFrame/discussions)
- **Source Code**: [GitHub Repository](https://github.com/Tim-Maes/BlazorFrame)
- **NuGet Package**: [BlazorFrame on NuGet](https://www.nuget.org/packages/BlazorFrame)

---

**Built with love for the Blazor community**