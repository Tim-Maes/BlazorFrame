# Quick Start Guide

Get BlazorFrame up and running in your Blazor application in under 5 minutes.

## Prerequisites

- **.NET 8.0** or later
- **Blazor Server** or **Blazor WebAssembly** project
- Modern browser with ES6 modules support

## Installation

### Via Package Manager Console
```powershell
Install-Package BlazorFrame
```

### Via .NET CLI
```bash
dotnet add package BlazorFrame
```

### Via PackageReference
```xml
<PackageReference Include="BlazorFrame" Version="2.1.0" />
```

## Basic Setup

### 1. Add the Using Statement

In your Razor component or `_Imports.razor`:

```razor
@using BlazorFrame
```

### 2. Your First BlazorFrame

```razor
@page "/my-page"

<h3>My First BlazorFrame</h3>

<BlazorFrame Src="https://example.com" 
            Width="100%" 
            Height="400px" />
```

That's it! You now have a secure, auto-resizing iframe with built-in origin validation.

## Basic Configuration

### Auto-Resize and Scrolling

```razor
<BlazorFrame Src="https://example.com"
            EnableAutoResize="true"     <!-- Automatically adjusts height -->
            EnableScroll="false"        <!-- Disables iframe scrolling -->
            Width="100%"
            Height="300px" />           <!-- Initial height -->
```

### Event Handling

```razor
<BlazorFrame Src="https://example.com"
            OnLoad="HandleLoad"
            OnValidatedMessage="HandleMessage"
            OnSecurityViolation="HandleViolation" />

@code {
    private Task HandleLoad()
    {
        Console.WriteLine("Iframe loaded successfully!");
        return Task.CompletedTask;
    }

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
}
```

## Security Configuration

### Basic Security Setup

```razor
<BlazorFrame Src="https://widget.example.com"
            SecurityOptions="@securityOptions" />

@code {
    private readonly MessageSecurityOptions securityOptions = new()
    {
        EnableStrictValidation = true,
        MaxMessageSize = 32 * 1024,  // 32KB limit
        LogSecurityViolations = true
    };
}
```

### Using Fluent Configuration (New in v2.1)

```razor
@code {
    private readonly MessageSecurityOptions securityOptions = new MessageSecurityOptions()
        .ForProduction()              // Production-ready security
        .WithBasicSandbox()          // Enable iframe sandbox
        .RequireHttps();             // Enforce HTTPS
}
```

## Next Steps

**Congratulations!** You've successfully set up BlazorFrame. Here's what to explore next:

### Learn More
- [**Basic Usage**](basic-usage.md) - Explore all basic features
- [**Security Features**](../security/overview.md) - Understand security capabilities
- [**Configuration**](../configuration/security-options.md) - Deep dive into configuration options

### Security Features (New in v2.1)
- [**Sandbox Attributes**](../security/sandbox.md) - Iframe content isolation
- [**HTTPS Enforcement**](../security/https-enforcement.md) - Transport security
- [**Configuration Validation**](../security/configuration-validation.md) - Real-time validation

### Examples
- [**Common Scenarios**](../examples/common-scenarios.md) - Real-world examples
- [**Payment Widgets**](../examples/payment-widgets.md) - High-security integration
- [**Third-Party Widgets**](../examples/third-party-widgets.md) - External widget integration

### Advanced Topics
- [**CSP Integration**](../csp/overview.md) - Content Security Policy
- [**Custom Validation**](../advanced/custom-validation.md) - Custom security validators
- [**Performance Optimization**](../advanced/performance.md) - Optimize performance

---

## Common Issues

### Cross-Origin Issues
If you're seeing cross-origin errors, ensure your iframe source allows your domain:

```razor
<!-- Explicitly set allowed origins -->
<BlazorFrame Src="https://example.com"
            AllowedOrigins="@allowedOrigins" />

@code {
    private readonly List<string> allowedOrigins = new()
    {
        "https://example.com",
        "https://www.example.com"
    };
}
```

### Security Violations
Monitor security violations to understand what's being blocked:

```razor
<BlazorFrame OnSecurityViolation="LogViolation" />

@code {
    private Task LogViolation(IframeMessage violation)
    {
        // Log the violation for debugging
        Console.WriteLine($"Blocked: {violation.ValidationError}");
        return Task.CompletedTask;
    }
}
```

### Auto-Resize Not Working
Auto-resize requires access to iframe content. For cross-origin iframes:

```razor
<!-- Disable auto-resize for cross-origin content -->
<BlazorFrame Src="https://external-site.com"
            EnableAutoResize="false"
            Height="500px" />
```

---

**Need help?** Check out our [Troubleshooting Guide](../advanced/troubleshooting.md) or [open an issue](https://github.com/Tim-Maes/BlazorFrame/issues) on GitHub.