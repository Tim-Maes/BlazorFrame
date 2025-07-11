# Advanced Configuration

**Performance tuning, debugging, and custom integration options for BlazorFrame**

This guide covers advanced configuration scenarios including performance optimization, debugging features, custom integrations, and enterprise-level deployment configurations.

## Performance Configuration

### Resize Performance Optimization

```csharp
var optimizedResizeOptions = new ResizeOptions
{
    // Use modern ResizeObserver API when available
    UseResizeObserver = true,
    
    // Fallback polling interval for older browsers
    FallbackPollingInterval = 2000,  // 2 seconds instead of default 1s
    
    // Debounce rapid resize events
    DebounceDelayMs = 200,          // Wait 200ms after last resize
    
    // Throttle resize events during animation
    ResizeThrottleMs = 33,          // ~30fps throttling
    
    // Smooth resize transitions
    SmoothResize = true,
    TransitionDuration = "0.3s",
    
    // Reasonable size limits
    MaxHeight = 2000,               // Prevent excessive heights
    MinHeight = 100,                // Ensure minimum usability
    
    // Performance monitoring
    EnablePerformanceLogging = true,
    LogResizeEvents = false         // Disable verbose logging in production
};
```

### Message Processing Optimization

```csharp
var performanceSecurityOptions = new MessageSecurityOptions
{
    // Optimize validation settings
    EnableStrictValidation = true,
    MaxMessageSize = 32 * 1024,     // 32KB - balance security vs performance
    MaxJsonDepth = 8,               // Reasonable depth limit
    MaxObjectProperties = 50,        // Prevent excessive object complexity
    MaxArrayElements = 500,         // Reasonable array size
    
    // Rate limiting for performance
    EnableRateLimiting = true,
    MaxMessagesPerSecond = 20,      // Allow reasonable message frequency
    RateLimitWindow = TimeSpan.FromSeconds(1),
    
    // Optimized content filtering
    FilterMaliciousContent = true,
    UseOptimizedPatterns = true,    // Use compiled regex patterns
    CacheValidationResults = true,  // Cache validation outcomes
    
    // Async processing
    EnableAsyncValidation = true,   // Don't block UI thread
    ValidationTimeout = TimeSpan.FromMilliseconds(500)
};
```

### Memory Management

```csharp
public class IframeResourceManager
{
    private readonly Dictionary<string, WeakReference<BlazorFrame>> activeIframes = new();
    private readonly Timer cleanupTimer;
    
    public IframeResourceManager()
    {
        // Cleanup unused references every 5 minutes
        cleanupTimer = new Timer(CleanupUnusedReferences, null, 
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }
    
    public void RegisterIframe(string id, BlazorFrame iframe)
    {
        activeIframes[id] = new WeakReference<BlazorFrame>(iframe);
    }
    
    private void CleanupUnusedReferences(object? state)
    {
        var keysToRemove = new List<string>();
        
        foreach (var kvp in activeIframes)
        {
            if (!kvp.Value.TryGetTarget(out _))
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            activeIframes.Remove(key);
        }
        
        // Force garbage collection if many references were cleaned
        if (keysToRemove.Count > 10)
        {
            GC.Collect();
        }
    }
}
```

### Lazy Loading Implementation

