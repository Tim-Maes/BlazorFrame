# Common Scenarios

Real-world examples and patterns for using BlazorFrame in common scenarios. Each example includes complete, copy-paste-ready code with best practices and security considerations.

## E-commerce Integration

### Product Widget with Add to Cart

```razor
@page "/products"
@using BlazorFrame

<div class="container">
    <h2>Featured Products</h2>
    
    <div class="row">
        <div class="col-md-8">
            <!-- Product iframe with form submission support -->
            <BlazorFrame Src="https://shop.example.com/widget/products"
                        Width="100%"
                        Height="500px"
                        EnableAutoResize="true"
                        SecurityOptions="@ecommerceSecurityOptions"
                        OnValidatedMessage="HandleProductMessage"
                        OnSecurityViolation="HandleEcommerceViolation"
                        class="product-widget border rounded" />
        </div>
        
        <div class="col-md-4">
            <div class="cart-summary">
                <h4>Cart Summary</h4>
                <p>Items: @cartItems</p>
                <p>Total: @cartTotal.ToString("C")</p>
            </div>
        </div>
    </div>
</div>

@code {
    private int cartItems = 0;
    private decimal cartTotal = 0;
    
    // E-commerce widgets need form submission for "Add to Cart"
    private readonly MessageSecurityOptions ecommerceSecurityOptions = new MessageSecurityOptions()
        .ForTrustedContent()          // Balanced security for trusted shop
        .WithPermissiveSandbox()      // Allow forms for cart interactions
        .RequireHttps();              // Secure transport for shopping

    private async Task HandleProductMessage(IframeMessage message)
    {
        try
        {
            var productData = JsonSerializer.Deserialize<ProductMessage>(message.Data);
            
            switch (productData?.Type)
            {
                case "add-to-cart":
                    await AddToCart(productData.ProductId, productData.Quantity);
                    break;
                    
                case "view-product":
                    await TrackProductView(productData.ProductId);
                    break;
                    
                case "price-check":
                    await UpdatePricing(productData.ProductId, productData.Price);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing product message");
        }
    }

    private async Task HandleEcommerceViolation(IframeMessage violation)
    {
        // Log security violations for e-commerce monitoring
        Logger.LogWarning("E-commerce security violation: {Error} from {Origin}", 
            violation.ValidationError, violation.Origin);
            
        // Could implement additional security measures
        await NotifySecurityTeam(violation);
    }

    private async Task AddToCart(string productId, int quantity)
    {
        // Update cart state
        cartItems += quantity;
        cartTotal += await GetProductPrice(productId) * quantity;
        StateHasChanged();
        
        // Sync with backend
        await CartService.AddItemAsync(productId, quantity);
    }
    
    public class ProductMessage
    {
        public string Type { get; set; } = "";
        public string ProductId { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}

<style>
.product-widget {
    min-height: 400px;
    background: #f8f9fa;
}

.cart-summary {
    background: #fff;
    padding: 20px;
    border: 1px solid #dee2e6;
    border-radius: 8px;
}
</style>
```

## Social Media Integration

### Social Feed Widget

```razor
@page "/social-feed"
@using BlazorFrame

<div class="social-container">
    <h2>Latest Updates</h2>
    
    <!-- Social feed with strict security -->
    <BlazorFrame Src="https://social.example.com/feed/embed"
                Width="100%"
                EnableAutoResize="true"
                SecurityOptions="@socialSecurityOptions"
                CspOptions="@socialCspOptions"
                OnValidatedMessage="HandleSocialMessage"
                OnSecurityViolation="HandleSocialViolation"
                class="social-feed" />
</div>

@code {
    // Social feeds don't need forms/popups, use strict sandbox
    private readonly MessageSecurityOptions socialSecurityOptions = new MessageSecurityOptions()
        .ForProduction()
        .WithStrictSandbox()          // No forms or popups needed
        .RequireHttps()
        .ValidateAndThrow();

    // CSP configuration for social media content
    private readonly CspOptions socialCspOptions = new CspOptions()
        .AllowSelf()
        .AllowFrameSources("https://social.example.com", "https://cdn.social.example.com")
        .AllowHttpsFrames()
        .WithCustomDirective("img-src", "'self'", "https:", "data:")
        .WithCustomDirective("media-src", "https:");

    private async Task HandleSocialMessage(IframeMessage message)
    {
        try
        {
            var socialData = JsonSerializer.Deserialize<SocialMessage>(message.Data);
            
            switch (socialData?.Type)
            {
                case "post-interaction":
                    await TrackInteraction(socialData.PostId, socialData.Action);
                    break;
                    
                case "scroll-position":
                    await UpdateScrollTracking(socialData.Position);
                    break;
                    
                case "content-loaded":
                    await OnContentLoaded(socialData.PostCount);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing social message");
        }
    }

    private async Task HandleSocialViolation(IframeMessage violation)
    {
        // Social media violations might indicate malicious content
        Logger.LogWarning("Social media security violation: {Error}", violation.ValidationError);
        
        // Could implement content filtering or user warnings
        if (violation.ValidationError?.Contains("malicious") == true)
        {
            await ShowUserWarning("Potentially unsafe content detected");
        }
    }
    
    public class SocialMessage
    {
        public string Type { get; set; } = "";
        public string PostId { get; set; } = "";
        public string Action { get; set; } = "";
        public int Position { get; set; }
        public int PostCount { get; set; }
    }
}
```

