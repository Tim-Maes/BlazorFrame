# Security Configuration

**Security configuration for BlazorFrame**

Security is a core feature of BlazorFrame. This guide covers all security-related configuration options, from basic protection to high-security environments.

## Overview

BlazorFrame security operates on multiple layers:

- **Transport Security** - HTTPS enforcement and connection validation
- **Sandbox Security** - Iframe isolation and permission control  
- **Message Security** - Cross-frame communication validation
- **Content Security Policy** - Browser-level security directives
- **Origin Validation** - Domain-based access control

## Security Presets

### Quick Setup with Presets

```csharp
// Development - Relaxed security for testing
var devOptions = new MessageSecurityOptions().ForDevelopment();

// Production - Balanced security for most applications  
var prodOptions = new MessageSecurityOptions().ForProduction();

// Payment/Financial - Maximum security for sensitive content
var paymentOptions = new MessageSecurityOptions().ForPaymentWidget();

// Trusted Content - Minimal restrictions for known-safe content
var trustedOptions = new MessageSecurityOptions().ForTrustedContent();
```

### Preset Comparison

| Preset | HTTPS Required | Sandbox Level | Message Validation | Use Case |
|--------|---------------|---------------|-------------------|----------|
| **Development** | No | Permissive | Basic | Development/Testing |
| **Production** | Yes | Basic | Strict | Most web applications |
| **Payment** | Yes | Paranoid | Maximum | Financial/Sensitive data |
| **Trusted** | No | None | Minimal | Internal/Trusted content |

## Transport Security

### HTTPS Enforcement

```csharp
var httpsOptions = new MessageSecurityOptions
{
    RequireHttps = true,                    // Require HTTPS for iframe sources
    AllowInsecureConnections = false,       // Disallow HTTP fallback
    ValidateTransportSecurity = true        // Validate SSL certificates
};
```

### Development HTTPS Configuration

```csharp
var devHttpsOptions = new MessageSecurityOptions
{
    RequireHttps = false,                   // Allow HTTP in development
    AllowInsecureConnections = true,        // Allow localhost HTTP
    AllowLocalhostInsecure = true,          // Specific localhost exception
    ValidateTransportSecurity = false       // Skip SSL validation in dev
};
```

### Custom Protocol Support

```csharp
var protocolOptions = new MessageSecurityOptions
{
    AllowedProtocols = new List<string> { "https", "data" },
    AllowScriptProtocols = false,           // Block javascript: URLs
    AllowDataUrls = true,                   // Allow data: URLs
    AllowBlobUrls = false                   // Block blob: URLs for security
};
```

## Sandbox Security

### Sandbox Presets

```csharp
// No sandbox - Full permissions (use with caution)
var noSandbox = new MessageSecurityOptions
{
    EnableSandbox = false,
    SandboxPreset = SandboxPreset.None
};

// Basic sandbox - Scripts + same-origin access
var basicSandbox = new MessageSecurityOptions
{
    EnableSandbox = true,
    SandboxPreset = SandboxPreset.Basic
};

// Permissive sandbox - Scripts + forms + popups + same-origin
var permissiveSandbox = new MessageSecurityOptions
{
    EnableSandbox = true,
    SandboxPreset = SandboxPreset.Permissive
};

// Strict sandbox - Scripts + same-origin only
var strictSandbox = new MessageSecurityOptions
{
    EnableSandbox = true,
    SandboxPreset = SandboxPreset.Strict
};

// Paranoid sandbox - Scripts only, no network access
var paranoidSandbox = new MessageSecurityOptions
{
    EnableSandbox = true,
    SandboxPreset = SandboxPreset.Paranoid
};
```

### Custom Sandbox Configuration

```csharp
var customSandbox = new MessageSecurityOptions
{
    EnableSandbox = true,
    Sandbox = "allow-scripts allow-same-origin allow-forms",
    SandboxPreset = SandboxPreset.None  // Disable preset when using custom
};
```

### Sandbox Security Levels

#### None - No Restrictions

```html
<!-- No sandbox attribute applied -->
<iframe src="https://example.com"></iframe>
```
**Use case**: Fully trusted content only