```razor
<div class="iframe-container" @ref="containerElement">
    @if (shouldLoadIframe)
    {
        <BlazorFrame @ref="iframeRef"
                    Src="@iframeUrl"
                    ResizeOptions="@lazyLoadResizeOptions"
                    OnLoad="HandleIframeLoad" />
    }
    else
    {
        <div class="iframe-placeholder" style="height: @estimatedHeight">
            <div class="placeholder-content">
                <div class="spinner-border" role="status"></div>
                <p>Content will load when visible</p>
            </div>
        </div>
    }
</div>

@code {
    private ElementReference containerElement;
    private BlazorFrame? iframeRef;
    private bool shouldLoadIframe = false;
    private string estimatedHeight = "400px";
    
    private readonly ResizeOptions lazyLoadResizeOptions = new()
    {
        UseResizeObserver = true,
        DebounceDelayMs = 100,
        EnablePerformanceLogging = false  // Disable for lazy-loaded content
    };
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await SetupIntersectionObserver();
        }
    }
    
    private async Task SetupIntersectionObserver()
    {
        var options = new
        {
            root = (object?)null,
            rootMargin = "100px",  // Start loading 100px before visible
            threshold = 0.1
        };
        
        await JSRuntime.InvokeVoidAsync("observeElement", 
            containerElement, 
            DotNetObjectReference.Create(this),
            options);
    }
    
    [JSInvokable]
    public async Task OnElementVisible()
    {
        shouldLoadIframe = true;
        StateHasChanged();
        
        // Preload resources
        await PreloadResources();
    }
    
    private async Task PreloadResources()
    {
        // Preload critical resources before iframe loads
        var preloadUrls = GetPreloadUrls();
        await JSRuntime.InvokeVoidAsync("preloadResources", preloadUrls);
    }
}
```

## Debugging and Diagnostics

### Comprehensive Debugging Configuration
```csharp
var debugOptions = new MessageSecurityOptions
{
    // Enable detailed logging
    LogSecurityViolations = true,
    LogValidationDetails = true,
    LogPerformanceMetrics = true,
    
    // Debug-specific settings
    EnableDebugMode = Environment.IsDevelopment(),
    VerboseLogging = Environment.IsDevelopment(),
    
    // Validation timing
    EnableValidationTiming = true,
    LogSlowValidations = true,
    SlowValidationThreshold = TimeSpan.FromMilliseconds(50),
    
    // Memory diagnostics
    TrackMemoryUsage = true,
    EnableMemoryProfiling = Environment.IsDevelopment()
};
```

### Debug Dashboard Component
```razor
@page "/iframe-debug"
@using System.Text.Json

<div class="debug-dashboard">
    <h2>BlazorFrame Debug Dashboard</h2>
    
    <div class="row">
        <div class="col-md-6">
            <div class="card">
                <div class="card-header">
                    <h5>Active Iframes</h5>
                </div>
                <div class="card-body">
                    @foreach (var iframe in activeIframes)
                    {
                        <div class="iframe-debug-item">
                            <strong>@iframe.Id</strong><br>
                            <small>Source: @iframe.Src</small><br>
                            <small>Status: @iframe.Status</small><br>
                            <small>Messages: @iframe.MessageCount</small>
                        </div>
                    }
                </div>
            </div>
        </div>
        
        <div class="col-md-6">
            <div class="card">
                <div class="card-header">
                    <h5>Performance Metrics</h5>
                </div>
                <div class="card-body">
                    <table class="table table-sm">
                        <tr>
                            <td>Total Iframes</td>
                            <td>@performanceMetrics.TotalIframes</td>
                        </tr>
                        <tr>
                            <td>Total Messages</td>
                            <td>@performanceMetrics.TotalMessages</td>
                        </tr>
                        <tr>
                            <td>Avg Validation Time</td>
                            <td>@performanceMetrics.AverageValidationTime.TotalMilliseconds ms</td>
                        </tr>
                        <tr>
                            <td>Security Violations</td>
                            <td>@performanceMetrics.SecurityViolations</td>
                        </tr>
                    </table>
                </div>
            </div>
        </div>
    </div>
    
    <div class="row mt-3">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h5>Recent Events</h5>
                </div>
                <div class="card-body">
                    <div class="debug-log" style="height: 300px; overflow-y: auto;">
                        @foreach (var logEntry in recentLogs.OrderByDescending(l => l.Timestamp))
                        {
                            <div class="log-entry @GetLogEntryClass(logEntry.Level)">
                                <small>@logEntry.Timestamp.ToString("HH:mm:ss.fff")</small>
                                <strong>[@logEntry.Level]</strong>
                                @logEntry.Message
                            </div>
                        }
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    private List<IframeDebugInfo> activeIframes = new();
    private PerformanceMetrics performanceMetrics = new();
    private List<LogEntry> recentLogs = new();
    private Timer? refreshTimer;
    
    protected override void OnInitialized()
    {
        // Refresh dashboard every 2 seconds
        refreshTimer = new Timer(async _ => await RefreshData(), null, 
            TimeSpan.Zero, TimeSpan.FromSeconds(2));
    }
    
    private async Task RefreshData()
    {
        // Collect current iframe data
        activeIframes = await IframeDebugService.GetActiveIframes();
        performanceMetrics = await PerformanceService.GetMetrics();
        recentLogs = await LogService.GetRecentLogs(50);
        
        await InvokeAsync(StateHasChanged);
    }
    
    private string GetLogEntryClass(LogLevel level)
    {
        return level switch
        {
            LogLevel.Error => "text-danger",
            LogLevel.Warning => "text-warning",
            LogLevel.Information => "text-info",
            LogLevel.Debug => "text-muted",
            _ => ""
        };
    }
}
```

