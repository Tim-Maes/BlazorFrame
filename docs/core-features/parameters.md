# Component Parameters

Complete reference for all BlazorFrame component parameters, their types, default values, and usage examples.

## Core Parameters

### Src

**Type:** `string`  
**Default:** `""`  
**Description:** The URL to load in the iframe.

```razor
<!-- Basic URL -->
<BlazorFrame Src="https://example.com" />

<!-- Data URL -->
<BlazorFrame Src="data:text/html,<h1>Hello World!</h1>" />

<!-- Relative URL -->
<BlazorFrame Src="./local-content.html" />

<!-- Dynamic URL -->
<BlazorFrame Src="@currentUrl" />

@code {
    private string currentUrl = "https://widget.example.com";
}
```

### Width

**Type:** `string`  
**Default:** `"100%"`  
**Description:** The width of the iframe. Supports CSS width values.

```razor
<!-- Percentage width -->
<BlazorFrame Src="https://example.com" Width="100%" />

<!-- Fixed pixel width -->
<BlazorFrame Src="https://example.com" Width="800px" />

<!-- CSS calc() function -->
<BlazorFrame Src="https://example.com" Width="calc(100% - 40px)" />

<!-- Responsive width -->
<BlazorFrame Src="https://example.com" Width="@GetResponsiveWidth()" />

@code {
    private string GetResponsiveWidth() => 
        DeviceDetection.IsMobile ? "100%" : "80%";
}
```

### Height
**Type:** `string`  
**Default:** `"600px"`  
**Description:** The initial height of the iframe. Can be overridden by auto-resize.

```razor
<!-- Fixed height -->
<BlazorFrame Src="https://example.com" Height="400px" />

<!-- Viewport-relative height -->
<BlazorFrame Src="https://example.com" Height="50vh" />

<!-- Dynamic height -->
<BlazorFrame Src="https://example.com" Height="@($"{calculatedHeight}px")" />

@code {
    private int calculatedHeight = 500;
}
```

### EnableAutoResize
**Type:** `bool`  
**Default:** `true`  
**Description:** Whether to automatically resize the iframe based on content height.

```razor
<!-- Auto-resize enabled (default) -->
<BlazorFrame Src="https://example.com" EnableAutoResize="true" />

<!-- Auto-resize disabled for fixed height -->
<BlazorFrame Src="https://example.com" 
            EnableAutoResize="false" 
            Height="500px" />

<!-- Conditional auto-resize -->
<BlazorFrame Src="https://example.com" 
            EnableAutoResize="@allowResize" />

@code {
    private bool allowResize = !IsCrossOrigin(currentUrl);
}
```

### ResizeOptions
**Type:** `ResizeOptions?`  
**Default:** `null`  
**Description:** Configuration options for auto-resize behavior including min/max height, polling interval, and debouncing.

```razor
<!-- Custom resize configuration -->
<BlazorFrame Src="https://example.com"
            EnableAutoResize="true"
            ResizeOptions="@customResizeOptions" />

<!-- Using built-in presets -->
<BlazorFrame Src="https://example.com"
            EnableAutoResize="true"
            ResizeOptions="@ResizeOptions.Performance" />

@code {
    // Custom configuration
    private readonly ResizeOptions customResizeOptions = new()
    {
        MinHeight = 200,
        MaxHeight = 1500,
        PollingInterval = 500,
        DebounceMs = 100,
        UseResizeObserver = true
    };
}
```

**ResizeOptions Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `MinHeight` | `int` | `100` | Minimum height in pixels |
| `MaxHeight` | `int` | `50000` | Maximum height in pixels |
| `PollingInterval` | `int` | `500` | Fallback polling interval (ms) when ResizeObserver is unavailable |
| `DebounceMs` | `int` | `100` | Debounce delay to prevent excessive updates (set to 0 to disable) |
| `UseResizeObserver` | `bool` | `true` | Use the modern ResizeObserver API when available |

**Built-in Presets:**

| Preset | PollingInterval | DebounceMs | Best For |
|--------|-----------------|------------|----------|
| `ResizeOptions.Default` | 500ms | 100ms | Most use cases |
| `ResizeOptions.Performance` | 1000ms | 250ms | Many iframes, mobile devices |
| `ResizeOptions.Responsive` | 250ms | 50ms | Dynamic content, smooth animations |

