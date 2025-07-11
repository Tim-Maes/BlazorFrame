# Sandbox Security

**New in BlazorFrame v2.1**

Iframe sandbox attributes provide content isolation and security by restricting what the iframe content can do. BlazorFrame includes comprehensive sandbox support with predefined presets and custom configuration options.

## Overview

The HTML5 `sandbox` attribute allows you to apply restrictions to iframe content, such as:
- Blocking script execution
- Preventing form submissions
- Disabling popups and modals
- Restricting same-origin access
- Blocking automatic downloads

BlazorFrame makes sandbox configuration simple and secure with intelligent defaults and easy-to-use presets.

## Sandbox Presets

### None (Default)
No sandbox restrictions - iframe content runs with full permissions.

```razor
<BlazorFrame Src="https://trusted-site.com"
            SecurityOptions="@noSandboxOptions" />

@code {
    private readonly MessageSecurityOptions noSandboxOptions = new()
    {
        SandboxPreset = SandboxPreset.None
    };
}
```

**Use when:** Loading fully trusted content that needs unrestricted access.

### Basic
Allows scripts and same-origin access - suitable for most trusted content.

**Sandbox Value:** `"allow-scripts allow-same-origin"`

```razor
<BlazorFrame Src="https://widget.example.com"
            SecurityOptions="@basicSandboxOptions" />

@code {
    private readonly MessageSecurityOptions basicSandboxOptions = new MessageSecurityOptions()
        .WithBasicSandbox();
        
    // Or using properties:
    // private readonly MessageSecurityOptions basicSandboxOptions = new()
    // {
    //     SandboxPreset = SandboxPreset.Basic,
    //     EnableSandbox = true
    // };
}
```

**Use when:** Loading trusted widgets that need script execution and same-origin access.

### Permissive
Allows scripts, same-origin access, forms, and popups - good for interactive widgets.

**Sandbox Value:** `"allow-scripts allow-same-origin allow-forms allow-popups"`

```razor
<BlazorFrame Src="https://interactive-widget.com"
            SecurityOptions="@permissiveSandboxOptions" />

@code {
    private readonly MessageSecurityOptions permissiveSandboxOptions = new MessageSecurityOptions()
        .WithPermissiveSandbox();
}
```

**Use when:** Loading interactive widgets that need form submission and popup capabilities.

### Strict
Only allows scripts and same-origin access - no forms or popups.

**Sandbox Value:** `"allow-scripts allow-same-origin"`

```razor
<BlazorFrame Src="https://display-widget.com"
            SecurityOptions="@strictSandboxOptions" />

@code {
    private readonly MessageSecurityOptions strictSandboxOptions = new MessageSecurityOptions()
        .WithStrictSandbox();
}
```

**Use when:** Loading content that should display information but not submit forms or open popups.

### Paranoid
Maximum isolation - only allows script execution, no same-origin access.

**Sandbox Value:** `"allow-scripts"`

```razor
<BlazorFrame Src="https://untrusted-content.com"
            SecurityOptions="@paranoidSandboxOptions" />

@code {
    private readonly MessageSecurityOptions paranoidSandboxOptions = new MessageSecurityOptions()
        .WithParanoidSandbox();
}
```

**Use when:** Loading untrusted content that needs maximum isolation.

## Custom Sandbox Configuration

### Using Explicit Sandbox Attributes

```razor
<BlazorFrame Src="https://custom-widget.com"
            SecurityOptions="@customSandboxOptions" />

@code {
    private readonly MessageSecurityOptions customSandboxOptions = new MessageSecurityOptions()
        .WithCustomSandbox("allow-scripts allow-forms allow-modals");
        
    // Or using properties:
    // private readonly MessageSecurityOptions customSandboxOptions = new()
    // {
    //     Sandbox = "allow-scripts allow-forms allow-modals"
    // };
}
```

### Using SandboxHelper for Custom Configuration

```razor
@using static BlazorFrame.SandboxHelper

@code {
    private readonly MessageSecurityOptions customOptions = new()
    {
        Sandbox = CreateCustomSandbox(
            allowScripts: true,
            allowSameOrigin: false,
            allowForms: true,
            allowPopups: false,
            allowModals: true
        )
    };
}
```

## Environment-Aware Configuration

### Development Environment
```razor
@code {
    private readonly MessageSecurityOptions devOptions = new MessageSecurityOptions()
        .ForDevelopment();  // Uses Permissive sandbox by default
}
```

### Production Environment
```razor
@code {
    private readonly MessageSecurityOptions prodOptions = new MessageSecurityOptions()
        .ForProduction();   // Uses Strict sandbox by default
}
```

### Payment Widgets (Maximum Security)
```razor
@code {
    private readonly MessageSecurityOptions paymentOptions = new MessageSecurityOptions()
        .ForPaymentWidget(); // Uses Strict sandbox + HTTPS enforcement
}
```

## Configuration Priority

BlazorFrame resolves sandbox configuration in this order:

1. **Explicit `Sandbox` property** - Takes highest precedence
2. **`SandboxPreset`** - If not `None` and no explicit sandbox
3. **`EnableSandbox` with Basic preset** - If enabled and no other configuration
4. **No sandbox** - Default behavior for backward compatibility

