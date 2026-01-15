# Display Configuration

**Visual presentation and layout options for BlazorFrame**

This guide covers all aspects of configuring how BlazorFrame appears and behaves visually, including dimensions, responsive design, styling, and layout options.

## Basic Display Configuration

### Essential Display Properties

```razor
<BlazorFrame Src="https://example.com"
            Width="100%"           
            Height="400px"         
            EnableAutoResize="true"
            EnableScroll="false"   
            class="border rounded" />
```

## Dimensions and Sizing

### Fixed Dimensions

```razor
<!-- Fixed width and height -->
<BlazorFrame Src="@contentUrl"
            Width="800px"
            Height="600px"
            EnableAutoResize="false" />

<!-- CSS calc() expressions -->
<BlazorFrame Src="@contentUrl"
            Width="calc(100vw - 40px)"
            Height="calc(100vh - 200px)" />

<!-- Percentage-based sizing -->
<BlazorFrame Src="@contentUrl"
            Width="80%"
            Height="75vh" />
```

### Responsive Dimensions

```razor
@* Responsive iframe that adapts to container *@
<div class="iframe-container">
    <BlazorFrame Src="@contentUrl"
                Width="100%"
                Height="@GetResponsiveHeight()"
                EnableAutoResize="true" />
</div>

@code {
    private string GetResponsiveHeight()
    {
        // Adapt height based on screen size, content type, etc.
        return IsSmallScreen() ? "300px" : "500px";
    }
    
    private bool IsSmallScreen()
    {
        // Implement screen size detection logic
        return false; // Placeholder
    }
}
```

### Container-Based Sizing

```razor
<div class="container-fluid">
    <div class="row">
        <div class="col-md-8">
            <!-- Main content iframe -->
            <BlazorFrame Src="@mainContentUrl"
                        Width="100%"
                        Height="600px"
                        EnableAutoResize="true" />
        </div>
        <div class="col-md-4">
            <!-- Sidebar widget iframe -->
            <BlazorFrame Src="@sidebarWidgetUrl"
                        Width="100%"
                        Height="300px"
                        EnableAutoResize="false" />
        </div>
    </div>
</div>
```

## Auto-Resize Configuration

### Basic Auto-Resize

```razor
<BlazorFrame Src="@contentUrl"
            EnableAutoResize="true"
            ResizeOptions="@basicResizeOptions" />

@code {
    private readonly ResizeOptions basicResizeOptions = new()
    {
        MinHeight = 200,             // Minimum height in pixels
        MaxHeight = 1000,            // Maximum height in pixels
        DebounceMs = 100,            // Wait 100ms before resizing
        UseResizeObserver = true     // Use modern ResizeObserver API
    };
}
```

### ResizeOptions Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `MinHeight` | `int` | `100` | Minimum height in pixels |
| `MaxHeight` | `int` | `50000` | Maximum height in pixels |
| `PollingInterval` | `int` | `500` | Fallback polling interval (ms) when ResizeObserver is unavailable |
| `DebounceMs` | `int` | `100` | Debounce delay to prevent excessive updates (set to 0 to disable) |
| `UseResizeObserver` | `bool` | `true` | Use the modern ResizeObserver API when available |

### Built-in Presets

BlazorFrame provides built-in presets for common resize configurations:

```razor
@code {
    // Default balanced configuration
    private readonly ResizeOptions defaultOptions = ResizeOptions.Default;
    
    // Optimized for performance (less frequent updates)
    private readonly ResizeOptions performanceOptions = ResizeOptions.Performance;
    
    // Optimized for responsiveness (more frequent updates)
    private readonly ResizeOptions responsiveOptions = ResizeOptions.Responsive;
}
```

| Preset | PollingInterval | DebounceMs | Best For |
|--------|-----------------|------------|----------|
| `Default` | 500ms | 100ms | Most use cases |
| `Performance` | 1000ms | 250ms | Many iframes, mobile devices |
| `Responsive` | 250ms | 50ms | Dynamic content, smooth animations |

### Advanced Auto-Resize Configuration

```razor
<BlazorFrame Src="@contentUrl"
            EnableAutoResize="true"
            ResizeOptions="@advancedResizeOptions" />

@code {
    private readonly ResizeOptions advancedResizeOptions = new()
    {
        MinHeight = 150,            // Small min height for compact widgets
        MaxHeight = 1500,           // Large max height for rich content
        PollingInterval = 1000,     // Fallback polling every 1 second
        DebounceMs = 50,            // Fast response for dynamic content
        UseResizeObserver = true    // Prefer modern API
    };
}
```

