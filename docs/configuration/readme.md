# Configuration Guide

**Complete configuration reference for BlazorFrame**

BlazorFrame provides extensive configuration options for security, behavior, and appearance. This guide covers all configuration aspects from basic setup to advanced security scenarios.

## Quick Configuration

### Basic Usage

```razor
@using BlazorFrame

<!-- Minimal configuration -->
<BlazorFrame Src="https://example.com" />

<!-- Basic security configuration -->
<BlazorFrame Src="https://widget.example.com"
            SecurityOptions="@basicSecurity" />

@code {
    private readonly MessageSecurityOptions basicSecurity = new MessageSecurityOptions()
        .ForProduction()
        .WithBasicSandbox();
}
```

## Configuration Categories

### [Security Configuration](security-options.md)
Comprehensive security settings including sandbox, HTTPS enforcement, and message validation.

### [Display Configuration](display-options.md)  
Visual presentation options including dimensions, styling, and responsive behavior.

### [Communication Configuration](communication-options.md)
Cross-frame messaging, origin validation, and event handling.

### [Content Security Policy](csp-configuration.md)
CSP header generation, directive configuration, and security policies.

### [Advanced Configuration](advanced-options.md)
Performance tuning, debugging, and custom integration options.

---

## Environment-Specific Configurations

### Development Environment

```csharp
public static MessageSecurityOptions GetDevelopmentConfig()
{
    return new MessageSecurityOptions()
        .ForDevelopment()           // Relaxed security
        .WithPermissiveSandbox()    // Allow most interactions
        .Validate();               // Warn on issues but don't throw
}
```

### Production Environment 

```csharp
public static MessageSecurityOptions GetProductionConfig()
{
    return new MessageSecurityOptions()
        .ForProduction()           // Strict security
        .WithStrictSandbox()       // Limited iframe permissions
        .RequireHttps()            // Enforce HTTPS transport
        .ValidateAndThrow();       // Fail fast on configuration errors
}
```

### High-Security Environment

```csharp
public static MessageSecurityOptions GetHighSecurityConfig()
{
    return new MessageSecurityOptions()
        .ForPaymentWidget()        // Maximum security preset
        .WithParanoidSandbox()     // Minimal iframe permissions
        .ValidateAndThrow();       // Critical validation
}
```

---

## Configuration Patterns

### Widget Hosting

```razor
<BlazorFrame Src="@widgetUrl"
            SecurityOptions="@widgetSecurity"
            CspOptions="@widgetCsp"
            OnValidatedMessage="HandleWidgetMessage"
            OnSecurityViolation="LogSecurityViolation" />

@code {
    private readonly MessageSecurityOptions widgetSecurity = new()
    {
        EnableStrictValidation = true,
        MaxMessageSize = 32 * 1024,        // 32KB limit
        AllowedOrigins = new() { "https://widget.example.com" },
        EnableSandbox = true,
        SandboxPreset = SandboxPreset.Basic,
        RequireHttps = true,
        LogSecurityViolations = true
    };
    
    private readonly CspOptions widgetCsp = new CspOptions()
        .ForProduction()
        .AllowFrameSources("https://widget.example.com")
        .WithScriptNonce("widget-nonce-123");
}
```

### Multi-Domain Setup

```razor
<BlazorFrame Src="@currentUrl"
            SecurityOptions="@multiDomainSecurity"
            AllowedOrigins="@allowedDomains" />

@code {
    private readonly List<string> allowedDomains = new()
    {
        "https://widget.example.com",
        "https://api.example.com", 
        "https://cdn.example.com"
    };
    
    private readonly MessageSecurityOptions multiDomainSecurity = new()
    {
        EnableStrictValidation = true,
        MaxMessageSize = 64 * 1024,
        ValidateOrigins = true,
        EnableSandbox = true,
        SandboxPreset = SandboxPreset.Permissive
    };
}
```

### Responsive Configuration

```razor
<div class="iframe-container">
    <BlazorFrame Src="@responsiveUrl"
                Width="@currentWidth"
                Height="@currentHeight"
                EnableAutoResize="@enableAutoResize"
                ResizeOptions="@resizeOptions" />
</div>

@code {
    private readonly ResizeOptions resizeOptions = new()
    {
        DebounceDelayMs = 100,
        MaxHeight = 1000,
        MinHeight = 200,
        UseResizeObserver = true
    };
    
    private string currentWidth = "100%";
    private string currentHeight = "400px";
    private bool enableAutoResize = true;
    
    protected override void OnInitialized()
    {
        // Adjust based on screen size, device, etc.
        SetResponsiveDefaults();
    }
}
```

---

## Configuration Validation

### Automatic Validation

BlazorFrame automatically validates configurations on:
- Component initialization
- Parameter changes  
- Manual validation calls

```csharp
// Validate configuration manually
var validation = securityOptions.ValidateConfiguration();

if (!validation.IsValid)
{
    foreach (var error in validation.Errors)
    {
        Logger.LogError("Config error: {Error}", error);
    }
}
```

