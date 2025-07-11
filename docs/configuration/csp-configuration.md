# Content Security Policy Configuration

**CSP header generation and security policy configuration for BlazorFrame**

Content Security Policy (CSP) provides an additional layer of security by controlling which resources can be loaded and executed. BlazorFrame includes comprehensive CSP support with automatic header generation and validation.

## CSP Overview

BlazorFrame's CSP features include:
- **Automatic CSP Header Generation** - Build CSP headers based on iframe requirements
- **Environment-Aware Policies** - Different CSP rules for development vs production
- **Fluent Configuration API** - Easy-to-use builder pattern for CSP rules
- **Validation and Recommendations** - Built-in CSP validation with security suggestions

## Basic CSP Configuration

### Simple CSP Setup

```razor
<BlazorFrame Src="https://widget.example.com"
            CspOptions="@cspOptions"
            OnCspHeaderGenerated="ApplyCspHeader" />

@code {
    private readonly CspOptions cspOptions = new CspOptions()
        .ForProduction()
        .AllowFrameSources("https://widget.example.com")
        .WithScriptNonce("widget-nonce-123");
    
    private async Task ApplyCspHeader(CspHeader cspHeader)
    {
        // Apply CSP header to HTTP response
        HttpContext.Response.Headers.Add(cspHeader.HeaderName, cspHeader.HeaderValue);
        
        Logger.LogInformation("Applied CSP header: {Header}", cspHeader.HeaderValue);
    }
}
```

### Environment-Specific CSP

```razor
@code {
    private CspOptions GetCspOptions()
    {
        if (Environment.IsDevelopment())
        {
            return new CspOptions()
                .ForDevelopment()           // Relaxed CSP for development
                .AllowFrameSources("http://localhost:3000", "https://widget.example.com");
        }
        else
        {
            return new CspOptions()
                .ForProduction()            // Strict CSP for production
                .AllowFrameSources("https://widget.example.com")
                .UseStrictDynamic()
                .WithScriptNonce(GetSecureNonce());
        }
    }
}
```

## CSP Directive Configuration

### Frame Sources (frame-src)

```csharp
var cspOptions = new CspOptions()
    .AllowFrameSource("https://widget.example.com")           // Single source
    .AllowFrameSources(                                       // Multiple sources
        "https://widget.example.com",
        "https://api.example.com",
        "https://cdn.example.com"
    )
    .AllowSelf()                                             // Allow same origin
    .AllowDataUrls()                                         // Allow data: URLs
    .AllowBlobUrls()                                         // Allow blob: URLs
    .AllowHttpsFrames();                                     // Allow any HTTPS source
```

### Script Sources (script-src)

```csharp
var cspOptions = new CspOptions()
    .AllowScriptSource("'self'")                            // Same origin scripts
    .AllowScriptSources(                                    // Multiple script sources
        "'self'",
        "https://cdn.jsdelivr.net",
        "https://unpkg.com"
    )
    .WithScriptNonce("secure-nonce-123")                    // Nonce-based scripts
    .UseStrictDynamic();                                    // Strict dynamic policy
```

### Custom Directives

```csharp
var cspOptions = new CspOptions()
    .WithCustomDirective("connect-src", "'self'", "https://api.example.com")
    .WithCustomDirective("img-src", "'self'", "data:", "https:")
    .WithCustomDirective("style-src", "'self'", "'unsafe-inline'")
    .WithCustomDirective("font-src", "'self'", "https://fonts.gstatic.com");
```

### Complete CSP Configuration

```csharp
var comprehensiveCsp = new CspOptions
{
    // Frame sources
    FrameSrc = new List<string> 
    { 
        "'self'", 
        "https://widget.example.com",
        "https://trusted-partner.com"
    },
    
    // Script sources
    ScriptSrc = new List<string> 
    { 
        "'self'", 
        "https://cdn.jsdelivr.net" 
    },
    
    // Style sources
    StyleSrc = new List<string> 
    { 
        "'self'", 
        "'unsafe-inline'",
        "https://fonts.googleapis.com" 
    },
    
    // Image sources
    ImgSrc = new List<string> 
    { 
        "'self'", 
        "data:", 
        "https:" 
    },
    
    // Connection sources
    ConnectSrc = new List<string> 
    { 
        "'self'", 
        "https://api.example.com" 
    },
    
    // Font sources
    FontSrc = new List<string> 
    { 
        "'self'", 
        "https://fonts.gstatic.com" 
    },
    
    // Frame ancestors (who can frame this page)
    FrameAncestors = new List<string> 
    { 
        "'self'" 
    },
    
    // Report URI for violations
    ReportUri = "https://csp-report.example.com/report",
    
    // Additional options
    AllowInlineScripts = false,
    AllowEval = false,
    UseStrictDynamic = true,
    ReportOnly = false
};
```