### EnableScroll
**Type:** `bool`  
**Default:** `false`  
**Description:** Whether to enable scrolling within the iframe wrapper.

```razor
<!-- Scrolling disabled (default) -->
<BlazorFrame Src="https://example.com" EnableScroll="false" />

<!-- Scrolling enabled -->
<BlazorFrame Src="https://example.com" 
            EnableScroll="true" 
            Height="400px" />

<!-- Conditional scrolling -->
<BlazorFrame Src="https://example.com" 
            EnableScroll="@(contentHeight > maxHeight)" />
```

### EnableNavigationTracking
**Type:** `bool`  
**Default:** `false`  
**Description:** Enable navigation event tracking to capture URL changes with parameters. Only works for same-origin iframes due to browser security restrictions.

```razor
<!-- Enable navigation tracking -->
<BlazorFrame Src="https://same-origin.example.com" 
            EnableNavigationTracking="true"
            OnNavigation="HandleNavigation"
            OnUrlChanged="HandleUrlChanged" />

<!-- Conditional navigation tracking -->
<BlazorFrame Src="@widgetUrl" 
            EnableNavigationTracking="@IsSameOrigin(widgetUrl)"
            OnNavigation="HandleNavigation" />

@code {
    private bool IsSameOrigin(string url) => 
        new Uri(url).Host == new Uri(NavigationManager.BaseUri).Host;
        
    private Task HandleNavigation(NavigationEvent navigation)
    {
        Logger.LogInformation("Navigation to {Url} with {ParamCount} parameters", 
            navigation.Url, navigation.QueryParameters.Count);
        return Task.CompletedTask;
    }
    
    private Task HandleUrlChanged(string newUrl)
    {
        Logger.LogInformation("URL changed to: {Url}", newUrl);
        return Task.CompletedTask;
    }
}
```

## Security Parameters

### AllowedOrigins
**Type:** `List<string>?`  
**Default:** `null` (auto-derives from Src)  
**Description:** Explicit list of allowed origins for postMessage communication.

```razor
<!-- Auto-derived origins (default) -->
<BlazorFrame Src="https://example.com" />

<!-- Explicit single origin -->
<BlazorFrame Src="https://widget.example.com"
            AllowedOrigins="@singleOrigin" />

<!-- Multiple allowed origins -->
<BlazorFrame Src="https://widget.example.com"
            AllowedOrigins="@multipleOrigins" />

@code {
    private readonly List<string> singleOrigin = new() 
    { 
        "https://widget.example.com" 
    };
    
    private readonly List<string> multipleOrigins = new()
    {
        "https://widget.example.com",
        "https://api.example.com",
        "https://cdn.example.com"
    };
}
```

### SecurityOptions
**Type:** `MessageSecurityOptions`  
**Default:** `new()` (default security settings)  
**Description:** Comprehensive security configuration options.

```razor
<!-- Default security options -->
<BlazorFrame Src="https://example.com" />

<!-- Custom security options -->
<BlazorFrame Src="https://example.com"
            SecurityOptions="@customSecurity" />

<!-- Fluent security configuration -->
<BlazorFrame Src="https://example.com"
            SecurityOptions="@fluentSecurity" />

@code {
    private readonly MessageSecurityOptions customSecurity = new()
    {
        EnableStrictValidation = true,
        MaxMessageSize = 32 * 1024,
        LogSecurityViolations = true,
        SandboxPreset = SandboxPreset.Basic,
        RequireHttps = true
    };
    
    private readonly MessageSecurityOptions fluentSecurity = new MessageSecurityOptions()
        .ForProduction()
        .WithBasicSandbox()
        .RequireHttps();
}
```

### CspOptions
**Type:** `CspOptions?`  
**Default:** `null` (no CSP generation)  
**Description:** Content Security Policy configuration options.

