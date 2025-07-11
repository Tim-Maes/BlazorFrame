# Troubleshooting Guide

Common issues, solutions, and debugging techniques for BlazorFrame. This guide covers security violations, configuration problems, performance issues, and integration challenges.

## Common Issues

### Security Violations

#### Origin Validation Failures

**Symptoms:**
- Messages not received from iframe
- Security violation events fired
- Console warnings about unauthorized origins

**Causes & Solutions:**

1. **Auto-derived origin mismatch**
```razor
<!-- Problem: Iframe redirects to different domain -->
<BlazorFrame Src="https://example.com/redirect-to-other-domain" />

<!-- Solution: Explicitly set allowed origins -->
<BlazorFrame Src="https://example.com/redirect-to-other-domain"
            AllowedOrigins="@allowedOrigins" />

@code {
    private readonly List<string> allowedOrigins = new()
    {
        "https://example.com",
        "https://other-domain.com",  // Add the redirect target
        "https://cdn.example.com"
    };
}
```

2. **Protocol mismatch (HTTP vs HTTPS)**
```razor
<!-- Problem: Mixed protocols -->
<BlazorFrame Src="http://example.com"  <!-- HTTP -->
            AllowedOrigins="@httpsOrigins" />

@code {
    private readonly List<string> httpsOrigins = new() 
    { 
        "https://example.com"  // HTTPS - won't match HTTP
    };
}

<!-- Solution: Match protocols exactly -->
@code {
    private readonly List<string> matchingOrigins = new() 
    { 
        "http://example.com"   // Match the actual protocol
    };
}
```

3. **Port number differences**
```razor
<!-- Problem: Port mismatch -->
<BlazorFrame Src="https://example.com:8080/widget"
            AllowedOrigins="@portMismatch" />

@code {
    private readonly List<string> portMismatch = new() 
    { 
        "https://example.com"  // Missing port :8080
    };
}

<!-- Solution: Include exact port -->
@code {
    private readonly List<string> exactOrigin = new() 
    { 
        "https://example.com:8080"  // Include port
    };
}
```

#### Message Size Violations

**Symptoms:**
- Large messages rejected
- Security violations for message size

**Diagnosis:**
```razor
<BlazorFrame OnSecurityViolation="DiagnoseMessageSize" />

@code {
    private Task DiagnoseMessageSize(IframeMessage violation)
    {
        if (violation.ValidationError?.Contains("size") == true)
        {
            Console.WriteLine($"Message size: {violation.Data.Length} bytes");
            Console.WriteLine($"Max allowed: {SecurityOptions.MaxMessageSize} bytes");
        }
        return Task.CompletedTask;
    }
}
```

**Solutions:**
```razor
@code {
    // Option 1: Increase message size limit
    private readonly MessageSecurityOptions largerLimit = new()
    {
        MaxMessageSize = 128 * 1024  // 128KB instead of 64KB
    };
    
    // Option 2: Use compression in iframe content
    // iframe: compress messages before sending
    // parent: decompress on receive
    
    // Option 3: Split large messages
    // iframe: send messages in chunks
    // parent: reassemble chunks
}
```

#### JSON Validation Failures

**Symptoms:**
- Valid-looking JSON rejected
- Validation errors for JSON structure

**Common Causes:**
```javascript
// Problem 1: JSON too deeply nested
{
  "level1": {
    "level2": {
      "level3": {
        "level4": {
          "level5": {
            "level6": "too deep"  // Exceeds MaxJsonDepth
          }
        }
      }
    }
  }
}

// Problem 2: Too many object properties
{
  "prop1": "value1",
  "prop2": "value2",
  // ... 150 properties (exceeds MaxObjectProperties)
}

// Problem 3: Too many array elements
{
  "data": [1, 2, 3, ... 2000]  // Exceeds MaxArrayElements
}
```

**Solutions:**
```razor
@code {
    // Option 1: Adjust validation limits
    private readonly MessageSecurityOptions relaxedValidation = new()
    {
        MaxJsonDepth = 20,           // Allow deeper nesting
        MaxObjectProperties = 500,    // Allow more properties
        MaxArrayElements = 5000      // Allow larger arrays
    };
    
    // Option 2: Disable strict validation (not recommended)
    private readonly MessageSecurityOptions noValidation = new()
    {
        EnableStrictValidation = false
    };
    
    // Option 3: Restructure iframe messages (recommended)
    // Design flatter JSON structures
    // Use pagination for large arrays
    // Split complex objects into smaller messages
}
```

### Configuration Issues

#### Sandbox Configuration Conflicts

**Problem:** Conflicting sandbox settings
```razor
@code {
    private readonly MessageSecurityOptions conflicting = new()
    {
        EnableSandbox = false,               // Disabled
        SandboxPreset = SandboxPreset.Strict, // But preset set
        Sandbox = "allow-scripts"            // And explicit value set
    };
}
```