## CSP Presets and Builders

### Environment Presets

```csharp
// Development - Relaxed CSP for easier development
var devCsp = new CspOptions()
    .ForDevelopment();
    
// Equivalent to:
var devCsp = new CspOptions
{
    AllowInlineScripts = true,
    AllowEval = true,
    ScriptSrc = new List<string> { "'self'", "'unsafe-inline'", "'unsafe-eval'" },
    FrameAncestors = new List<string> { "'self'" },
    AutoDeriveFrameSrc = true
};

// Production - Strict CSP for security
var prodCsp = new CspOptions()
    .ForProduction();
    
// Equivalent to:
var prodCsp = new CspOptions
{
    AllowInlineScripts = false,
    AllowEval = false,
    UseStrictDynamic = true,
    ScriptSrc = new List<string> { "'self'" },
    FrameAncestors = new List<string> { "'self'" },
    AutoDeriveFrameSrc = true
};
```

### Security Level Presets

```csharp
// Strict security - Maximum protection
var strictCsp = new CspOptions()
    .UseStrictSecurity()
    .AllowFrameSources("https://trusted-widget.com");

// Balanced security - Good security with flexibility
var balancedCsp = new CspOptions()
    .UseBalancedSecurity()
    .AllowFrameSources("https://widget.example.com");

// Permissive security - Minimal restrictions
var permissiveCsp = new CspOptions()
    .UsePermissiveSecurity()
    .AllowFrameSources("https://widget.example.com");
```

### Fluent Builder Pattern

```csharp
var csp = new CspOptions()
    .ForProduction()                                        // Start with production preset
    .AllowFrameSources("https://widget.example.com")        // Add frame sources
    .AllowScriptSources("https://cdn.example.com")          // Add script sources
    .WithScriptNonce("nonce-123")                          // Add script nonce
    .UseStrictDynamic()                                    // Enable strict-dynamic
    .WithReportUri("https://csp-report.example.com")       // Add report URI
    .AsReportOnly()                                        // Enable report-only mode
    .ValidateAndBuild();                                   // Validate and build
```

## Nonce and Hash Management

### Script Nonces

```razor
<BlazorFrame Src="@widgetUrl"
            CspOptions="@GetCspWithNonce()"
            OnCspHeaderGenerated="ApplyCspHeader" />

@code {
    private string currentNonce = "";
    
    protected override void OnInitialized()
    {
        // Generate a new nonce for each page load
        currentNonce = GenerateSecureNonce();
    }
    
    private CspOptions GetCspWithNonce()
    {
        return new CspOptions()
            .ForProduction()
            .AllowFrameSources("https://widget.example.com")
            .WithScriptNonce(currentNonce);
    }
    
    private string GenerateSecureNonce()
    {
        var bytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
```

### Script Hashes

```csharp
public class CspHashGenerator
{
    public static string GenerateScriptHash(string scriptContent)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(scriptContent));
        return $"'sha256-{Convert.ToBase64String(hash)}'";
    }
}

// Usage
var cspOptions = new CspOptions()
    .AllowScriptSources(
        "'self'",
        CspHashGenerator.GenerateScriptHash("console.log('Hello World');")
    );
```

### Dynamic Nonce Management

```csharp
public class NonceManager
{
    private readonly Dictionary<string, string> pageNonces = new();
    
    public string GetOrCreateNonce(string pageId)
    {
        if (!pageNonces.TryGetValue(pageId, out var nonce))
        {
            nonce = GenerateNonce();
            pageNonces[pageId] = nonce;
            
            // Auto-expire nonce after 1 hour
            _ = Task.Delay(TimeSpan.FromHours(1))
                .ContinueWith(_ => pageNonces.Remove(pageId));
        }
        
        return nonce;
    }
    
    private string GenerateNonce()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
```

## CSP Validation and Monitoring

### Configuration Validation

```razor
@code {
    private void ValidateCspConfiguration()
    {
        var cspOptions = new CspOptions()
            .ForProduction()
            .AllowFrameSources("https://widget.example.com");
            
        var validation = cspBuilderService.ValidateCspOptions(cspOptions);
        
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                Logger.LogError("CSP configuration error: {Error}", error);
            }
        }
        
        foreach (var warning in validation.Warnings)
        {
            Logger.LogWarning("CSP configuration warning: {Warning}", warning);
        }
        
        foreach (var suggestion in validation.Suggestions)
        {
            Logger.LogInformation("CSP suggestion: {Suggestion}", suggestion);
        }
    }
}
```

### CSP Violation Reporting