```razor
<!-- No CSP (default) -->
<BlazorFrame Src="https://example.com" />

<!-- Basic CSP configuration -->
<BlazorFrame Src="https://example.com"
            CspOptions="@basicCsp" />

<!-- Advanced CSP with custom directives -->
<BlazorFrame Src="https://example.com"
            CspOptions="@advancedCsp" />

@code {
    private readonly CspOptions basicCsp = new CspOptions()
        .AllowSelf()
        .AllowFrameSources("https://example.com");
    
    private readonly CspOptions advancedCsp = new CspOptions()
        .ForProduction()
        .AllowFrameSources("https://widget.example.com")
        .WithScriptNonce("secure-nonce-123")
        .WithCustomDirective("img-src", "'self'", "data:", "https:");
}
```

## Event Parameters

### OnLoad
**Type:** `EventCallback`  
**Description:** Fired when the iframe loads successfully.

```razor
<BlazorFrame Src="https://example.com" OnLoad="HandleLoad" />

@code {
    private Task HandleLoad()
    {
        Console.WriteLine("Iframe loaded!");
        return Task.CompletedTask;
    }
    
    private async Task HandleLoadAsync()
    {
        await LogEvent("Iframe loaded");
        StateHasChanged();
    }
}
```

### OnMessage
**Type:** `EventCallback<string>`  
**Description:** Fired when receiving valid postMessage (legacy - use OnValidatedMessage instead).

```razor
<BlazorFrame Src="https://example.com" OnMessage="HandleMessage" />

@code {
    private Task HandleMessage(string messageJson)
    {
        Console.WriteLine($"Received: {messageJson}");
        return Task.CompletedTask;
    }
}
```

### OnValidatedMessage
**Type:** `EventCallback<IframeMessage>`  
**Description:** Fired when receiving a validated message with full details.

```razor
<BlazorFrame Src="https://example.com" OnValidatedMessage="HandleValidatedMessage" />

@code {
    private Task HandleValidatedMessage(IframeMessage message)
    {
        Console.WriteLine($"From {message.Origin}: {message.Data}");
        Console.WriteLine($"Type: {message.MessageType}");
        return Task.CompletedTask;
    }
}
```

### OnSecurityViolation
**Type:** `EventCallback<IframeMessage>`  
**Description:** Fired when a security violation occurs.

```razor
<BlazorFrame Src="https://example.com" OnSecurityViolation="HandleViolation" />

@code {
    private Task HandleViolation(IframeMessage violation)
    {
        Logger.LogWarning("Security violation: {Error}", violation.ValidationError);
        
        // Handle different types of violations
        return violation.MessageType switch
        {
            "origin-validation" => HandleOriginViolation(violation),
            "message-validation" => HandleMessageViolation(violation),
            "url-validation" => HandleUrlViolation(violation),
            "configuration-validation" => HandleConfigViolation(violation),
            _ => Task.CompletedTask
        };
    }
}
```

### OnInitializationError
**Type:** `EventCallback<Exception>`  
**Description:** Fired when JavaScript initialization fails.

```razor
<BlazorFrame Src="https://example.com" OnInitializationError="HandleInitError" />

@code {
    private Task HandleInitError(Exception ex)
    {
        Logger.LogError(ex, "BlazorFrame initialization failed");
        // Show user-friendly error message
        ShowErrorMessage("Failed to load content. Please try again.");
        return Task.CompletedTask;
    }
}
```

### OnCspHeaderGenerated
**Type:** `EventCallback<CspHeader>`  
**Description:** Fired when CSP header is generated.

```razor
<BlazorFrame Src="https://example.com" 
            CspOptions="@cspOptions"
            OnCspHeaderGenerated="HandleCspGenerated" />

@code {
    private Task HandleCspGenerated(CspHeader cspHeader)
    {
        // Apply CSP header to HTTP response
        if (HttpContext?.Response != null)
        {
            HttpContext.Response.Headers.Add(
                cspHeader.HeaderName, 
                cspHeader.HeaderValue);
        }
        
        Logger.LogDebug("CSP Header: {Header}", cspHeader.HeaderValue);
        return Task.CompletedTask;
    }
}
```

### OnMessageSent
**Type:** `EventCallback<string>`  
**Description:** Fired when a message is successfully sent to the iframe.