**Solution:** Use configuration validation
```razor
@code {
    private void FixSandboxConfiguration()
    {
        var options = new MessageSecurityOptions()
            .WithBasicSandbox()  // Clear, simple configuration
            .Validate();         // Check for issues
            
        if (!options.ValidateConfiguration().IsValid)
        {
            // Fix the issues or use safe defaults
            options = new MessageSecurityOptions().ForProduction();
        }
    }
}
```

#### HTTPS Configuration Problems

**Problem:** HTTPS enforcement blocking legitimate content
```razor
@code {
    private readonly MessageSecurityOptions tooStrict = new()
    {
        RequireHttps = true,
        AllowInsecureConnections = false
    };
}

// This blocks legitimate localhost development
// <BlazorFrame Src="http://localhost:3000" SecurityOptions="@tooStrict" />
```

**Solution:** Environment-aware configuration
```razor
@inject IWebHostEnvironment Environment

@code {
    private MessageSecurityOptions GetEnvironmentConfig()
    {
        if (Environment.IsDevelopment())
        {
            return new MessageSecurityOptions()
                .ForDevelopment();  // Allows HTTP for localhost
        }
        
        return new MessageSecurityOptions()
            .ForProduction();       // Strict HTTPS in production
    }
}
```

### Cross-Origin Issues

#### Auto-Resize Not Working

**Symptoms:**
- Iframe not resizing automatically
- Fixed height despite EnableAutoResize="true"

**Cause:** Cross-origin access restrictions
```razor
<!-- Cross-origin iframe - auto-resize won't work -->
<BlazorFrame Src="https://external-domain.com"
            EnableAutoResize="true" />  <!-- Won't work -->
```

**Solutions:**
```razor
<!-- Option 1: Disable auto-resize, set fixed height -->
<BlazorFrame Src="https://external-domain.com"
            EnableAutoResize="false"
            Height="500px"
            EnableScroll="true" />

<!-- Option 2: Use iframe content cooperation -->
<BlazorFrame Src="https://cooperative-domain.com"
            EnableAutoResize="true"
            OnValidatedMessage="HandleResizeMessage" />

@code {
    private Task HandleResizeMessage(IframeMessage message)
    {
        // iframe sends resize messages via postMessage
        if (message.MessageType == "resize")
        {
            // BlazorFrame handles this automatically
        }
        return Task.CompletedTask;
    }
}
```

#### CSP Blocking Iframe Content

**Symptoms:**
- Iframe content not loading
- CSP violation errors in browser console

**Diagnosis:**
1. Open browser Developer Tools (F12)
2. Check Console for CSP violations
3. Look for messages like "Refused to frame ... because it violates CSP"

**Solutions:**
```razor
<!-- Problem: Restrictive CSP -->
<BlazorFrame Src="https://widget.example.com"
            CspOptions="@restrictiveCsp" />

@code {
    private readonly CspOptions restrictiveCsp = new CspOptions()
        .AllowSelf();  // Only allows same-origin - too restrictive
}

<!-- Solution: Add frame sources -->
<BlazorFrame Src="https://widget.example.com"
            CspOptions="@permissiveCsp" />

@code {
    private readonly CspOptions permissiveCsp = new CspOptions()
        .AllowSelf()
        .AllowFrameSources("https://widget.example.com")  // Allow the iframe source
        .AllowHttpsFrames();  // Allow any HTTPS iframe
}
```

### Performance Issues

#### Slow Iframe Loading

**Symptoms:**
- Long loading times
- Poor user experience

**Solutions:**
```razor
<!-- Add loading indicator -->
<div class="iframe-container">
    @if (!isLoaded)
    {
        <div class="loading-indicator">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    }
    
    <BlazorFrame Src="@iframeUrl"
                OnLoad="HandleLoad"
                class="@(isLoaded ? "" : "d-none")" />
</div>

@code {
    private bool isLoaded = false;
    
    private Task HandleLoad()
    {
        isLoaded = true;
        StateHasChanged();
        return Task.CompletedTask;
    }
}

<!-- Use lazy loading for off-screen iframes -->
<BlazorFrame Src="@iframeUrl"
            loading="lazy"  <!-- Browser native lazy loading -->
            class="lazy-iframe" />
```

#### Memory Leaks

**Symptoms:**
- Increasing memory usage over time
- Browser performance degradation

**Solutions:**
```razor
@implements IAsyncDisposable

<BlazorFrame @ref="blazorFrame" Src="@iframeUrl" />

@code {
    private BlazorFrame.BlazorFrame? blazorFrame;
    
    public async ValueTask DisposeAsync()
    {
        // BlazorFrame automatically disposes resources
        if (blazorFrame != null)
        {
            await blazorFrame.DisposeAsync();
        }
    }
}

<!-- Limit number of concurrent iframes -->
@code {
    private const int MaxConcurrentIframes = 5;
    private readonly Queue<string> iframeQueue = new();
    
    private void LoadNextIframe()
    {
        if (activeIframes.Count < MaxConcurrentIframes && iframeQueue.Count > 0)
        {
            var nextUrl = iframeQueue.Dequeue();
            // Load next iframe
        }
    }
}
```