```csharp
[ApiController]
[Route("api/[controller]")]
public class CspReportController : ControllerBase
{
    private readonly ILogger<CspReportController> _logger;
    
    public CspReportController(ILogger<CspReportController> logger)
    {
        _logger = logger;
    }
    
    [HttpPost("violation")]
    public async Task<IActionResult> ReportViolation([FromBody] CspViolationReport report)
    {
        _logger.LogWarning("CSP Violation: {Document} - {Directive}: {BlockedUri}", 
            report.DocumentUri, report.ViolatedDirective, report.BlockedUri);
            
        // Store violation for analysis
        await StoreViolationReport(report);
        
        // Alert if critical violation
        if (IsCriticalViolation(report))
        {
            await AlertSecurityTeam(report);
        }
        
        return Ok();
    }
    
    private bool IsCriticalViolation(CspViolationReport report)
    {
        // Define what constitutes a critical violation
        return report.ViolatedDirective == "script-src" && 
               report.BlockedUri.Contains("javascript:");
    }
}

public class CspViolationReport
{
    public string DocumentUri { get; set; } = "";
    public string ViolatedDirective { get; set; } = "";
    public string BlockedUri { get; set; } = "";
    public string OriginalPolicy { get; set; } = "";
}
```

### Real-Time CSP Monitoring

```razor
<BlazorFrame Src="@widgetUrl"
            CspOptions="@monitoringCspOptions"
            OnCspHeaderGenerated="SetupCspMonitoring" />

@code {
    private readonly CspOptions monitoringCspOptions = new CspOptions()
        .ForProduction()
        .AllowFrameSources("https://widget.example.com")
        .WithReportUri("/api/csp/violation")
        .AsReportOnly();  // Start with report-only for monitoring
    
    private async Task SetupCspMonitoring(CspHeader cspHeader)
    {
        // Apply CSP header
        HttpContext.Response.Headers.Add(cspHeader.HeaderName, cspHeader.HeaderValue);
        
        // Setup client-side violation monitoring
        await JSRuntime.InvokeVoidAsync("setupCspMonitoring", new
        {
            reportUri = "/api/csp/violation",
            enableRealTimeAlerts = true
        });
    }
}
```

## Advanced CSP Scenarios

### Multi-Tenant CSP Configuration

```csharp
public class TenantCspProvider
{
    public CspOptions GetCspForTenant(string tenantId)
    {
        var tenant = GetTenant(tenantId);
        
        var csp = new CspOptions()
            .ForProduction()
            .AllowSelf();
            
        // Add tenant-specific frame sources
        foreach (var allowedDomain in tenant.AllowedIframeDomains)
        {
            csp.AllowFrameSource(allowedDomain);
        }
        
        // Add tenant-specific script sources
        foreach (var scriptDomain in tenant.AllowedScriptDomains)
        {
            csp.AllowScriptSource(scriptDomain);
        }
        
        // Apply tenant security level
        switch (tenant.SecurityLevel)
        {
            case SecurityLevel.High:
                csp.UseStrictDynamic();
                break;
            case SecurityLevel.Medium:
                csp.UseBalancedSecurity();
                break;
            case SecurityLevel.Low:
                csp.UsePermissiveSecurity();
                break;
        }
        
        return csp;
    }
}
```

### Dynamic CSP Updates

```razor
@code {
    private async Task UpdateCspForNewWidget(string widgetUrl)
    {
        var uri = new Uri(widgetUrl);
        var origin = uri.GetLeftPart(UriPartial.Authority);
        
        // Check if origin is already allowed
        if (!currentCspOptions.FrameSrc.Contains(origin))
        {
            // Create updated CSP
            var updatedCsp = currentCspOptions.Clone()
                .AllowFrameSource(origin);
                
            // Validate the updated CSP
            var validation = cspBuilderService.ValidateCspOptions(updatedCsp);
            
            if (validation.IsValid)
            {
                // Apply updated CSP
                currentCspOptions = updatedCsp;
                await ApplyUpdatedCsp();
            }
            else
            {
                Logger.LogWarning("Invalid CSP update for origin: {Origin}", origin);
            }
        }
    }
    
    private async Task ApplyUpdatedCsp()
    {
        var cspHeader = cspBuilderService.BuildCspHeader(currentCspOptions);
        
        // Update response headers if possible
        if (!HttpContext.Response.HasStarted)
        {
            HttpContext.Response.Headers[cspHeader.HeaderName] = cspHeader.HeaderValue;
        }
        
        // Log the CSP update
        Logger.LogInformation("Updated CSP header: {Header}", cspHeader.HeaderValue);
    }
}
```

### CSP Testing and Debugging