### Validation Events

```razor
<BlazorFrame SecurityOptions="@securityOptions"
            OnSecurityViolation="HandleConfigurationViolation" />

@code {
    private async Task HandleConfigurationViolation(IframeMessage violation)
    {
        if (violation.MessageType == "configuration-validation")
        {
            Logger.LogWarning("Configuration issue: {Error}", violation.ValidationError);
            
            // Could show user notification or apply safe defaults
            await ApplySafeDefaults();
        }
    }
}
```

---

## Best Practices

### Do
- **Use environment-specific configurations** for development vs production
- **Validate configurations early** with `.ValidateAndThrow()` during startup
- **Monitor security violations** in production environments
- **Use fluent configuration API** for cleaner, more readable code
- **Document security decisions** especially when using permissive settings

### Don't  
- **Mix development and production** settings in the same environment
- **Ignore configuration warnings** - they often indicate real issues
- **Use overly permissive settings** in production without justification
- **Disable validation** to hide configuration problems
- **Set very large limits** (MaxMessageSize, MaxJsonDepth) without considering DoS risks

---

## Configuration Reference

### Core Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Src` | `string` | `null` | URL of the iframe content |
| `Width` | `string` | `"100%"` | Iframe width (CSS value) |
| `Height` | `string` | `"300px"` | Iframe height (CSS value) |
| `EnableAutoResize` | `bool` | `true` | Enable automatic height adjustment |
| `EnableScroll` | `bool` | `false` | Enable iframe scrolling |

### Security Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `SecurityOptions` | `MessageSecurityOptions` | Default instance | Security configuration |
| `AllowedOrigins` | `List<string>` | `null` | Explicit allowed message origins |
| `CspOptions` | `CspOptions` | `null` | Content Security Policy configuration |

### Event Parameters
| Parameter | Type | Description |
|-----------|------|-------------|
| `OnValidatedMessage` | `EventCallback<IframeMessage>` | Secure message received |
| `OnSecurityViolation` | `EventCallback<IframeMessage>` | Security policy violation |
| `OnLoad` | `EventCallback` | Iframe finished loading |
| `OnCspHeaderGenerated` | `EventCallback<CspHeader>` | CSP header created |

---

## Migration Guide

### From Basic to Secure Configuration

```csharp
// Before: Basic configuration
<BlazorFrame Src="https://example.com" />

// After: Secure configuration
<BlazorFrame Src="https://example.com"
            SecurityOptions="@secureOptions" />

@code {
    private readonly MessageSecurityOptions secureOptions = new MessageSecurityOptions()
        .ForProduction()
        .WithBasicSandbox()
        .RequireHttps();
}
```

### Adding CSP Protection

```csharp
// Before: No CSP
<BlazorFrame Src="@iframeUrl" SecurityOptions="@securityOptions" />

// After: With CSP
<BlazorFrame Src="@iframeUrl" 
            SecurityOptions="@securityOptions"
            CspOptions="@cspOptions"
            OnCspHeaderGenerated="ApplyCspHeader" />

@code {
    private readonly CspOptions cspOptions = new CspOptions()
        .ForProduction()
        .AllowFrameSources(iframeUrl);
        
    private Task ApplyCspHeader(CspHeader header)
    {
        // Apply CSP header to HTTP response
        HttpContext.Response.Headers.Add(header.HeaderName, header.HeaderValue);
        return Task.CompletedTask;
    }
}
```

---

## Troubleshooting

### Common Configuration Issues

1. **HTTPS conflicts** - `RequireHttps=true` with `AllowInsecureConnections=true`
2. **Sandbox conflicts** - `EnableSandbox=false` with `SandboxPreset` set
3. **Origin validation failures** - Messages from unexpected domains
4. **CSP violations** - Iframe sources not allowed by policy

### Debugging Configuration

```razor
@page "/config-debug"

<div class="config-debug">
    <h3>Current Configuration</h3>
    
    <pre>@JsonSerializer.Serialize(securityOptions, new JsonSerializerOptions 
    { 
        WriteIndented = true 
    })</pre>
    
    <h3>Validation Results</h3>
    <div class="validation-results">
        @foreach (var error in validationResult.Errors)
        {
            <div class="alert alert-danger">@error</div>
        }
        @foreach (var warning in validationResult.Warnings)
        {
            <div class="alert alert-warning">@warning</div>
        }
    </div>
</div>

@code {
    private readonly MessageSecurityOptions securityOptions = new MessageSecurityOptions()
        .ForProduction();
        
    private ConfigurationValidationResult validationResult = new();
    
    protected override void OnInitialized()
    {
        validationResult = securityOptions.ValidateConfiguration();
    }
}
```

---

## Support

- **Configuration Issues**: [GitHub Issues](https://github.com/Tim-Maes/BlazorFrame/issues)
- **Security Questions**: [GitHub Discussions](https://github.com/Tim-Maes/BlazorFrame/discussions)
- **API Reference**: [Complete API Documentation](../api/message-security-options.md)

---
