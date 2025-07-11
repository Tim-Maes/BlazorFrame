# MessageSecurityOptions API Reference

Complete API reference for the `MessageSecurityOptions` class, which provides comprehensive security configuration for BlazorFrame components.

## Class Overview

```csharp
public class MessageSecurityOptions
{
    // Core message validation
    public List<string>? AllowedOrigins { get; set; }
    public bool EnableStrictValidation { get; set; } = true;
    public int MaxMessageSize { get; set; } = 64 * 1024; // 64KB
    public bool LogSecurityViolations { get; set; } = true;
    
    // JSON structure limits
    public int MaxJsonDepth { get; set; } = 10;
    public int MaxObjectProperties { get; set; } = 100;
    public int MaxArrayElements { get; set; } = 1000;
    
    // Protocol and custom validation
    public bool AllowScriptProtocols { get; set; } = false;
    public Func<string, string, bool>? CustomValidator { get; set; }
    
    // Sandbox security (v2.1+)
    public string? Sandbox { get; set; } = null;
    public SandboxPreset SandboxPreset { get; set; } = SandboxPreset.None;
    public bool EnableSandbox { get; set; } = false;
    
    // Transport security (v2.1+)
    public bool RequireHttps { get; set; } = false;
    public bool AllowInsecureConnections { get; set; } = false;
    
    // Methods
    public string? GetEffectiveSandboxValue();
    public ConfigurationValidationResult ValidateConfiguration();
}
```

## Properties

### Core Message Validation

#### AllowedOrigins
**Type:** `List<string>?`  
**Default:** `null`  
**Description:** List of allowed origins for postMessage communication. If null or empty, origins are auto-derived from the iframe source URL.

```csharp
var options = new MessageSecurityOptions
{
    AllowedOrigins = new List<string>
    {
        "https://widget.example.com",
        "https://api.example.com",
        "https://cdn.example.com"
    }
};
```

**Special Values:**
- `null` - Auto-derive from iframe Src URL
- Empty list - No origins allowed (blocks all messages)
- `["*"]` - Allow all origins (not recommended for security)

#### EnableStrictValidation
**Type:** `bool`  
**Default:** `true`  
**Description:** Whether to perform strict JSON format validation on incoming messages.

```csharp
// Strict validation (recommended)
var options = new MessageSecurityOptions
{
    EnableStrictValidation = true
};

// Relaxed validation (for development)
var devOptions = new MessageSecurityOptions
{
    EnableStrictValidation = false
};
```

When enabled:
- Validates JSON structure and syntax
- Enforces JSON depth and complexity limits
- Checks for malicious content patterns
- Validates message type format

#### MaxMessageSize
**Type:** `int`  
**Default:** `64 * 1024` (64KB)  
**Description:** Maximum message size in bytes to prevent DoS attacks.

```csharp
// Different size limits for different environments
var devOptions = new MessageSecurityOptions
{
    MaxMessageSize = 128 * 1024  // 128KB for development
};

var prodOptions = new MessageSecurityOptions
{
    MaxMessageSize = 32 * 1024   // 32KB for production
};

var paymentOptions = new MessageSecurityOptions
{
    MaxMessageSize = 16 * 1024   // 16KB for payment widgets
};
```

#### LogSecurityViolations
**Type:** `bool`  
**Default:** `true`  
**Description:** Whether to log security violations for monitoring and debugging.

```csharp
var options = new MessageSecurityOptions
{
    LogSecurityViolations = true  // Log violations for monitoring
};
```

### JSON Structure Limits

#### MaxJsonDepth
**Type:** `int`  
**Default:** `10`  
**Description:** Maximum JSON nesting depth allowed to prevent deeply nested JSON attacks.

```csharp
var options = new MessageSecurityOptions
{
    MaxJsonDepth = 5  // Limit to 5 levels of nesting
};

// Example: This would be rejected with MaxJsonDepth = 3
// {"level1": {"level2": {"level3": {"level4": "too deep"}}}}
```

#### MaxObjectProperties
**Type:** `int`  
**Default:** `100`  
**Description:** Maximum number of properties allowed in a JSON object.

```csharp
var options = new MessageSecurityOptions
{
    MaxObjectProperties = 50  // Limit objects to 50 properties
};
```

#### MaxArrayElements
**Type:** `int`  
**Default:** `1000`  
**Description:** Maximum number of elements allowed in a JSON array.

```csharp
var options = new MessageSecurityOptions
{
    MaxArrayElements = 500  // Limit arrays to 500 elements
};
```

### Protocol and Custom Validation

#### AllowScriptProtocols

**Type:** `bool`  
**Default:** `false`  
**Description:** Whether to allow JavaScript protocol URLs (javascript:, vbscript:, etc.).