### Performance Profiling

```csharp
public class IframePerformanceProfiler
{
    private readonly Dictionary<string, PerformanceCounter> counters = new();
    private readonly ILogger<IframePerformanceProfiler> logger;
    
    public IframePerformanceProfiler(ILogger<IframePerformanceProfiler> logger)
    {
        this.logger = logger;
    }
    
    public IDisposable StartOperation(string operationName)
    {
        return new PerformanceOperation(operationName, this);
    }
    
    public void RecordOperation(string operationName, TimeSpan duration)
    {
        if (!counters.TryGetValue(operationName, out var counter))
        {
            counter = new PerformanceCounter(operationName);
            counters[operationName] = counter;
        }
        
        counter.RecordOperation(duration);
        
        // Log slow operations
        if (duration.TotalMilliseconds > 100)
        {
            logger.LogWarning("Slow operation {Operation}: {Duration}ms", 
                operationName, duration.TotalMilliseconds);
        }
    }
    
    public PerformanceReport GenerateReport()
    {
        return new PerformanceReport
        {
            Counters = counters.Values.ToList(),
            GeneratedAt = DateTime.UtcNow
        };
    }
    
    private class PerformanceOperation : IDisposable
    {
        private readonly string operationName;
        private readonly IframePerformanceProfiler profiler;
        private readonly Stopwatch stopwatch;
        
        public PerformanceOperation(string operationName, IframePerformanceProfiler profiler)
        {
            this.operationName = operationName;
            this.profiler = profiler;
            this.stopwatch = Stopwatch.StartNew();
        }
        
        public void Dispose()
        {
            stopwatch.Stop();
            profiler.RecordOperation(operationName, stopwatch.Elapsed);
        }
    }
}

// Usage
using (profiler.StartOperation("MessageValidation"))
{
    var result = await ValidateMessage(message);
}
```

## Custom Integration Patterns