```razor
@page "/csp-test"

<div class="csp-test-page">
    <h2>CSP Configuration Test</h2>
    
    <div class="form-group">
        <label>Test URL:</label>
        <input @bind="testUrl" class="form-control" />
        <button @onclick="TestCspWithUrl" class="btn btn-primary">Test CSP</button>
    </div>
    
    <div class="csp-results">
        <h3>Current CSP Configuration</h3>
        <pre>@currentCspHeader</pre>
        
        <h3>Validation Results</h3>
        @if (validationResult != null)
        {
            <div class="validation-results">
                <p><strong>Valid:</strong> @validationResult.IsValid</p>
                
                @if (validationResult.Errors.Any())
                {
                    <div class="alert alert-danger">
                        <h6>Errors:</h6>
                        <ul>
                            @foreach (var error in validationResult.Errors)
                            {
                                <li>@error</li>
                            }
                        </ul>
                    </div>
                }
                
                @if (validationResult.Warnings.Any())
                {
                    <div class="alert alert-warning">
                        <h6>Warnings:</h6>
                        <ul>
                            @foreach (var warning in validationResult.Warnings)
                            {
                                <li>@warning</li>
                            }
                        </ul>
                    </div>
                }
            </div>
        }
        
        <h3>Test Iframe</h3>
        @if (!string.IsNullOrEmpty(testUrl))
        {
            <BlazorFrame Src="@testUrl"
                        CspOptions="@testCspOptions"
                        OnCspHeaderGenerated="HandleTestCspGenerated" />
        }
    </div>
</div>

@code {
    private string testUrl = "";
    private string currentCspHeader = "";
    private CspValidationResult? validationResult;
    private CspOptions testCspOptions = new CspOptions().ForDevelopment();
    
    private async Task TestCspWithUrl()
    {
        if (string.IsNullOrEmpty(testUrl)) return;
        
        try
        {
            var uri = new Uri(testUrl);
            var origin = uri.GetLeftPart(UriPartial.Authority);
            
            // Create CSP for testing
            testCspOptions = new CspOptions()
                .ForProduction()
                .AllowFrameSource(origin);
                
            // Validate CSP
            validationResult = cspBuilderService.ValidateCspOptions(testCspOptions);
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error testing CSP with URL: {Url}", testUrl);
        }
    }
    
    private async Task HandleTestCspGenerated(CspHeader cspHeader)
    {
        currentCspHeader = $"{cspHeader.HeaderName}: {cspHeader.HeaderValue}";
        StateHasChanged();
    }
}
```

## CSP Best Practices

### Do
- **Use nonces or hashes** instead of 'unsafe-inline' for scripts
- **Start with report-only mode** when implementing CSP
- **Monitor CSP violations** and investigate unusual activity  
- **Use strict-dynamic** for modern applications with dynamic script loading
- **Validate CSP configuration** before deploying to production
- **Use environment-specific CSP** policies (dev vs prod)
- **Keep CSP as restrictive as possible** while maintaining functionality

### Don't
- **Use 'unsafe-inline' or 'unsafe-eval'** in production unless absolutely necessary
- **Allow wildcard sources** (*) without careful consideration
- **Ignore CSP violations** - they often indicate real security issues
- **Use overly permissive CSP** that defeats the security purpose
- **Hard-code nonces** - generate them dynamically for each request
- **Forget to test CSP changes** - they can break application functionality
- **Mix HTTP and HTTPS sources** unnecessarily

## CSP Reference

### Common CSP Directives
| Directive | Purpose | Example Values |
|-----------|---------|----------------|
| `default-src` | Fallback for other directives | `'self'`, `'none'` |
| `script-src` | Control script sources | `'self'`, `'nonce-xyz'`, `'strict-dynamic'` |
| `frame-src` | Control iframe sources | `'self'`, `https://widget.com` |
| `style-src` | Control stylesheet sources | `'self'`, `'unsafe-inline'` |
| `img-src` | Control image sources | `'self'`, `data:`, `https:` |
| `connect-src` | Control AJAX/WebSocket sources | `'self'`, `https://api.com` |
| `font-src` | Control font sources | `'self'`, `https://fonts.gstatic.com` |
| `frame-ancestors` | Control who can frame this page | `'self'`, `'none'` |

### CSP Keywords
| Keyword | Meaning |
|---------|---------|
| `'self'` | Same origin as the document |
| `'none'` | No sources allowed |
| `'unsafe-inline'` | Allow inline scripts/styles |
| `'unsafe-eval'` | Allow eval() and similar |
| `'strict-dynamic'` | Trust scripts loaded by trusted scripts |
| `'nonce-xyz'` | Allow scripts/styles with matching nonce |
| `'sha256-xyz'` | Allow scripts/styles matching hash |

---