## Payment Integration

### Secure Payment Widget

```razor
@page "/payment"
@using BlazorFrame

<div class="payment-container">
    <h2>Secure Payment</h2>
    
    <div class="alert alert-info">
        <i class="fas fa-shield-alt"></i>
        This payment form is secured with industry-standard encryption and sandbox isolation.
    </div>
    
    <!-- Maximum security payment widget -->
    <BlazorFrame Src="@paymentUrl"
                Width="100%"
                Height="400px"
                EnableAutoResize="false"
                AllowedOrigins="@paymentOrigins"
                SecurityOptions="@paymentSecurityOptions"
                CspOptions="@paymentCspOptions"
                OnValidatedMessage="HandlePaymentMessage"
                OnSecurityViolation="HandlePaymentViolation"
                OnInitializationError="HandlePaymentError"
                class="payment-widget" />
                
    <div class="payment-status mt-3">
        <span class="badge @GetStatusBadgeClass()">@paymentStatus</span>
    </div>
</div>

@code {
    private string paymentUrl = "https://secure-payments.example.com/checkout";
    private string paymentStatus = "Initializing";
    
    // Whitelist only the payment provider's domains
    private readonly List<string> paymentOrigins = new()
    {
        "https://secure-payments.example.com",
        "https://api.secure-payments.example.com"
    };
    
    // Maximum security for payment processing
    private readonly MessageSecurityOptions paymentSecurityOptions = new MessageSecurityOptions()
        .ForPaymentWidget()           // Highest security preset
        .ValidateAndThrow();          // Ensure configuration is valid

    // Strict CSP for payment security
    private readonly CspOptions paymentCspOptions = new CspOptions()
        .ForProduction()
        .AllowFrameSources("https://secure-payments.example.com")
        .WithScriptNonce(GenerateSecureNonce())
        .WithCustomDirective("connect-src", "'self'", "https://api.secure-payments.example.com")
        .WithCustomDirective("form-action", "https://secure-payments.example.com");

    private async Task HandlePaymentMessage(IframeMessage message)
    {
        try
        {
            var paymentData = JsonSerializer.Deserialize<PaymentMessage>(message.Data);
            
            switch (paymentData?.Type)
            {
                case "payment-started":
                    paymentStatus = "Processing Payment";
                    StateHasChanged();
                    break;
                    
                case "payment-success":
                    paymentStatus = "Payment Successful";
                    await ProcessSuccessfulPayment(paymentData.TransactionId);
                    break;
                    
                case "payment-failed":
                    paymentStatus = "Payment Failed";
                    await HandlePaymentFailure(paymentData.ErrorCode, paymentData.ErrorMessage);
                    break;
                    
                case "payment-cancelled":
                    paymentStatus = "Payment Cancelled";
                    await HandlePaymentCancellation();
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing payment message");
            paymentStatus = "Payment Error";
            StateHasChanged();
        }
    }

    private async Task HandlePaymentViolation(IframeMessage violation)
    {
        // Payment security violations are critical
        Logger.LogCritical("PAYMENT SECURITY VIOLATION: {Error} from {Origin}", 
            violation.ValidationError, violation.Origin);
            
        paymentStatus = "Security Error";
        StateHasChanged();
        
        // Immediate security response
        await NotifySecurityTeam(violation);
        await DisablePaymentWidget();
    }

    private async Task HandlePaymentError(Exception ex)
    {
        Logger.LogError(ex, "Payment widget initialization failed");
        paymentStatus = "Initialization Failed";
        StateHasChanged();
        
        // Show user-friendly error
        await ShowErrorMessage("Payment system is temporarily unavailable. Please try again later.");
    }

    private string GetStatusBadgeClass() => paymentStatus switch
    {
        "Payment Successful" => "badge-success",
        "Payment Failed" or "Payment Error" or "Security Error" => "badge-danger",
        "Payment Cancelled" => "badge-warning",
        "Processing Payment" => "badge-info",
        _ => "badge-secondary"
    };
    
    private static string GenerateSecureNonce() => 
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    
    public class PaymentMessage
    {
        public string Type { get; set; } = "";
        public string TransactionId { get; set; } = "";
        public string ErrorCode { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public decimal Amount { get; set; }
    }
}

<style>
.payment-widget {
    border: 2px solid #28a745;
    border-radius: 8px;
    background: #f8fff9;
}

.payment-status .badge {
    font-size: 1rem;
    padding: 8px 12px;
}
</style>
```