### Plugin Architecture
```csharp
public interface IBlazorFramePlugin
{
    string Name { get; }
    Version Version { get; }
    
    Task InitializeAsync(BlazorFrame iframe);
    Task OnMessageAsync(IframeMessage message);
    Task OnLoadAsync();
    Task OnUnloadAsync();
}

public class BlazorFramePluginManager
{
    private readonly List<IBlazorFramePlugin> plugins = new();
    private readonly IServiceProvider serviceProvider;
    
    public BlazorFramePluginManager(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }
    
    public void RegisterPlugin<T>() where T : class, IBlazorFramePlugin
    {
        var plugin = ActivatorUtilities.CreateInstance<T>(serviceProvider);
        plugins.Add(plugin);
    }
    
    public async Task InitializePlugins(BlazorFrame iframe)
    {
        foreach (var plugin in plugins)
        {
            try
            {
                await plugin.InitializeAsync(iframe);
            }
            catch (Exception ex)
            {
                // Log plugin initialization failure but continue
                var logger = serviceProvider.GetService<ILogger<BlazorFramePluginManager>>();
                logger?.LogError(ex, "Failed to initialize plugin {Plugin}", plugin.Name);
            }
        }
    }
    
    public async Task NotifyPlugins(Func<IBlazorFramePlugin, Task> action)
    {
        var tasks = plugins.Select(async plugin =>
        {
            try
            {
                await action(plugin);
            }
            catch (Exception ex)
            {
                // Log but don't fail other plugins
                var logger = serviceProvider.GetService<ILogger<BlazorFramePluginManager>>();
                logger?.LogError(ex, "Plugin {Plugin} failed", plugin.Name);
            }
        });
        
        await Task.WhenAll(tasks);
    }
}

// Example plugin
public class AnalyticsPlugin : IBlazorFramePlugin
{
    public string Name => "Analytics Plugin";
    public Version Version => new Version(1, 0, 0);
    
    private readonly IAnalyticsService analyticsService;
    
    public AnalyticsPlugin(IAnalyticsService analyticsService)
    {
        this.analyticsService = analyticsService;
    }
    
    public Task InitializeAsync(BlazorFrame iframe)
    {
        return analyticsService.TrackEvent("IframeInitialized", new
        {
            Source = iframe.Src,
            Timestamp = DateTime.UtcNow
        });
    }
    
    public Task OnMessageAsync(IframeMessage message)
    {
        return analyticsService.TrackEvent("IframeMessageReceived", new
        {
            Origin = message.Origin,
            MessageType = message.MessageType,
            Timestamp = DateTime.UtcNow
        });
    }
    
    public Task OnLoadAsync()
    {
        return analyticsService.TrackEvent("IframeLoaded");
    }
    
    public Task OnUnloadAsync()
    {
        return analyticsService.TrackEvent("IframeUnloaded");
    }
}
```

### Middleware Pattern

```csharp
public interface IIframeMiddleware
{
    Task<bool> ProcessAsync(IframeMessage message, Func<IframeMessage, Task> next);
}

public class IframeMiddlewarePipeline
{
    private readonly List<IIframeMiddleware> middlewares = new();
    
    public void Use(IIframeMiddleware middleware)
    {
        middlewares.Add(middleware);
    }
    
    public async Task ProcessAsync(IframeMessage message, Func<IframeMessage, Task> finalHandler)
    {
        await ExecuteMiddleware(0, message, finalHandler);
    }
    
    private async Task ExecuteMiddleware(int index, IframeMessage message, Func<IframeMessage, Task> finalHandler)
    {
        if (index >= middlewares.Count)
        {
            await finalHandler(message);
            return;
        }
        
        var middleware = middlewares[index];
        var shouldContinue = await middleware.ProcessAsync(message, async msg =>
        {
            await ExecuteMiddleware(index + 1, msg, finalHandler);
        });
    }
}

// Example middleware
public class AuthenticationMiddleware : IIframeMiddleware
{
    private readonly IAuthenticationService authService;
    
    public AuthenticationMiddleware(IAuthenticationService authService)
    {
        this.authService = authService;
    }
    
    public async Task<bool> ProcessAsync(IframeMessage message, Func<IframeMessage, Task> next)
    {
        // Check if message requires authentication
        if (RequiresAuthentication(message))
        {
            var isAuthenticated = await authService.ValidateTokenAsync(message.AuthToken);
            
            if (!isAuthenticated)
            {
                // Block message processing
                return false;
            }
        }
        
        // Continue to next middleware
        await next(message);
        return true;
    }
    
    private bool RequiresAuthentication(IframeMessage message)
    {
        return message.MessageType?.StartsWith("auth:") == true;
    }
}
```

## Enterprise Configuration