## Debugging Techniques

### Enable Detailed Logging

```razor
@code {
    private readonly MessageSecurityOptions debugOptions = new()
    {
        LogSecurityViolations = true,  // Enable violation logging
        EnableStrictValidation = true
    };
}

<BlazorFrame SecurityOptions="@debugOptions"
            OnValidatedMessage="LogMessage"
            OnSecurityViolation="LogViolation"
            OnInitializationError="LogError" />

@code {
    private Task LogMessage(IframeMessage message)
    {
        Logger.LogInformation("Message from {Origin}: {Data}", 
            message.Origin, message.Data);
        return Task.CompletedTask;
    }
    
    private Task LogViolation(IframeMessage violation)
    {
        Logger.LogWarning("Security violation: {Error} from {Origin}", 
            violation.ValidationError, violation.Origin);
        return Task.CompletedTask;
    }
    
    private Task LogError(Exception ex)
    {
        Logger.LogError(ex, "BlazorFrame initialization failed");
        return Task.CompletedTask;
    }
}
```

### Browser Developer Tools

1. **Open Developer Tools** (F12)
2. **Console Tab** - Check for JavaScript errors and warnings
3. **Network Tab** - Verify iframe sources are loading
4. **Security Tab** - Check for mixed content issues
5. **Elements Tab** - Inspect iframe HTML and attributes

### Configuration Validation Testing

```razor
@page "/debug-config"

<div class="container">
    <h2>Configuration Debug</h2>
    
    <button class="btn btn-primary" @onclick="TestConfigurations">
        Test All Configurations
    </button>
    
    <div class="debug-results mt-3">
        @foreach (var result in debugResults)
        {
            <div class="alert @GetAlertClass(result.IsValid)">
                <h5>@result.Name</h5>
                <p><strong>Valid:</strong> @result.IsValid</p>
                
                @if (result.Errors.Any())
                {
                    <h6>Errors:</h6>
                    <ul>
                        @foreach (var error in result.Errors)
                        {
                            <li>@error</li>
                        }
                    </ul>
                }
                
                @if (result.Warnings.Any())
                {
                    <h6>Warnings:</h6>
                    <ul>
                        @foreach (var warning in result.Warnings)
                        {
                            <li>@warning</li>
                        }
                    </ul>
                }
            </div>
        }
    </div>
</div>

@code {
    private List<DebugResult> debugResults = new();
    
    private void TestConfigurations()
    {
        debugResults.Clear();
        
        var configurations = new Dictionary<string, MessageSecurityOptions>
        {
            ["Development"] = new MessageSecurityOptions().ForDevelopment(),
            ["Production"] = new MessageSecurityOptions().ForProduction(),
            ["Payment"] = new MessageSecurityOptions().ForPaymentWidget(),
            ["Trusted"] = new MessageSecurityOptions().ForTrustedContent(),
            ["Conflicting"] = new MessageSecurityOptions 
            { 
                RequireHttps = true, 
                AllowInsecureConnections = true 
            },
            ["Invalid"] = new MessageSecurityOptions 
            { 
                MaxMessageSize = -1 
            }
        };
        
        foreach (var config in configurations)
        {
            var validation = config.Value.ValidateConfiguration();
            debugResults.Add(new DebugResult
            {
                Name = config.Key,
                IsValid = validation.IsValid,
                Errors = validation.Errors,
                Warnings = validation.Warnings
            });
        }
        
        StateHasChanged();
    }
    
    private string GetAlertClass(bool isValid) => 
        isValid ? "alert-success" : "alert-danger";
    
    public class DebugResult
    {
        public string Name { get; set; } = "";
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
```

## Getting Help

### Information to Include

When reporting issues, include:

1. **BlazorFrame version** (check NuGet package)
2. **.NET version** and project type (Server/WASM)
3. **Browser and version**
4. **Complete error messages** from browser console
5. **Minimal reproduction code**
6. **Security configuration** being used

### Example Issue Report

```
BlazorFrame Version: 2.1.0
.NET Version: 8.0
Browser: Chrome 118
Project Type: Blazor Server

Issue: Security violations when loading trusted widget

Error Messages:
- "Origin 'https://widget.example.com' is not in the allowed origins list"

Configuration:
var options = new MessageSecurityOptions
{
    AllowedOrigins = new() { "https://example.com" },  // Missing widget domain
    EnableStrictValidation = true
};

Expected: Widget should load successfully
Actual: Security violation prevents widget loading
```

### Community Resources

- **GitHub Issues**: [BlazorFrame Issues](https://github.com/Tim-Maes/BlazorFrame/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Tim-Maes/BlazorFrame/discussions)
- **Documentation**: [Complete Documentation](../index.md)
- **Examples**: [Common Scenarios](../examples/common-scenarios.md)

---