## Analytics Dashboard

### Embedded Analytics with Responsive Design

```razor
@page "/analytics"
@using BlazorFrame

<div class="analytics-container">
    <h2>Analytics Dashboard</h2>
    
    <div class="row">
        <div class="col-md-12">
            <!-- Responsive analytics iframe -->
            <BlazorFrame Src="@GetAnalyticsUrl()"
                        Width="100%"
                        Height="@GetAnalyticsHeight()"
                        EnableAutoResize="@enableAutoResize"
                        EnableScroll="@isMobile"
                        SecurityOptions="@analyticsSecurityOptions"
                        OnValidatedMessage="HandleAnalyticsMessage"
                        OnLoad="HandleAnalyticsLoad"
                        class="@GetAnalyticsCssClass()" />
        </div>
    </div>
    
    <div class="analytics-controls mt-3">
        <button class="btn btn-secondary" @onclick="ToggleView">
            @(isMobile ? "Desktop View" : "Mobile View")
        </button>
        
        <button class="btn btn-outline-secondary" @onclick="RefreshDashboard">
            <i class="fas fa-sync"></i> Refresh
        </button>
    </div>
</div>

@code {
    private bool isMobile = false;
    private bool enableAutoResize = true;
    private DateTime lastRefresh = DateTime.Now;
    
    // Analytics typically needs basic interactivity but not forms
    private readonly MessageSecurityOptions analyticsSecurityOptions = new MessageSecurityOptions()
        .ForTrustedContent()
        .WithBasicSandbox()           // Allow scripts and same-origin
        .RequireHttps();

    private string GetAnalyticsUrl()
    {
        var baseUrl = "https://analytics.example.com/dashboard";
        var parameters = new Dictionary<string, string>
        {
            ["view"] = isMobile ? "mobile" : "desktop",
            ["theme"] = "light",
            ["refresh"] = lastRefresh.Ticks.ToString()
        };
        
        var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
        return $"{baseUrl}?{queryString}";
    }

    private string GetAnalyticsHeight() => isMobile ? "400px" : "600px";
    
    private string GetAnalyticsCssClass() => isMobile 
        ? "analytics-iframe mobile-view" 
        : "analytics-iframe desktop-view";

    private async Task HandleAnalyticsMessage(IframeMessage message)
    {
        try
        {
            var analyticsData = JsonSerializer.Deserialize<AnalyticsMessage>(message.Data);
            
            switch (analyticsData?.Type)
            {
                case "data-updated":
                    await OnDataUpdated(analyticsData.Timestamp);
                    break;
                    
                case "export-request":
                    await HandleExportRequest(analyticsData.Format, analyticsData.DateRange);
                    break;
                    
                case "filter-changed":
                    await OnFilterChanged(analyticsData.Filters);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing analytics message");
        }
    }

    private async Task HandleAnalyticsLoad()
    {
        Console.WriteLine("Analytics dashboard loaded successfully");
        StateHasChanged();
    }

    private void ToggleView()
    {
        isMobile = !isMobile;
        enableAutoResize = !isMobile; // Disable auto-resize on mobile
        StateHasChanged();
    }

    private void RefreshDashboard()
    {
        lastRefresh = DateTime.Now;
        StateHasChanged();
    }
    
    public class AnalyticsMessage
    {
        public string Type { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string Format { get; set; } = "";
        public string DateRange { get; set; } = "";
        public Dictionary<string, object> Filters { get; set; } = new();
    }
}

<style>
.analytics-iframe.mobile-view {
    border: 1px solid #6c757d;
    border-radius: 4px;
}

.analytics-iframe.desktop-view {
    border: 2px solid #007bff;
    border-radius: 8px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.analytics-controls {
    text-align: center;
}
</style>
```

## Multi-Domain Widget Integration

### Widget Hub with Multiple Sources