### Multi-Tenant Configuration
```csharp
public class TenantIframeConfigurationProvider
{
    private readonly ITenantService tenantService;
    private readonly IConfiguration configuration;
    
    public TenantIframeConfigurationProvider(ITenantService tenantService, IConfiguration configuration)
    {
        this.tenantService = tenantService;
        this.configuration = configuration;
    }
    
    public async Task<MessageSecurityOptions> GetSecurityOptionsForTenant(string tenantId)
    {
        var tenant = await tenantService.GetTenantAsync(tenantId);
        
        var baseOptions = new MessageSecurityOptions();
        
        // Apply tenant-specific security level
        switch (tenant.SecurityTier)
        {
            case SecurityTier.Basic:
                baseOptions.ForProduction();
                break;
                
            case SecurityTier.Enhanced:
                baseOptions.ForProduction()
                    .WithStrictSandbox()
                    .RequireHttps();
                break;
                
            case SecurityTier.Maximum:
                baseOptions.ForPaymentWidget()
                    .WithParanoidSandbox();
                break;
        }
        
        // Apply tenant-specific allowed origins
        baseOptions.AllowedOrigins = tenant.AllowedIframeDomains?.ToList() ?? new List<string>();
        
        // Apply tenant-specific message limits
        baseOptions.MaxMessageSize = tenant.MaxMessageSize ?? 64 * 1024;
        baseOptions.MaxMessagesPerSecond = tenant.RateLimit ?? 10;
        
        return baseOptions;
    }
    
    public async Task<CspOptions> GetCspOptionsForTenant(string tenantId)
    {
        var tenant = await tenantService.GetTenantAsync(tenantId);
        
        var cspOptions = new CspOptions();
        
        if (tenant.SecurityTier == SecurityTier.Maximum)
        {
            cspOptions.ForProduction().UseStrictDynamic();
        }
        else
        {
            cspOptions.ForProduction();
        }
        
        // Add tenant-specific frame sources
        foreach (var domain in tenant.AllowedIframeDomains ?? Array.Empty<string>())
        {
            cspOptions.AllowFrameSource(domain);
        }
        
        return cspOptions;
    }
}
```

### Configuration Validation Pipeline

```csharp
public class ConfigurationValidationPipeline
{
    private readonly List<IConfigurationValidator> validators;
    
    public ConfigurationValidationPipeline()
    {
        validators = new List<IConfigurationValidator>
        {
            new SecurityConfigurationValidator(),
            new PerformanceConfigurationValidator(),
            new ComplianceConfigurationValidator(),
            new BusinessRuleValidator()
        };
    }
    
    public async Task<ValidationResult> ValidateAsync(IframeConfiguration configuration)
    {
        var results = new List<ValidationResult>();
        
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(configuration);
            results.Add(result);
        }
        
        return CombineResults(results);
    }
    
    private ValidationResult CombineResults(List<ValidationResult> results)
    {
        var combinedErrors = results.SelectMany(r => r.Errors).ToList();
        var combinedWarnings = results.SelectMany(r => r.Warnings).ToList();
        var combinedSuggestions = results.SelectMany(r => r.Suggestions).ToList();
        
        return new ValidationResult
        {
            IsValid = combinedErrors.Count == 0,
            Errors = combinedErrors,
            Warnings = combinedWarnings,
            Suggestions = combinedSuggestions
        };
    }
}

public class ComplianceConfigurationValidator : IConfigurationValidator
{
    public async Task<ValidationResult> ValidateAsync(IframeConfiguration configuration)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var suggestions = new List<string>();
        
        // GDPR compliance checks
        if (IsEuTenant(configuration.TenantId))
        {
            if (!configuration.SecurityOptions.RequireHttps)
            {
                errors.Add("HTTPS is required for EU tenants (GDPR compliance)");
            }
            
            if (configuration.SecurityOptions.LogSecurityViolations)
            {
                warnings.Add("Security violation logging may need GDPR consent");
            }
        }
        
        // PCI DSS compliance checks
        if (IsPaymentTenant(configuration.TenantId))
        {
            if (configuration.SecurityOptions.SandboxPreset != SandboxPreset.Paranoid)
            {
                errors.Add("Paranoid sandbox required for PCI DSS compliance");
            }
            
            if (configuration.SecurityOptions.MaxMessageSize > 16 * 1024)
            {
                warnings.Add("Consider smaller message size limits for payment processing");
            }
        }
        
        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings,
            Suggestions = suggestions
        };
    }
    
    private bool IsEuTenant(string tenantId) => /* implementation */;
    private bool IsPaymentTenant(string tenantId) => /* implementation */;
}
```