### Conditional Auto-Resize

```razor
<BlazorFrame Src="@contentUrl"
            EnableAutoResize="@ShouldAutoResize()"
            Height="@GetInitialHeight()" />

@code {
    private bool ShouldAutoResize()
    {
        // Disable auto-resize for specific content types
        if (contentUrl.Contains("video") || contentUrl.Contains("game"))
            return false;
            
        // Disable on mobile for performance
        if (IsMobileDevice())
            return false;
            
        return true;
    }
    
    private string GetInitialHeight()
    {
        // Set appropriate fixed height when auto-resize is disabled
        return ShouldAutoResize() ? "auto" : "400px";
    }
}
```

## Programmatic Reload

Refresh iframe content without recreating the entire Blazor component. This is particularly useful for:
- Refreshing PDF documents
- Reloading dynamic content
- Cache-busting with updated URLs

### Basic Reload

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

### Reload with New URL

```razor
<BlazorFrame @ref="iframeRef" Src="@currentUrl" />

<button @onclick="LoadNewDocument">Load New Document</button>

@code {
    private BlazorFrame? iframeRef;
    private string currentUrl = "https://example.com/doc1.pdf";
    
    private async Task LoadNewDocument()
    {
        if (iframeRef != null)
        {
            await iframeRef.ReloadAsync("https://example.com/doc2.pdf");
        }
    }
}
```

### Cache-Busting Reload

```razor
<BlazorFrame @ref="iframeRef" Src="@pdfUrl" />

<button @onclick="RefreshWithCacheBust">Force Refresh</button>

@code {
    private BlazorFrame? iframeRef;
    private string pdfUrl = "https://example.com/report.pdf";
    
    private async Task RefreshWithCacheBust()
    {
        if (iframeRef != null)
        {
            // Add timestamp to bust cache
            var cacheBustedUrl = $"{pdfUrl}?v={DateTime.UtcNow.Ticks}";
            await iframeRef.ReloadAsync(cacheBustedUrl);
        }
    }
}
```

### Auto-Refresh on Interval

```razor
<BlazorFrame @ref="iframeRef" Src="@dashboardUrl" />

@code {
    private BlazorFrame? iframeRef;
    private string dashboardUrl = "https://example.com/dashboard";
    private Timer? refreshTimer;
    
    protected override void OnInitialized()
    {
        // Auto-refresh every 5 minutes
        refreshTimer = new Timer(async _ =>
        {
            if (iframeRef != null)
            {
                await InvokeAsync(async () =>
                {
                    await iframeRef.ReloadAsync();
                    StateHasChanged();
                });
            }
        }, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }
    
    public void Dispose()
    {
        refreshTimer?.Dispose();
    }
}
```

## Scrolling and Overflow

### Scroll Configuration

```razor
<!-- Enable scrolling for content that might overflow -->
<BlazorFrame Src="@longContentUrl"
            Width="100%"
            Height="400px"
            EnableAutoResize="false"
            EnableScroll="true" />

<!-- Disable scrolling for content that should fit exactly -->
<BlazorFrame Src="@fixedContentUrl"
            Width="800px"
            Height="600px"
            EnableAutoResize="false"
            EnableScroll="false" />
```

### Scroll Behavior Customization

```razor
<BlazorFrame Src="@contentUrl"
            EnableScroll="true"
            ScrollOptions="@scrollOptions" />

@code {
    private readonly ScrollOptions scrollOptions = new()
    {
        ScrollbarWidth = "thin",        // Thin scrollbars
        ScrollBehavior = "smooth",      // Smooth scrolling
        OverflowX = "hidden",          // Hide horizontal scrollbar
        OverflowY = "auto",            // Show vertical scrollbar when needed
        ScrollIndicator = true          // Show scroll indicator
    };
}
```

## Styling and Appearance

### CSS Classes and Styling

```razor
<!-- Basic styling with CSS classes -->
<BlazorFrame Src="@contentUrl"
            class="border border-primary rounded shadow-sm"
            style="margin: 20px; transition: all 0.3s ease;" />

<!-- Custom CSS classes for different iframe types -->
<BlazorFrame Src="@widgetUrl"
            class="widget-iframe @GetWidgetClass()"
            Width="100%"
            Height="300px" />

@code {
    private string GetWidgetClass()
    {
        return widgetUrl.Contains("payment") ? "payment-widget" : "standard-widget";
    }
}
```