```razor
@page "/widgets"
@using BlazorFrame

<div class="widget-hub">
    <h2>Widget Dashboard</h2>
    
    <div class="row">
        @foreach (var widget in widgets)
        {
            <div class="col-md-6 mb-4">
                <div class="widget-card">
                    <h5>@widget.Title</h5>
                    
                    <BlazorFrame Src="@widget.Url"
                                Width="100%"
                                Height="300px"
                                EnableAutoResize="false"
                                AllowedOrigins="@widget.AllowedOrigins"
                                SecurityOptions="@widget.SecurityOptions"
                                OnValidatedMessage="@((msg) => HandleWidgetMessage(widget.Id, msg))"
                                OnSecurityViolation="@((violation) => HandleWidgetViolation(widget.Id, violation))"
                                class="widget-iframe" />
                </div>
            </div>
        }
    </div>
</div>

@code {
    private readonly List<WidgetConfig> widgets = new()
    {
        new WidgetConfig
        {
            Id = "weather",
            Title = "Weather Widget",
            Url = "https://weather.example.com/widget",
            AllowedOrigins = new() { "https://weather.example.com" },
            SecurityOptions = new MessageSecurityOptions()
                .WithBasicSandbox()
                .RequireHttps()
        },
        
        new WidgetConfig
        {
            Id = "news",
            Title = "News Feed",
            Url = "https://news.example.com/feed",
            AllowedOrigins = new() { "https://news.example.com", "https://cdn.news.example.com" },
            SecurityOptions = new MessageSecurityOptions()
                .WithStrictSandbox()    // News doesn't need forms
                .RequireHttps()
        },
        
        new WidgetConfig
        {
            Id = "calendar",
            Title = "Calendar",
            Url = "https://calendar.example.com/embed",
            AllowedOrigins = new() { "https://calendar.example.com" },
            SecurityOptions = new MessageSecurityOptions()
                .WithPermissiveSandbox()  // Calendar might need forms
                .RequireHttps()
        }
    };

    private async Task HandleWidgetMessage(string widgetId, IframeMessage message)
    {
        Logger.LogInformation("Widget {WidgetId} message: {Type}", widgetId, 
            ExtractMessageType(message.Data));
            
        // Route messages based on widget type
        switch (widgetId)
        {
            case "weather":
                await HandleWeatherMessage(message);
                break;
                
            case "news":
                await HandleNewsMessage(message);
                break;
                
            case "calendar":
                await HandleCalendarMessage(message);
                break;
        }
    }

    private async Task HandleWidgetViolation(string widgetId, IframeMessage violation)
    {
        Logger.LogWarning("Widget {WidgetId} security violation: {Error}", 
            widgetId, violation.ValidationError);
            
        // Could implement widget-specific violation handling
        await NotifyWidgetProvider(widgetId, violation);
    }

    private static string ExtractMessageType(string messageJson)
    {
        try
        {
            var doc = JsonDocument.Parse(messageJson);
            return doc.RootElement.TryGetProperty("type", out var typeElement) 
                ? typeElement.GetString() ?? "unknown" 
                : "unknown";
        }
        catch
        {
            return "invalid";
        }
    }
    
    public class WidgetConfig
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public List<string> AllowedOrigins { get; set; } = new();
        public MessageSecurityOptions SecurityOptions { get; set; } = new();
    }
}

<style>
.widget-card {
    background: white;
    border: 1px solid #dee2e6;
    border-radius: 8px;
    padding: 15px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.widget-iframe {
    border: 1px solid #e9ecef;
    border-radius: 4px;
}
</style>
```

## Development vs Production Scenarios

### Environment-Aware Configuration

```razor
@using BlazorFrame
@inject IWebHostEnvironment Environment

<BlazorFrame Src="@GetEnvironmentUrl()"
            SecurityOptions="@GetEnvironmentSecurityOptions()"
            OnValidatedMessage="HandleMessage"
            OnSecurityViolation="HandleViolation" />

@code {
    private string GetEnvironmentUrl()
    {
        return Environment.IsDevelopment() 
            ? "http://localhost:3000/widget"    // Local development
            : "https://widget.example.com";     // Production
    }

    private MessageSecurityOptions GetEnvironmentSecurityOptions()
    {
        if (Environment.IsDevelopment())
        {
            return new MessageSecurityOptions()
                .ForDevelopment()               // Relaxed for development
                .WithPermissiveSandbox()        // Allow more interactions
                .Validate();                    // Validate but don't throw
        }
        else
        {
            return new MessageSecurityOptions()
                .ForProduction()                // Strict for production
                .WithStrictSandbox()           // Limited interactions
                .ValidateAndThrow();           // Throw on configuration errors
        }
    }

    private async Task HandleMessage(IframeMessage message)
    {
        if (Environment.IsDevelopment())
        {
            // Verbose logging in development
            Console.WriteLine($"[DEV] Message from {message.Origin}: {message.Data}");
        }
        
        // Handle message based on environment
        await ProcessMessage(message);
    }

    private async Task HandleViolation(IframeMessage violation)
    {
        if (Environment.IsDevelopment())
        {
            // Show detailed violation info in development
            Console.WriteLine($"[DEV] Security violation: {violation.ValidationError}");
            Console.WriteLine($"[DEV] Origin: {violation.Origin}");
            Console.WriteLine($"[DEV] Data: {violation.Data}");
        }
        else
        {
            // Log violations securely in production
            Logger.LogWarning("Security violation: {Error}", violation.ValidationError);
            await NotifySecurityTeam(violation);
        }
    }
}
```

---