### Environment Configuration Management

```csharp
public class EnvironmentConfigurationManager
{
    private readonly IConfiguration configuration;
    private readonly IHostEnvironment environment;
    
    public EnvironmentConfigurationManager(IConfiguration configuration, IHostEnvironment environment)
    {
        this.configuration = configuration;
        this.environment = environment;
    }
    
    public MessageSecurityOptions GetSecurityOptions()
    {
        var options = new MessageSecurityOptions();
        
        if (environment.IsDevelopment())
        {
            return options
                .ForDevelopment()
                .WithPermissiveSandbox()
                .Validate(); // Warn but don't throw
        }
        
        if (environment.IsStaging())
        {
            return options
                .ForProduction()
                .WithBasicSandbox()
                .ValidateAndThrow(); // Fail fast in staging
        }
        
        if (environment.IsProduction())
        {
            var securityLevel = configuration.GetValue<string>("BlazorFrame:SecurityLevel");
            
            return securityLevel?.ToLowerInvariant() switch
            {
                "high" => options.ForPaymentWidget().ValidateAndThrow(),
                "medium" => options.ForProduction().WithStrictSandbox().ValidateAndThrow(),
                _ => options.ForProduction().ValidateAndThrow()
            };
        }
        
        // Default fallback
        return options.ForProduction().ValidateAndThrow();
    }
    
    public CspOptions GetCspOptions()
    {
        var options = new CspOptions();
        
        if (environment.IsDevelopment())
        {
            return options.ForDevelopment();
        }
        
        var cspLevel = configuration.GetValue<string>("BlazorFrame:CspLevel", "standard");
        
        return cspLevel.ToLowerInvariant() switch
        {
            "strict" => options.ForProduction().UseStrictDynamic(),
            "balanced" => options.ForProduction(),
            "permissive" => options.ForDevelopment(),
            _ => options.ForProduction()
        };
    }
}
```

## Advanced Best Practices

### Do
- **Monitor performance metrics** in production environments
- **Implement comprehensive logging** for debugging and monitoring
- **Use lazy loading** for iframes that aren't immediately visible
- **Profile memory usage** to prevent memory leaks
- **Validate configurations** in CI/CD pipelines
- **Implement plugin architecture** for extensibility
- **Use middleware patterns** for cross-cutting concerns
- **Plan for multi-tenant scenarios** from the beginning

### Don't
- **Enable verbose logging** in production environments
- **Ignore performance metrics** - monitor and act on them
- **Implement complex logic** in iframe message handlers
- **Forget about cleanup** - dispose resources properly
- **Skip configuration validation** in automated tests
- **Hard-code tenant-specific settings** - use configuration providers
- **Block the UI thread** with heavy processing
- **Neglect error handling** in advanced scenarios

## Advanced Scenarios Reference

### Configuration Patterns
| Pattern | Use Case | Benefits |
|---------|----------|----------|
| **Plugin Architecture** | Extensible functionality | Modularity, maintainability |
| **Middleware Pipeline** | Cross-cutting concerns | Separation of concerns, reusability |
| **Multi-Tenant Provider** | SaaS applications | Tenant isolation, customization |
| **Environment Manager** | Multiple environments | Environment-specific configurations |

### Performance Considerations
| Area | Optimization | Impact |
|------|-------------|---------|
| **Resize Events** | Debounce and throttle | Reduces CPU usage |
| **Message Validation** | Cache validation results | Improves response time |
| **Memory Management** | Weak references and cleanup | Prevents memory leaks |
| **Lazy Loading** | Intersection Observer | Reduces initial load time |

---