### CSS Custom Properties

```css
/* Custom CSS for BlazorFrame styling */
.widget-iframe {
    --iframe-border-color: #e0e0e0;
    --iframe-border-radius: 8px;
    --iframe-shadow: 0 2px 4px rgba(0,0,0,0.1);
    
    border: 1px solid var(--iframe-border-color);
    border-radius: var(--iframe-border-radius);
    box-shadow: var(--iframe-shadow);
    transition: box-shadow 0.3s ease;
}

.widget-iframe:hover {
    --iframe-shadow: 0 4px 8px rgba(0,0,0,0.15);
}

.payment-widget {
    --iframe-border-color: #28a745;
    border-width: 2px;
}
```

### Responsive Design Classes

```razor
<BlazorFrame Src="@contentUrl"
            class="responsive-iframe d-block"
            Width="100%"
            Height="@GetResponsiveHeight()" />

<style>
.responsive-iframe {
    max-width: 100%;
    height: auto;
}

@media (max-width: 768px) {
    .responsive-iframe {
        height: 300px !important;
        margin: 10px 0;
    }
}

@media (min-width: 1200px) {
    .responsive-iframe {
        max-width: 1140px;
        margin: 0 auto;
    }
}
</style>
```

## Loading States and Placeholders

### Loading Indicators

```razor
<div class="iframe-container">
    @if (isLoading)
    {
        <div class="iframe-placeholder">
            <div class="spinner-border" role="status">
                <span class="sr-only">Loading...</span>
            </div>
            <p>Loading iframe content...</p>
        </div>
    }
    
    <BlazorFrame Src="@contentUrl"
                Width="100%"
                Height="400px"
                OnLoad="HandleIframeLoad"
                style="@(isLoading ? "display: none;" : "")" />
</div>

@code {
    private bool isLoading = true;
    
    private async Task HandleIframeLoad()
    {
        isLoading = false;
        StateHasChanged();
    }
}
```

### Custom Placeholder Content

```razor
<div class="iframe-wrapper">
    @if (!contentLoaded)
    {
        <div class="iframe-placeholder" style="width: 100%; height: 400px;">
            <div class="placeholder-content">
                <img src="/images/iframe-placeholder.svg" alt="Loading" />
                <h5>Loading Widget</h5>
                <p>Please wait while we load the content...</p>
            </div>
        </div>
    }
    
    <BlazorFrame Src="@contentUrl"
                OnLoad="@(() => contentLoaded = true)" />
</div>

@code {
    private bool contentLoaded = false;
}
```

## Theme Integration

### Bootstrap Integration

```razor
<!-- Bootstrap-styled iframe cards -->
<div class="card">
    <div class="card-header">
        <h5 class="card-title">External Widget</h5>
    </div>
    <div class="card-body p-0">
        <BlazorFrame Src="@widgetUrl"
                    Width="100%"
                    Height="300px"
                    class="rounded-bottom" />
    </div>
</div>

<!-- Bootstrap responsive utilities -->
<BlazorFrame Src="@contentUrl"
            class="d-none d-md-block w-100"
            Width="100%"
            Height="500px" />
            
<BlazorFrame Src="@mobileContentUrl"
            class="d-block d-md-none w-100"
            Width="100%"
            Height="300px" />
```

### Dark Mode Support

```razor
<BlazorFrame Src="@GetThemedUrl()"
            class="@GetThemeClass()"
            Width="100%"
            Height="400px" />

@code {
    private string GetThemedUrl()
    {
        var baseUrl = "https://widget.example.com";
        var theme = CurrentTheme == "dark" ? "dark" : "light";
        return $"{baseUrl}?theme={theme}";
    }
    
    private string GetThemeClass()
    {
        return CurrentTheme == "dark" ? "iframe-dark-theme" : "iframe-light-theme";
    }
}

<style>
.iframe-dark-theme {
    border: 1px solid #444;
    background-color: #2d2d2d;
}

.iframe-light-theme {
    border: 1px solid #ddd;
    background-color: #fff;
}
</style>
```

## Layout Patterns

### Full-Screen Layout

```razor
<div class="fullscreen-iframe-container">
    <BlazorFrame Src="@fullscreenContentUrl"
                Width="100vw"
                Height="100vh"
                EnableAutoResize="false"
                EnableScroll="true"
                class="fullscreen-iframe" />
</div>

<style>
.fullscreen-iframe-container {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    z-index: 9999;
    background: rgba(0,0,0,0.9);
}

.fullscreen-iframe {
    border: none;
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    max-width: 95vw;
    max-height: 95vh;
}
</style>
```