#### Basic - Scripts + Same-Origin

```html
<iframe src="https://example.com" sandbox="allow-scripts allow-same-origin"></iframe>
```
**Use case**: Most trusted widgets and applications

#### Permissive - Interactive Content

```html
<iframe src="https://example.com" sandbox="allow-scripts allow-same-origin allow-forms allow-popups"></iframe>
```
**Use case**: Interactive widgets requiring forms and popups

#### Strict - Display Only

```html
<iframe src="https://example.com" sandbox="allow-scripts allow-same-origin"></iframe>
```
**Use case**: Display widgets without user interaction

#### Paranoid - Isolated Content

```html
<iframe src="https://example.com" sandbox="allow-scripts"></iframe>
```
**Use case**: Untrusted content that must be isolated

## Message Security

### Message Validation Configuration

```csharp
var messageValidation = new MessageSecurityOptions
{
    EnableStrictValidation = true,          // Enable comprehensive validation
    MaxMessageSize = 32 * 1024,            // 32KB message size limit
    MaxJsonDepth = 10,                     // Max JSON nesting depth
    MaxObjectProperties = 100,              // Max properties per object
    MaxArrayElements = 1000,               // Max array length
    ValidateMessageStructure = true,        // Validate JSON structure
    FilterMaliciousContent = true,         // Filter dangerous patterns
    LogSecurityViolations = true           // Log all violations
};
```

### Origin Validation

```csharp
var originValidation = new MessageSecurityOptions
{
    ValidateOrigins = true,                // Enable origin checking
    AllowedOrigins = new List<string>      // Specific allowed origins
    {
        "https://widget.example.com",
        "https://api.example.com"
    },
    AllowWildcardOrigins = false,          // Disable wildcard origins
    StrictOriginMatching = true            // Require exact origin match
};
```

### Content Filtering

```csharp
var contentFiltering = new MessageSecurityOptions
{
    FilterMaliciousContent = true,         // Enable content filtering
    BlockedPatterns = new List<string>     // Custom blocked patterns
    {
        @"<script\b",                      // Block script tags
        @"javascript:",                    // Block javascript URLs
        @"eval\s*\(",                      // Block eval calls
        @"document\.write"                 // Block document.write
    },
    AllowHtmlContent = false,              // Block HTML in messages
    SanitizeStringValues = true            // Sanitize string values
};
```

## Advanced Security Options


### Rate Limiting

```csharp
var rateLimiting = new MessageSecurityOptions
{
    EnableRateLimiting = true,             // Enable message rate limiting
    MaxMessagesPerSecond = 10,             // Max 10 messages per second
    RateLimitWindow = TimeSpan.FromMinutes(1), // 1-minute window
    TemporaryBlockDuration = TimeSpan.FromMinutes(5) // 5-minute block
};
```

### Security Headers

```csharp
var securityHeaders = new MessageSecurityOptions
{
    RequireSecurityHeaders = true,         // Require security headers
    RequiredHeaders = new Dictionary<string, string>
    {
        ["X-Frame-Options"] = "SAMEORIGIN",
        ["X-Content-Type-Options"] = "nosniff",
        ["X-XSS-Protection"] = "1; mode=block"
    },
    ValidateResponseHeaders = true         // Validate iframe response headers
};
```

### Cryptographic Verification

```csharp
var cryptoOptions = new MessageSecurityOptions
{
    RequireMessageSigning = true,          // Require signed messages
    SharedSecret = "your-shared-secret",   // Secret for HMAC verification
    SigningAlgorithm = "HMAC-SHA256",     // Signing algorithm
    MessageTimestampTolerance = TimeSpan.FromMinutes(5) // Clock skew tolerance
};
```

## Security Event Handling

### Security Violation Events