```csharp
// Generally should remain false for security
var options = new MessageSecurityOptions
{
    AllowScriptProtocols = false  // Block javascript: URLs
};

// Only enable if absolutely necessary
var legacyOptions = new MessageSecurityOptions
{
    AllowScriptProtocols = true  // Security risk
};
```

#### CustomValidator

**Type:** `Func<string, string, bool>?`  
**Default:** `null`  
**Description:** Custom validation function for additional security checks.

```csharp
var options = new MessageSecurityOptions
{
    CustomValidator = (origin, messageJson) =>
    {
        // Custom validation logic
        if (origin.Contains("untrusted-domain.com"))
            return false;
            
        if (messageJson.Contains("malicious-pattern"))
            return false;
            
        // Additional business logic validation
        return ValidateBusinessRules(messageJson);
    }
};

// More complex custom validator
var advancedOptions = new MessageSecurityOptions
{
    CustomValidator = (origin, messageJson) =>
    {
        try
        {
            var message = JsonSerializer.Deserialize<CustomMessage>(messageJson);
            return message.IsValid && message.Timestamp > DateTime.UtcNow.AddMinutes(-5);
        }
        catch
        {
            return false;
        }
    }
};
```

### Sandbox Security (New in v2.1)

#### Sandbox

**Type:** `string?`  
**Default:** `null`  
**Description:** Explicit iframe sandbox attributes. Takes precedence over SandboxPreset.

```csharp
var options = new MessageSecurityOptions
{
    Sandbox = "allow-scripts allow-same-origin allow-forms"
};

// Using SandboxHelper for type safety
var safeOptions = new MessageSecurityOptions
{
    Sandbox = SandboxHelper.CreateCustomSandbox(
        allowScripts: true,
        allowSameOrigin: true,
        allowForms: true,
        allowPopups: false
    )
};
```

#### SandboxPreset

**Type:** `SandboxPreset`  
**Default:** `SandboxPreset.None`  
**Description:** Predefined sandbox configuration preset.

```csharp
public enum SandboxPreset
{
    None,       // No sandbox restrictions
    Basic,      // allow-scripts allow-same-origin
    Permissive, // allow-scripts allow-same-origin allow-forms allow-popups
    Strict,     // allow-scripts allow-same-origin (no forms/popups)
    Paranoid    // allow-scripts (maximum isolation)
}

var options = new MessageSecurityOptions
{
    SandboxPreset = SandboxPreset.Strict,
    EnableSandbox = true
};
```

#### EnableSandbox

**Type:** `bool`  
**Default:** `false`  
**Description:** Enable automatic sandbox with safe defaults.

```csharp
var options = new MessageSecurityOptions
{
    EnableSandbox = true,  // Enables Basic sandbox if no other config
    SandboxPreset = SandboxPreset.Strict  // Override default
};
```

### Transport Security (New in v2.1)

#### RequireHttps

**Type:** `bool`  
**Default:** `false`  
**Description:** Require HTTPS for iframe sources to ensure transport security.

```csharp
var prodOptions = new MessageSecurityOptions
{
    RequireHttps = true  // Reject HTTP URLs in production
};
```

#### AllowInsecureConnections

**Type:** `bool`  
**Default:** `false`  
**Description:** Allow insecure (HTTP) connections even when RequireHttps is true.

```csharp
var devOptions = new MessageSecurityOptions
{
    RequireHttps = true,
    AllowInsecureConnections = true  // Allow HTTP in development
};
```

## Methods

### GetEffectiveSandboxValue()

**Returns:** `string?`  
**Description:** Gets the effective sandbox value based on configuration priority.

```csharp
var options = new MessageSecurityOptions
{
    Sandbox = "allow-scripts",  // Explicit value
    SandboxPreset = SandboxPreset.Strict,  // Ignored
    EnableSandbox = true  // Ignored
};

string? effective = options.GetEffectiveSandboxValue();
// Returns: "allow-scripts"
```

**Priority Order:**
1. Explicit `Sandbox` property
2. `SandboxPreset` (if not None)
3. `EnableSandbox` with Basic preset
4. `null` (no sandbox)

### ValidateConfiguration()
**Returns:** `ConfigurationValidationResult`  
**Description:** Validates the current configuration for conflicts and issues.

```csharp
var options = new MessageSecurityOptions
{
    RequireHttps = true,
    AllowInsecureConnections = true,  // Conflict!
    MaxMessageSize = -1  // Invalid!
};

var validation = options.ValidateConfiguration();

Console.WriteLine($"Valid: {validation.IsValid}");
foreach (var error in validation.Errors)
{
    Console.WriteLine($"Error: {error}");
}
foreach (var warning in validation.Warnings)
{
    Console.WriteLine($"Warning: {warning}");
}
```

## Extension Methods

BlazorFrame provides fluent extension methods for easier configuration:

