# Configuration Validation

**? New in BlazorFrame v2.1**

BlazorFrame includes comprehensive configuration validation that detects conflicts, invalid settings, and potential security issues in real-time. This helps prevent configuration errors and provides guidance for optimal security settings.

## Overview

Configuration validation automatically:
- **Detects conflicts** between different security settings
- **Validates parameter values** for correctness
- **Provides warnings** for potentially insecure configurations
- **Suggests improvements** for better security
- **Fires security events** for configuration errors

## Validation Triggers

Validation occurs automatically:
- **On component initialization** - When BlazorFrame is first rendered
- **On parameter changes** - When SecurityOptions or other parameters change
- **On explicit validation** - When calling `ValidateConfiguration()` manually

## Validation Result Structure

```csharp
public class ConfigurationValidationResult
{
    public bool IsValid { get; }           // True if no errors
    public List<string> Errors { get; }    // Critical configuration errors
    public List<string> Warnings { get; }  // Potential issues
    public List<string> Suggestions { get; } // Improvement recommendations
}
```

## Common Configuration Issues

### HTTPS Configuration Conflicts

#### Problem: RequireHttps + AllowInsecureConnections
```razor
<BlazorFrame Src="http://example.com"
            SecurityOptions="@conflictingOptions" />

@code {
    private readonly MessageSecurityOptions conflictingOptions = new()
    {
        RequireHttps = true,              // Require HTTPS
        AllowInsecureConnections = true   // But also allow HTTP
    };
    // Warning: "RequireHttps is true but AllowInsecureConnections is also true..."
}
```

#### Solution: Clear Intent
```razor
@code {
    // For development - allow HTTP temporarily
    private readonly MessageSecurityOptions devOptions = new()
    {
        RequireHttps = false,             // Clear: don't require HTTPS in dev
        AllowInsecureConnections = true   // Allow HTTP for localhost
    };
    
    // For production - strict HTTPS
    private readonly MessageSecurityOptions prodOptions = new()
    {
        RequireHttps = true,              // Require HTTPS
        AllowInsecureConnections = false  // No HTTP allowed
    };
}
```

### Sandbox Configuration Conflicts

#### Problem: Disabled Sandbox with Preset
```razor
@code {
    private readonly MessageSecurityOptions sandboxConflict = new()
    {
        EnableSandbox = false,               // Sandbox disabled
        SandboxPreset = SandboxPreset.Strict // But preset is set
    };
    // Warning: "EnableSandbox is false but SandboxPreset is set..."
}
```

#### Solution: Consistent Configuration
```razor
@code {
    // Option 1: Enable sandbox with preset
    private readonly MessageSecurityOptions enabledSandbox = new()
    {
        EnableSandbox = true,
        SandboxPreset = SandboxPreset.Strict
    };
    
    // Option 2: Disable sandbox completely
    private readonly MessageSecurityOptions disabledSandbox = new()
    {
        EnableSandbox = false,
        SandboxPreset = SandboxPreset.None
    };
}
```

### Invalid Parameter Values

#### Problem: Invalid Numeric Values
```razor
@code {
    private readonly MessageSecurityOptions invalidOptions = new()
    {
        MaxMessageSize = -1,        // Error: Must be > 0
        MaxJsonDepth = 0,           // Error: Must be > 0
        MaxObjectProperties = -5    // Error: Must be > 0
    };
}
```

#### Solution: Valid Ranges
```razor
@code {
    private readonly MessageSecurityOptions validOptions = new()
    {
        MaxMessageSize = 32 * 1024,     // 32KB - valid
        MaxJsonDepth = 10,              // 10 levels - valid
        MaxObjectProperties = 100       // 100 properties - valid
    };
}
```

## Real-Time Validation Example