```razor
<BlazorFrame Src="https://example.com" OnMessageSent="HandleMessageSent" />

@code {
    private Task HandleMessageSent(string messageJson)
    {
        Logger.LogDebug("Message sent successfully: {Message}", messageJson);
        return Task.CompletedTask;
    }
}
```

### OnMessageSendFailed
**Type:** `EventCallback<Exception>`  
**Description:** Fired when sending a message to the iframe fails.

```razor
<BlazorFrame Src="https://example.com" OnMessageSendFailed="HandleSendFailure" />

@code {
    private Task HandleSendFailure(Exception ex)
    {
        Logger.LogError(ex, "Failed to send message to iframe");
        // Handle the failure appropriately
        ShowErrorToUser("Communication with widget failed");
        return Task.CompletedTask;
    }
}
```

### OnNavigation
**Type:** `EventCallback<NavigationEvent>`  
**Description:** Fired when iframe navigation occurs with complete URL details and query parameters.

```razor
<BlazorFrame Src="https://example.com" 
            EnableNavigationTracking="true"
            OnNavigation="HandleNavigation" />

@code {
    private Task HandleNavigation(NavigationEvent navigation)
    {
        Logger.LogInformation("Navigation detected:");
        Logger.LogInformation("  URL: {Url}", navigation.Url);
        Logger.LogInformation("  Pathname: {Pathname}", navigation.Pathname);
        Logger.LogInformation("  Query: {Query}", navigation.Search);
        Logger.LogInformation("  Hash: {Hash}", navigation.Hash);
        Logger.LogInformation("  Type: {Type}", navigation.NavigationType);
        Logger.LogInformation("  Same Origin: {SameOrigin}", navigation.IsSameOrigin);
        
        foreach (var param in navigation.QueryParameters)
        {
            Logger.LogInformation("  Query Param: {Key} = {Value}", param.Key, param.Value);
        }
        
        return Task.CompletedTask;
    }
}
```

### OnUrlChanged
**Type:** `EventCallback<string>`  
**Description:** Fired when URL changes are detected (simpler alternative to OnNavigation).

```razor
<BlazorFrame Src="https://example.com" 
            EnableNavigationTracking="true"
            OnUrlChanged="HandleUrlChanged" />

@code {
    private Task HandleUrlChanged(string newUrl)
    {
        Logger.LogInformation("URL changed to: {Url}", newUrl);
        
        // Parse URL manually if needed
        var uri = new Uri(newUrl);
        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        
        return Task.CompletedTask;
    }
}
```

## Component Methods

### ReloadAsync()
**Returns:** `Task`  
**Description:** Reloads the iframe content by forcing a re-render of the iframe element.

```razor
<BlazorFrame @ref="iframeRef" Src="https://example.com/document.pdf" />

<button @onclick="RefreshContent">Refresh</button>

@code {
    private BlazorFrame? iframeRef;
    
    private async Task RefreshContent()
    {
        if (iframeRef != null)
        {
            await iframeRef.ReloadAsync();
        }
    }
}
```

### ReloadAsync(string newSrc)
**Returns:** `Task`  
**Description:** Reloads the iframe with a new source URL.

```razor
<BlazorFrame @ref="iframeRef" Src="@currentUrl" />

@code {
    private BlazorFrame? iframeRef;
    private string currentUrl = "https://example.com/doc1.pdf";
    
    private async Task LoadDifferentDocument()
    {
        if (iframeRef != null)
        {
            // Load a new URL with cache-busting
            var newUrl = $"https://example.com/doc2.pdf?v={DateTime.UtcNow.Ticks}";
            await iframeRef.ReloadAsync(newUrl);
        }
    }
}
```

### SendMessageAsync(object data, string? targetOrigin)
**Returns:** `Task<bool>`  
**Description:** Sends a message to the iframe content.

```razor
<BlazorFrame @ref="iframeRef" Src="https://example.com" />

@code {
    private BlazorFrame? iframeRef;
    
    private async Task SendData()
    {
        if (iframeRef != null)
        {
            var success = await iframeRef.SendMessageAsync(new { action = "update", value = 42 });
            if (!success)
            {
                Console.WriteLine("Failed to send message");
            }
        }
    }
}
```