### Sandbox Extensions
```csharp
using BlazorFrame;

var options = new MessageSecurityOptions()
    .WithBasicSandbox()      // SandboxPreset.Basic + EnableSandbox = true
    .WithPermissiveSandbox() // SandboxPreset.Permissive + EnableSandbox = true
    .WithStrictSandbox()     // SandboxPreset.Strict + EnableSandbox = true
    .WithParanoidSandbox()   // SandboxPreset.Paranoid + EnableSandbox = true
    .WithCustomSandbox("allow-scripts allow-forms")  // Custom sandbox
    .WithoutSandbox();       // Disable all sandbox
```

### Security Extensions
```csharp
var options = new MessageSecurityOptions()
    .RequireHttps(allowInsecureInDevelopment: true)
    .ForDevelopment()        // Development-friendly settings
    .ForProduction()         // Production-ready security
    .ForPaymentWidget()      // Maximum security for payments
    .ForTrustedContent();    // Balanced security for trusted sources
```

### Validation Extensions
```csharp
var options = new MessageSecurityOptions()
    .ForProduction()
    .ValidateAndThrow();     // Throws if configuration is invalid

var validation = options.Validate();  // Returns validation result
```

## Configuration Examples

### Basic Configuration
```csharp
var basic = new MessageSecurityOptions
{
    EnableStrictValidation = true,
    MaxMessageSize = 32 * 1024,
    LogSecurityViolations = true
};
```

### Production Configuration
```csharp
var production = new MessageSecurityOptions
{
    EnableStrictValidation = true,
    MaxMessageSize = 32 * 1024,
    MaxJsonDepth = 5,
    MaxObjectProperties = 50,
    MaxArrayElements = 100,
    AllowScriptProtocols = false,
    SandboxPreset = SandboxPreset.Strict,
    EnableSandbox = true,
    RequireHttps = true,
    AllowInsecureConnections = false,
    LogSecurityViolations = true
};
```

### Development Configuration
```csharp
var development = new MessageSecurityOptions
{
    EnableStrictValidation = false,
    MaxMessageSize = 128 * 1024,
    MaxJsonDepth = 20,
    AllowScriptProtocols = false,
    SandboxPreset = SandboxPreset.Permissive,
    EnableSandbox = true,
    RequireHttps = false,
    AllowInsecureConnections = true,
    LogSecurityViolations = true
};
```

### Payment Widget Configuration

```csharp
var payment = new MessageSecurityOptions
{
    EnableStrictValidation = true,
    MaxMessageSize = 16 * 1024,
    MaxJsonDepth = 3,
    MaxObjectProperties = 20,
    MaxArrayElements = 50,
    AllowScriptProtocols = false,
    SandboxPreset = SandboxPreset.Strict,
    EnableSandbox = true,
    RequireHttps = true,
    AllowInsecureConnections = false,
    LogSecurityViolations = true,
    AllowedOrigins = new List<string> 
    { 
        "https://secure-payment-provider.com" 
    }
};
```

### Custom Validation Configuration
```csharp
var custom = new MessageSecurityOptions
{
    EnableStrictValidation = true,
    CustomValidator = (origin, messageJson) =>
    {
        // Only allow messages with specific structure
        try
        {
            var json = JsonDocument.Parse(messageJson);
            return json.RootElement.TryGetProperty("apiKey", out _) &&
                   json.RootElement.TryGetProperty("timestamp", out _);
        }
        catch
        {
            return false;
        }
    },
    SandboxPreset = SandboxPreset.Basic,
    EnableSandbox = true
};
```

## Configuration Validation Results

The `ValidateConfiguration()` method returns a `ConfigurationValidationResult`:

```csharp
public class ConfigurationValidationResult
{
    public bool IsValid { get; }           // No errors present
    public List<string> Errors { get; }    // Critical configuration errors
    public List<string> Warnings { get; }  // Potential issues
    public List<string> Suggestions { get; } // Improvement recommendations
}
```

### Common Validation Issues

#### Configuration Conflicts
```csharp
// Warning: RequireHttps + AllowInsecureConnections
var conflicting = new MessageSecurityOptions
{
    RequireHttps = true,
    AllowInsecureConnections = true
};
// Warning: "RequireHttps is true but AllowInsecureConnections is also true..."
```

#### Invalid Values
```csharp
// Error: Invalid property values
var invalid = new MessageSecurityOptions
{
    MaxMessageSize = -1,     // Error: "MaxMessageSize must be greater than 0"
    MaxJsonDepth = 0,        // Error: "MaxJsonDepth must be greater than 0"
    MaxObjectProperties = -5  // Error: "MaxObjectProperties must be greater than 0"
};
```

#### Security Warnings
```csharp
// Warning: Insecure configuration
var insecure = new MessageSecurityOptions
{
    AllowScriptProtocols = true,  // Warning: Allows dangerous URLs
    EnableStrictValidation = false // Warning: Reduces security
};
```

---