```razor
@page "/validation-demo"
@using BlazorFrame

<div class="container">
    <h2>Configuration Validation Demo</h2>
    
    <!-- Configuration controls -->
    <div class="row mb-4">
        <div class="col-md-6">
            <h4>Security Settings</h4>
            
            <div class="form-check">
                <input class="form-check-input" type="checkbox" 
                       @bind="requireHttps" @onchange="UpdateConfiguration" id="requireHttps">
                <label class="form-check-label" for="requireHttps">
                    Require HTTPS
                </label>
            </div>
            
            <div class="form-check">
                <input class="form-check-input" type="checkbox" 
                       @bind="allowInsecure" @onchange="UpdateConfiguration" id="allowInsecure">
                <label class="form-check-label" for="allowInsecure">
                    Allow Insecure Connections
                </label>
            </div>
            
            <div class="form-check">
                <input class="form-check-input" type="checkbox" 
                       @bind="enableSandbox" @onchange="UpdateConfiguration" id="enableSandbox">
                <label class="form-check-label" for="enableSandbox">
                    Enable Sandbox
                </label>
            </div>
            
            <div class="form-group">
                <label>Sandbox Preset:</label>
                <select class="form-select" @bind="sandboxPreset" @onchange="UpdateConfiguration">
                    <option value="@SandboxPreset.None">None</option>
                    <option value="@SandboxPreset.Basic">Basic</option>
                    <option value="@SandboxPreset.Permissive">Permissive</option>
                    <option value="@SandboxPreset.Strict">Strict</option>
                    <option value="@SandboxPreset.Paranoid">Paranoid</option>
                </select>
            </div>
        </div>
        
        <div class="col-md-6">
            <h4>Validation Results</h4>
            
            <div class="validation-status">
                <span class="badge @(validationResult.IsValid ? "bg-success" : "bg-danger")">
                    @(validationResult.IsValid ? "? Valid" : "? Issues Found")
                </span>
            </div>
            
            @if (validationResult.Errors.Any())
            {
                <div class="alert alert-danger mt-2">
                    <h6>? Errors:</h6>
                    <ul class="mb-0">
                        @foreach (var error in validationResult.Errors)
                        {
                            <li>@error</li>
                        }
                    </ul>
                </div>
            }
            
            @if (validationResult.Warnings.Any())
            {
                <div class="alert alert-warning mt-2">
                    <h6>?? Warnings:</h6>
                    <ul class="mb-0">
                        @foreach (var warning in validationResult.Warnings)
                        {
                            <li>@warning</li>
                        }
                    </ul>
                </div>
            }
            
            @if (validationResult.Suggestions.Any())
            {
                <div class="alert alert-info mt-2">
                    <h6>?? Suggestions:</h6>
                    <ul class="mb-0">
                        @foreach (var suggestion in validationResult.Suggestions)
                        {
                            <li>@suggestion</li>
                        }
                    </ul>
                </div>
            }
        </div>
    </div>
    
    <!-- BlazorFrame with current configuration -->
    <div class="row">
        <div class="col-12">
            <h4>Current Configuration</h4>
            <div class="config-display mb-3">
                <strong>Effective Sandbox:</strong> @(currentOptions.GetEffectiveSandboxValue() ?? "none")<br>
                <strong>HTTPS Required:</strong> @currentOptions.RequireHttps<br>
                <strong>Allow Insecure:</strong> @currentOptions.AllowInsecureConnections<br>
            </div>
            
            <BlazorFrame Src="https://httpbin.org/html"
                        Width="100%"
                        Height="300px"
                        SecurityOptions="@currentOptions"
                        OnSecurityViolation="HandleConfigurationViolation"
                        class="border rounded" />
        </div>
    </div>
</div>

@code {
    // Configuration state
    private bool requireHttps = false;
    private bool allowInsecure = false;
    private bool enableSandbox = false;
    private SandboxPreset sandboxPreset = SandboxPreset.None;
    
    // Current options and validation result
    private MessageSecurityOptions currentOptions = new();
    private ConfigurationValidationResult validationResult = new();

    protected override void OnInitialized()
    {
        UpdateConfiguration();
    }

    private void UpdateConfiguration()
    {
        // Update current options based on UI state
        currentOptions = new MessageSecurityOptions
        {
            RequireHttps = requireHttps,
            AllowInsecureConnections = allowInsecure,
            EnableSandbox = enableSandbox,
            SandboxPreset = sandboxPreset,
            EnableStrictValidation = true,
            LogSecurityViolations = true
        };
        
        // Validate the configuration
        validationResult = currentOptions.ValidateConfiguration();
        
        StateHasChanged();
    }

    private async Task HandleConfigurationViolation(IframeMessage violation)
    {
        if (violation.MessageType == "configuration-validation")
        {
            Console.WriteLine($"Configuration violation: {violation.ValidationError}");
        }
    }
}
```

## Programmatic Validation

### Manual Validation
```razor
@code {
    private void ValidateMyConfiguration()
    {
        var options = new MessageSecurityOptions()
            .ForProduction()
            .WithBasicSandbox();
            
        var validation = options.ValidateConfiguration();
        
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                Logger.LogError("Configuration error: {Error}", error);
            }
        }
        
        foreach (var warning in validation.Warnings)
        {
            Logger.LogWarning("Configuration warning: {Warning}", warning);
        }
    }
}
```