### Modal Dialog Layout

```razor
<!-- Bootstrap Modal with iframe -->
<div class="modal fade" id="iframeModal" tabindex="-1">
    <div class="modal-dialog modal-xl">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">External Content</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body p-0">
                <BlazorFrame Src="@modalContentUrl"
                            Width="100%"
                            Height="600px"
                            EnableAutoResize="true" />
            </div>
        </div>
    </div>
</div>
```

### Grid Layout

```razor
<div class="iframe-grid">
    @foreach (var widget in widgets)
    {
        <div class="iframe-grid-item">
            <BlazorFrame Src="@widget.Url"
                        Width="100%"
                        Height="@widget.Height"
                        EnableAutoResize="@widget.AutoResize"
                        class="grid-iframe" />
        </div>
    }
</div>

<style>
.iframe-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: 20px;
    padding: 20px;
}

.iframe-grid-item {
    display: flex;
    flex-direction: column;
}

.grid-iframe {
    border: 1px solid #ddd;
    border-radius: 8px;
    min-height: 200px;
}
</style>
```

## Performance Optimization

### Lazy Loading

```razor
<div class="iframe-container" @ref="containerRef">
    @if (shouldLoad)
    {
        <BlazorFrame Src="@contentUrl"
                    Width="100%"
                    Height="400px"
                    OnLoad="HandleLoad" />
    }
    else
    {
        <div class="iframe-placeholder" style="height: 400px;">
            <p>Content will load when scrolled into view</p>
        </div>
    }
</div>

@code {
    private ElementReference containerRef;
    private bool shouldLoad = false;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await SetupIntersectionObserver();
        }
    }
    
    private async Task SetupIntersectionObserver()
    {
        // Setup intersection observer to load iframe when visible
        await JSRuntime.InvokeVoidAsync("setupLazyLoading", containerRef, 
            DotNetObjectReference.Create(this));
    }
    
    [JSInvokable]
    public async Task LoadIframe()
    {
        shouldLoad = true;
        StateHasChanged();
    }
}
```

### Resource Optimization

```razor
<BlazorFrame Src="@contentUrl"
            Width="100%"
            Height="400px"
            ResizeOptions="@ResizeOptions.Performance" />
```

## Accessibility

### Accessibility Configuration

```razor
<BlazorFrame Src="@contentUrl"
            Width="100%"
            Height="400px"
            title="External widget content"
            aria-label="Interactive widget"
            role="application"
            tabindex="0" />
```

### Screen Reader Support

```razor
<div class="iframe-wrapper">
    <div class="sr-only">
        <p>Beginning of external content widget</p>
    </div>
    
    <BlazorFrame Src="@contentUrl"
                Width="100%"
                Height="400px"
                title="@GetAccessibleTitle()"
                OnLoad="AnnounceLoad" />
                
    <div class="sr-only">
        <p>End of external content widget</p>
    </div>
</div>

@code {
    private string GetAccessibleTitle()
    {
        return $"External widget from {new Uri(contentUrl).Host}";
    }
    
    private async Task AnnounceLoad()
    {
        await JSRuntime.InvokeVoidAsync("announceToScreenReader", 
            "External widget has finished loading");
    }
}
```

## Display Best Practices

### Do
- **Use responsive dimensions** - Make iframes work on all screen sizes
- **Enable auto-resize** for dynamic content that changes height
- **Configure ResizeOptions** appropriately for your use case
- **Use built-in presets** (`ResizeOptions.Performance`, `ResizeOptions.Responsive`) when applicable
- **Use `ReloadAsync()`** instead of recreating components for content refresh
- **Provide loading indicators** - Show users that content is loading
- **Set appropriate min/max heights** - Prevent layout issues
- **Use semantic HTML** - Include proper titles and ARIA labels
- **Test on mobile devices** - Ensure good mobile experience
- **Consider performance** - Use lazy loading for off-screen content

### Don't
- **Use fixed pixel dimensions** unless necessary
- **Enable scrolling** unless content might overflow
- **Forget about loading states** - Always handle loading gracefully
- **Ignore accessibility** - Include proper labels and structure
- **Make iframes too small** - Ensure content is readable
- **Forget about responsive design** - Test on different screen sizes
- **Overuse auto-resize** - Can cause performance issues with many iframes
- **Recreate components** just to refresh content - use `ReloadAsync()` instead

---