```razor
@code {
    private readonly MessageSecurityOptions conflictExample = new()
    {
        Sandbox = "allow-scripts",           // This wins (priority 1)
        SandboxPreset = SandboxPreset.Strict, // Ignored
        EnableSandbox = true                  // Ignored
    };
    // Result: sandbox="allow-scripts"
}
```

## Sandbox Permissions Reference

| Permission | Description | Security Impact |
|------------|-------------|-----------------|
| `allow-scripts` | Allows JavaScript execution | Medium - Required for most interactive content |
| `allow-same-origin` | Allows same-origin access | Medium - Required for many widgets |
| `allow-forms` | Allows form submission | Low - Only needed for forms |
| `allow-popups` | Allows popups and new windows | Low - Often blocked by browsers anyway |
| `allow-modals` | Allows modal dialogs (alert, confirm) | Low - Limited security impact |
| `allow-pointer-lock` | Allows pointer lock API | Low - Rarely needed |
| `allow-presentation` | Allows presentation API | Low - Rarely needed |

## Real-World Examples

### E-commerce Product Widget
```razor
<BlazorFrame Src="https://shop.example.com/widget"
            SecurityOptions="@ecommerceOptions" />

@code {
    private readonly MessageSecurityOptions ecommerceOptions = new MessageSecurityOptions()
        .WithPermissiveSandbox()  // Allow forms for "Add to Cart"
        .RequireHttps()           // Secure transport
        .ForProduction();         // Production security
}
```

### Social Media Feed
```razor
<BlazorFrame Src="https://social.example.com/feed"
            SecurityOptions="@socialOptions" />

@code {
    private readonly MessageSecurityOptions socialOptions = new MessageSecurityOptions()
        .WithStrictSandbox()      // No forms/popups needed
        .RequireHttps();          // Secure transport
}
```

### Analytics Dashboard
```razor
<BlazorFrame Src="https://analytics.example.com/dashboard"
            SecurityOptions="@analyticsOptions" />

@code {
    private readonly MessageSecurityOptions analyticsOptions = new MessageSecurityOptions()
        .WithBasicSandbox()       // Basic script execution
        .RequireHttps();          // Secure transport
}
```

### Untrusted Content Viewer
```razor
<BlazorFrame Src="@userProvidedUrl"
            SecurityOptions="@untrustedOptions" />

@code {
    private readonly MessageSecurityOptions untrustedOptions = new MessageSecurityOptions()
        .WithParanoidSandbox()    // Maximum isolation
        .RequireHttps()           // Force HTTPS
        .ForProduction();         // Strict validation
}
```

## Testing Sandbox Configuration

### Browser Developer Tools
1. Open **F12 Developer Tools**
2. Inspect the iframe element
3. Check the `sandbox` attribute value

```html
<!-- Example output -->
<iframe src="https://example.com" 
        sandbox="allow-scripts allow-same-origin">
</iframe>
```

### Configuration Validation
BlazorFrame validates sandbox configuration and provides warnings:

```razor
<BlazorFrame SecurityOptions="@options"
            OnSecurityViolation="HandleConfigurationIssue" />

@code {
    private readonly MessageSecurityOptions options = new()
    {
        EnableSandbox = false,        // Disabled
        SandboxPreset = SandboxPreset.Strict  // But preset is set
    };
    // This will generate a configuration warning
    
    private Task HandleConfigurationIssue(IframeMessage violation)
    {
        if (violation.MessageType == "configuration-validation")
        {
            Console.WriteLine($"Configuration issue: {violation.ValidationError}");
        }
        return Task.CompletedTask;
    }
}
```

## Best Practices

### Do
- **Use the most restrictive sandbox** that still allows required functionality
- **Test thoroughly** when changing sandbox settings
- **Monitor security violations** to catch sandbox-related issues
- **Use environment-aware presets** (`.ForDevelopment()`, `.ForProduction()`)
- **Document sandbox requirements** for third-party widgets

### Don't
- **Use `SandboxPreset.None`** for untrusted content
- **Disable sandbox** without understanding security implications
- **Mix sandbox configurations** without understanding priority
- **Ignore configuration warnings** - they indicate potential issues

## Migration from Manual Sandbox

If you were previously setting sandbox manually via `AdditionalAttributes`:

### Before (v2.0 and earlier)
```razor
<BlazorFrame Src="https://example.com"
            AdditionalAttributes="@sandboxAttributes" />

@code {
    private readonly Dictionary<string, object> sandboxAttributes = new()
    {
        ["sandbox"] = "allow-scripts allow-same-origin"
    };
}
```

### After (v2.1+)
```razor
<BlazorFrame Src="https://example.com"
            SecurityOptions="@securityOptions" />

@code {
    private readonly MessageSecurityOptions securityOptions = new MessageSecurityOptions()
        .WithBasicSandbox();
}
```

The new approach provides:
- **Type safety** and IntelliSense support
- **Configuration validation** with helpful warnings
- **Environment-aware defaults** for different scenarios
- **Fluent API** for easy chaining

---

**Related Topics:**
- [Security Overview](overview.md) - Complete security feature overview
- [Configuration Validation](configuration-validation.md) - Real-time validation
- [HTTPS Enforcement](https-enforcement.md) - Transport security
- [MessageSecurityOptions API](../api/message-security-options.md) - Complete API reference