### SendTypedMessageAsync(string messageType, object? data, string? targetOrigin)
**Returns:** `Task<bool>`  
**Description:** Sends a typed message to the iframe with automatic timestamp.

```razor
<BlazorFrame @ref="iframeRef" Src="https://example.com" />

@code {
    private BlazorFrame? iframeRef;
    
    private async Task SendTypedData()
    {
        if (iframeRef != null)
        {
            await iframeRef.SendTypedMessageAsync("user-update", new { userId = 123, name = "John" });
        }
    }
}
```

### GetRecommendedCspHeader()
**Returns:** `CspHeader?`  
**Description:** Gets the recommended CSP header for the current configuration.

```razor
@code {
    private void GetCspHeader()
    {
        var cspHeader = iframeRef?.GetRecommendedCspHeader();
        if (cspHeader != null)
        {
            Console.WriteLine($"Header: {cspHeader.HeaderName}");
            Console.WriteLine($"Value: {cspHeader.HeaderValue}");
        }
    }
}
```

### ValidateCspConfiguration()
**Returns:** `CspValidationResult?`  
**Description:** Validates the current CSP configuration.

```razor
@code {
    private void ValidateCsp()
    {
        var result = iframeRef?.ValidateCspConfiguration();
        if (result != null)
        {
            foreach (var warning in result.Warnings)
            {
                Console.WriteLine($"Warning: {warning}");
            }
        }
    }
}
```

## Styling Parameters

### AdditionalAttributes
**Type:** `Dictionary<string, object>`  
**Default:** `new()`  
**Description:** Additional HTML attributes to apply to the iframe.

```razor
<!-- CSS classes and styles -->
<BlazorFrame Src="https://example.com"
            class="my-iframe-class"
            style="border: 1px solid #ccc;" />

<!-- Custom attributes -->
<BlazorFrame Src="https://example.com"
            title="External Widget"
            loading="lazy"
            referrerpolicy="strict-origin-when-cross-origin" />

<!-- Dynamic attributes -->
<BlazorFrame Src="https://example.com"
            @attributes="dynamicAttributes" />

@code {
    private readonly Dictionary<string, object> dynamicAttributes = new()
    {
        ["class"] = "border rounded shadow",
        ["style"] = "margin: 20px;",
        ["title"] = "Dynamic Content",
        ["data-testid"] = "main-iframe"
    };
}
```

**Note:** If you specify a `sandbox` attribute in `AdditionalAttributes`, it will override the computed sandbox value from `SecurityOptions`.

## Parameter Combinations

### Responsive Design
```razor
<div class="container-fluid">
    <BlazorFrame Src="@contentUrl"
                Width="100%"
                Height="@GetResponsiveHeight()"
                EnableAutoResize="@(!isMobile)"
                ResizeOptions="@(isMobile ? ResizeOptions.Performance : ResizeOptions.Default)"
                EnableScroll="@isMobile"
                SecurityOptions="@GetSecurityOptions()"
                class="@GetCssClasses()" />
</div>

@code {
    private bool isMobile = false;
    private string contentUrl = "https://example.com";
    
    private string GetResponsiveHeight() => 
        isMobile ? "300px" : "500px";
    
    private MessageSecurityOptions GetSecurityOptions() =>
        new MessageSecurityOptions()
            .ForProduction()
            .WithBasicSandbox();
    
    private string GetCssClasses() =>
        isMobile ? "mobile-iframe" : "desktop-iframe";
}
```

### High-Security Configuration
```razor
<BlazorFrame Src="@secureUrl"
            Width="100%"
            Height="400px"
            EnableAutoResize="false"
            AllowedOrigins="@trustedOrigins"
            SecurityOptions="@maxSecurityOptions"
            CspOptions="@strictCspOptions"
            OnValidatedMessage="LogSecureMessage"
            OnSecurityViolation="HandleSecurityBreach"
            OnCspHeaderGenerated="ApplyCspHeader"
            class="secure-iframe" />

@code {
    private readonly string secureUrl = "https://secure-widget.example.com";
    
    private readonly List<string> trustedOrigins = new()
    {
        "https://secure-widget.example.com"
    };
    
    private readonly MessageSecurityOptions maxSecurityOptions = 
        new MessageSecurityOptions()
            .ForPaymentWidget();  // Maximum security preset
    
    private readonly CspOptions strictCspOptions = new CspOptions()
        .ForProduction()
        .AllowFrameSources("https://secure-widget.example.com")
        .WithScriptNonce("secure-nonce-456");
}
```