```razor
<BlazorFrame SecurityOptions="@securityOptions"
            OnSecurityViolation="HandleSecurityViolation" />

@code {
    private async Task HandleSecurityViolation(IframeMessage violation)
    {
        var violationType = violation.MessageType;
        var errorDetails = violation.ValidationError;
        
        switch (violationType)
        {
            case "origin-validation":
                await HandleOriginViolation(violation);
                break;
                
            case "message-validation":
                await HandleMessageViolation(violation);
                break;
                
            case "rate-limiting":
                await HandleRateLimitViolation(violation);
                break;
                
            case "transport-security":
                await HandleTransportViolation(violation);
                break;
                
            default:
                await HandleGenericViolation(violation);
                break;
        }
    }
    
    private async Task HandleOriginViolation(IframeMessage violation)
    {
        Logger.LogWarning("Origin violation from {Origin}: {Error}", 
            violation.Origin, violation.ValidationError);
            
        // Could block the origin temporarily
        await TemporarilyBlockOrigin(violation.Origin);
    }
    
    private async Task HandleMessageViolation(IframeMessage violation)
    {
        Logger.LogError("Message validation failed: {Error}", violation.ValidationError);
        
        // Could show user notification
        await ShowSecurityAlert("Invalid message received from iframe");
    }
}
```

### Security Monitoring

```csharp
public class SecurityMonitoringService
{
    private readonly ILogger<SecurityMonitoringService> _logger;
    private readonly Dictionary<string, ViolationCounter> _violationCounters = new();
    
    public async Task MonitorViolation(IframeMessage violation)
    {
        var origin = violation.Origin ?? "unknown";
        
        if (!_violationCounters.ContainsKey(origin))
        {
            _violationCounters[origin] = new ViolationCounter();
        }
        
        var counter = _violationCounters[origin];
        counter.Increment();
        
        // Block origin if too many violations
        if (counter.ViolationCount > 10)
        {
            await BlockOriginPermanently(origin);
        }
        
        // Alert if unusual activity
        if (counter.ViolationsPerMinute > 5)
        {
            await AlertSecurityTeam(origin, violation);
        }
    }
}
```

## Environment-Specific Security

### Development Security

```csharp
public static MessageSecurityOptions CreateDevelopmentSecurity()
{
    return new MessageSecurityOptions
    {
        // Relaxed for development
        RequireHttps = false,
        AllowInsecureConnections = true,
        AllowLocalhostInsecure = true,
        
        // Basic validation
        EnableStrictValidation = false,
        LogSecurityViolations = true,
        
        // Permissive sandbox
        EnableSandbox = true,
        SandboxPreset = SandboxPreset.Permissive,
        
        // Large limits for testing
        MaxMessageSize = 1024 * 1024,  // 1MB
        MaxJsonDepth = 50
    };
}
```

### Production Security

```csharp
public static MessageSecurityOptions CreateProductionSecurity()
{
    return new MessageSecurityOptions
    {
        // Strict transport security
        RequireHttps = true,
        AllowInsecureConnections = false,
        ValidateTransportSecurity = true,
        
        // Comprehensive validation
        EnableStrictValidation = true,
        LogSecurityViolations = true,
        
        // Basic sandbox for compatibility
        EnableSandbox = true,
        SandboxPreset = SandboxPreset.Basic,
        
        // Conservative limits
        MaxMessageSize = 64 * 1024,    // 64KB
        MaxJsonDepth = 10,
        
        // Origin validation
        ValidateOrigins = true,
        StrictOriginMatching = true
    };
}
```

### High-Security Environment

```csharp
public static MessageSecurityOptions CreateHighSecurity()
{
    return new MessageSecurityOptions
    {
        // Maximum transport security
        RequireHttps = true,
        AllowInsecureConnections = false,
        ValidateTransportSecurity = true,
        RequireSecurityHeaders = true,
        
        // Maximum validation
        EnableStrictValidation = true,
        FilterMaliciousContent = true,
        LogSecurityViolations = true,
        
        // Paranoid sandbox
        EnableSandbox = true,
        SandboxPreset = SandboxPreset.Paranoid,
        
        // Strict limits
        MaxMessageSize = 16 * 1024,    // 16KB
        MaxJsonDepth = 5,
        MaxObjectProperties = 50,
        
        // Cryptographic verification
        RequireMessageSigning = true,
        
        // Rate limiting
        EnableRateLimiting = true,
        MaxMessagesPerSecond = 5
    };
}
```