### Validation with Exception Throwing
```razor
@code {
    private void StrictValidation()
    {
        try
        {
            var options = new MessageSecurityOptions
            {
                MaxMessageSize = -1,  // Invalid!
                RequireHttps = true,
                AllowInsecureConnections = true
            };
            
            options.ValidateAndThrow();  // Throws InvalidOperationException
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogError(ex, "Configuration validation failed");
            // Handle validation failure
        }
    }
}
```

## Environment-Aware Validation

### Development Environment
```razor
@code {
    private MessageSecurityOptions GetDevelopmentOptions()
    {
        var options = new MessageSecurityOptions()
            .ForDevelopment();
            
        var validation = options.ValidateConfiguration();
        
        // In development, log warnings but don't fail
        foreach (var warning in validation.Warnings)
        {
            Console.WriteLine($"[DEV] Config warning: {warning}");
        }
        
        return options;
    }
}
```

### Production Environment
```razor
@code {
    private MessageSecurityOptions GetProductionOptions()
    {
        var options = new MessageSecurityOptions()
            .ForProduction()
            .ValidateAndThrow();  // Fail fast in production
            
        return options;
    }
}
```

## Security Event Integration

### Handling Configuration Violations
```razor
<BlazorFrame SecurityOptions="@securityOptions"
            OnSecurityViolation="HandleAllViolations" />

@code {
    private async Task HandleAllViolations(IframeMessage violation)
    {
        switch (violation.MessageType)
        {
            case "configuration-validation":
                await HandleConfigurationViolation(violation);
                break;
                
            case "url-validation":
                await HandleUrlViolation(violation);
                break;
                
            case "message-validation":
                await HandleMessageViolation(violation);
                break;
                
            default:
                await HandleGenericViolation(violation);
                break;
        }
    }

    private async Task HandleConfigurationViolation(IframeMessage violation)
    {
        Logger.LogError("Configuration validation failed: {Error}", violation.ValidationError);
        
        // Could show user notification
        await ShowConfigurationError(violation.ValidationError);
        
        // Could disable component or fallback to safe defaults
        await ApplySafeDefaults();
    }
}
```

## Best Practices

### Do
- **Validate early** - Use `.ValidateAndThrow()` during application startup
- **Monitor warnings** - Log and review configuration warnings regularly
- **Test configurations** - Validate different environment configurations
- **Handle violations** - Implement proper error handling for configuration issues
- **Document intent** - Clear comments when using conflicting settings intentionally

### Don't
- **Ignore warnings** - Configuration warnings often indicate real issues
- **Suppress validation** - Don't disable validation to hide problems
- **Use conflicting settings** without understanding the implications
- **Mix development and production** settings in the same environment

## Validation Reference

### Error Conditions
| Condition | Error Message |
|-----------|---------------|
| `MaxMessageSize <= 0` | "MaxMessageSize must be greater than 0" |
| `MaxJsonDepth <= 0` | "MaxJsonDepth must be greater than 0" |
| `MaxObjectProperties <= 0` | "MaxObjectProperties must be greater than 0" |
| `MaxArrayElements <= 0` | "MaxArrayElements must be greater than 0" |

### Warning Conditions
| Condition | Warning Message |
|-----------|-----------------|
| `RequireHttps && AllowInsecureConnections` | "RequireHttps is true but AllowInsecureConnections is also true..." |
| `!EnableSandbox && !string.IsNullOrEmpty(Sandbox)` | "EnableSandbox is false but Sandbox property is set..." |
| `!EnableSandbox && SandboxPreset != None` | "EnableSandbox is false but SandboxPreset is set..." |
| `AllowScriptProtocols` | "AllowScriptProtocols=true allows dangerous URLs..." |
| `!EnableStrictValidation` | "EnableStrictValidation=false reduces security..." |

### Suggestion Conditions
| Condition | Suggestion |
|-----------|------------|
| Production + No Sandbox | "Consider enabling sandbox protection for additional security" |
| Development + RequireHttps | "Consider setting RequireHttps=false for development" |
| Large MaxMessageSize | "Consider reducing MaxMessageSize to 64KB-1MB" |
| High MaxJsonDepth | "Consider reducing MaxJsonDepth to prevent DoS attacks" |

---

**Related Topics:**
- [Security Overview](overview.md) - Complete security feature overview
- [Sandbox Security](sandbox.md) - Iframe sandbox configuration
- [HTTPS Enforcement](https-enforcement.md) - Transport security
- [MessageSecurityOptions API](../api/message-security-options.md) - Complete API reference