### PDF Viewer with Refresh
```razor
<BlazorFrame @ref="pdfViewer"
            Src="@pdfUrl"
            Width="100%"
            Height="800px"
            EnableAutoResize="false"
            EnableScroll="true"
            OnLoad="HandlePdfLoad" />

<button @onclick="RefreshPdf">Refresh PDF</button>
<button @onclick="LoadNextPage">Next Document</button>

@code {
    private BlazorFrame? pdfViewer;
    private string pdfUrl = "https://example.com/document.pdf";
    private int currentPage = 1;
    
    private async Task RefreshPdf()
    {
        if (pdfViewer != null)
        {
            // Reload with cache-busting
            var cacheBustedUrl = $"{pdfUrl}?v={DateTime.UtcNow.Ticks}";
            await pdfViewer.ReloadAsync(cacheBustedUrl);
        }
    }
    
    private async Task LoadNextPage()
    {
        currentPage++;
        if (pdfViewer != null)
        {
            await pdfViewer.ReloadAsync($"https://example.com/document-{currentPage}.pdf");
        }
    }
    
    private Task HandlePdfLoad()
    {
        Console.WriteLine("PDF loaded successfully");
        return Task.CompletedTask;
    }
}
```

### Development-Friendly Configuration
```razor
<BlazorFrame Src="@devUrl"
            Width="100%"
            EnableAutoResize="true"
            ResizeOptions="@ResizeOptions.Responsive"
            SecurityOptions="@devSecurityOptions"
            OnValidatedMessage="LogDevMessage"
            OnSecurityViolation="LogDevViolation"
            OnInitializationError="LogDevError"
            class="dev-iframe border" />

@code {
    private readonly string devUrl = "http://localhost:3000";
    
    private readonly MessageSecurityOptions devSecurityOptions = 
        new MessageSecurityOptions()
            .ForDevelopment();  // Relaxed security for development
    
    private Task LogDevMessage(IframeMessage message)
    {
        Console.WriteLine($"[DEV] Message: {message.Data}");
        return Task.CompletedTask;
    }
    
    private Task LogDevViolation(IframeMessage violation)
    {
        Console.WriteLine($"[DEV] Violation: {violation.ValidationError}");
        return Task.CompletedTask;
    }
    
    private Task LogDevError(Exception ex)
    {
        Console.WriteLine($"[DEV] Error: {ex.Message}");
        return Task.CompletedTask;
    }
}
```

## Parameter Validation

BlazorFrame automatically validates parameter combinations and provides warnings for potential issues:

### Configuration Conflicts
```razor
@code {
    // This will generate a configuration warning
    private readonly MessageSecurityOptions conflictingOptions = new()
    {
        RequireHttps = true,          // Require HTTPS
        AllowInsecureConnections = true  // But also allow HTTP
    };
    // Warning: "RequireHttps is true but AllowInsecureConnections is also true..."
}
```

### Invalid Parameters
```razor
@code {
    // This will generate validation errors
    private readonly MessageSecurityOptions invalidOptions = new()
    {
        MaxMessageSize = -1,      // Error: Must be greater than 0
        MaxJsonDepth = 0,         // Error: Must be greater than 0
        MaxObjectProperties = -5   // Error: Must be greater than 0
    };
}
```

Monitor configuration issues using the `OnSecurityViolation` event:

```razor
<BlazorFrame OnSecurityViolation="HandleConfigurationIssue" />

@code {
    private Task HandleConfigurationIssue(IframeMessage violation)
    {
        if (violation.MessageType == "configuration-validation")
        {
            Logger.LogError("Configuration error: {Error}", violation.ValidationError);
        }
        return Task.CompletedTask;
    }
}
```

---