## Security Best Practices

### Do
- **Use HTTPS in production** - Always require secure transport
- **Enable sandbox protection** - Isolate iframe content appropriately
- **Validate message origins** - Only accept messages from trusted domains
- **Monitor security violations** - Log and respond to security events
- **Use environment-specific configs** - Different security for dev vs prod
- **Regular security audits** - Review and update security settings
- **Apply principle of least privilege** - Use most restrictive settings possible

### Don't
- **Disable security features** without understanding the implications
- **Use overly permissive settings** in production environments
- **Ignore security violations** - Always investigate unusual activity
- **Hard-code secrets** in client-side code
- **Allow untrusted origins** without proper validation
- **Set very large limits** that could enable DoS attacks
- **Mix development and production** security settings

## Security Validation

### Configuration Validation

```csharp
public static void ValidateSecurityConfiguration(MessageSecurityOptions options)
{
    var validation = options.ValidateConfiguration();
    
    if (!validation.IsValid)
    {
        var errors = string.Join(", ", validation.Errors);
        throw new InvalidOperationException($"Security configuration invalid: {errors}");
    }
    
    // Log warnings for review
    foreach (var warning in validation.Warnings)
    {
        Logger.LogWarning("Security configuration warning: {Warning}", warning);
    }
}
```

### Runtime Security Checks

```csharp
public class SecurityValidator
{
    public bool ValidateIframeSource(string src, MessageSecurityOptions options)
    {
        // Check HTTPS requirement
        if (options.RequireHttps && !src.StartsWith("https://"))
        {
            return false;
        }
        
        // Check allowed protocols
        var uri = new Uri(src);
        if (!options.AllowedProtocols.Contains(uri.Scheme))
        {
            return false;
        }
        
        // Check origin whitelist
        if (options.ValidateOrigins && 
            !options.AllowedOrigins.Contains(uri.GetLeftPart(UriPartial.Authority)))
        {
            return false;
        }
        
        return true;
    }
}
```

## Security Testing

### Security Test Suite

```csharp
[TestClass]
public class SecurityConfigurationTests
{
    [TestMethod]
    public void Production_Configuration_Should_Be_Secure()
    {
        var options = new MessageSecurityOptions().ForProduction();
        var validation = options.ValidateConfiguration();
        
        Assert.IsTrue(validation.IsValid);
        Assert.IsTrue(options.RequireHttps);
        Assert.IsTrue(options.EnableStrictValidation);
        Assert.IsTrue(options.EnableSandbox);
    }
    
    [TestMethod]
    public void Payment_Configuration_Should_Be_Maximum_Security()
    {
        var options = new MessageSecurityOptions().ForPaymentWidget();
        
        Assert.IsTrue(options.RequireHttps);
        Assert.AreEqual(SandboxPreset.Paranoid, options.SandboxPreset);
        Assert.IsTrue(options.FilterMaliciousContent);
        Assert.IsTrue(options.ValidateOrigins);
    }
}
```

---

## Security Reference

### Security Presets Quick Reference
| Feature | Development | Production | Payment | Trusted |
|---------|------------|------------|---------|---------|
| HTTPS Required | :x: | :white_check_mark: | :white_check_mark: | :x: |
| Sandbox Level | Permissive | Basic | Paranoid | None |
| Message Validation | Basic | Strict | Maximum | Minimal |
| Origin Validation | :x: | :white_check_mark: | :white_check_mark: | :x: |
| Content Filtering | :x: | :white_check_mark: | :white_check_mark: | :x: |
| Rate Limiting | :x: | :x: | :white_check_mark: | :x: |

### Security Headers Reference
| Header | Purpose | Recommended Value |
|--------|---------|-------------------|
| X-Frame-Options | Prevent clickjacking | `SAMEORIGIN` or `DENY` |
| X-Content-Type-Options | Prevent MIME sniffing | `nosniff` |
| X-XSS-Protection | Enable XSS filtering | `1; mode=block` |
| Strict-Transport-Security | Enforce HTTPS | `max-age=31536000; includeSubDomains` |

---
