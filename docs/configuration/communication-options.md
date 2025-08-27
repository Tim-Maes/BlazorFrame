# Communication Configuration

**Cross-frame messaging, navigation tracking, and event handling for BlazorFrame**

This guide covers all aspects of configuring communication between your Blazor application and iframe content, including message validation, origin control, event handling, **bidirectional communication**, and **navigation tracking**.

## Communication Overview

BlazorFrame provides comprehensive communication capabilities:

- **Iframe -> Host** (`OnValidatedMessage`) - Receive messages from iframe with validation
- **Host -> Iframe** (`SendMessageAsync`) - Send messages to iframe with security validation
- **Navigation Events** (`OnNavigation`) - Track URL changes with query parameters
- **Raw Messages** (`OnMessage`) - Legacy support for simple scenarios

## Navigation Tracking

### Basic Navigation Event Handling

```razor
<BlazorFrame @ref="iframeRef"
            Src="@widgetUrl"
            EnableNavigationTracking="true"
            OnNavigation="HandleNavigation"
            OnUrlChanged="HandleUrlChanged" />

@code {
    private BlazorFrame? iframeRef;
    
    private async Task HandleNavigation(NavigationEvent navigation)
    {
        Logger.LogInformation("Navigation to {Url}", navigation.Url);
        
        // Access URL components
        Logger.LogInformation("Pathname: {Pathname}", navigation.Pathname);
        Logger.LogInformation("Query: {Query}", navigation.Search);
        Logger.LogInformation("Hash: {Hash}", navigation.Hash);
        Logger.LogInformation("Navigation Type: {Type}", navigation.NavigationType);
        
        // Process query parameters
        foreach (var param in navigation.QueryParameters)
        {
            Logger.LogInformation("Query param {Key}: {Value}", param.Key, param.Value);
        }
        
        // Handle different navigation types
        switch (navigation.NavigationType)
        {
            case "load":
                await HandleInitialLoad(navigation);
                break;
            case "pushstate":
            case "replacestate":
                await HandleProgrammaticNavigation(navigation);
                break;
            case "popstate":
                await HandleHistoryNavigation(navigation);
                break;
            case "hashchange":
                await HandleHashChange(navigation);
                break;
            case "postmessage":
                await HandleMessageNavigation(navigation);
                break;
        }
    }
    
    private Task HandleUrlChanged(string newUrl)
    {
        Logger.LogInformation("Simple URL change: {Url}", newUrl);
        return Task.CompletedTask;
    }
}
```

### Advanced Navigation Handling

```razor
<BlazorFrame Src="@widgetUrl"
            EnableNavigationTracking="@IsSameOriginWidget()"
            OnNavigation="HandleAdvancedNavigation" />

@code {
    private readonly Dictionary<string, object> navigationHistory = new();
    
    private bool IsSameOriginWidget()
    {
        try
        {
            var widgetUri = new Uri(widgetUrl);
            var hostUri = new Uri(NavigationManager.BaseUri);
            return widgetUri.Host.Equals(hostUri.Host, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
    
    private async Task HandleAdvancedNavigation(NavigationEvent navigation)
    {
        // Store navigation in history
        navigationHistory[navigation.Timestamp.ToString()] = navigation;
        
        // Extract specific query parameters
        if (navigation.QueryParameters.TryGetValue("userId", out var userId))
        {
            await HandleUserContext(userId);
        }
        
        if (navigation.QueryParameters.TryGetValue("action", out var action))
        {
            await HandleActionRequest(action, navigation.QueryParameters);
        }
        
        // Track analytics
        await TrackPageView(navigation.Pathname, navigation.QueryParameters);
        
        // Update parent application state based on iframe navigation
        await SyncApplicationState(navigation);
    }
    
    private async Task HandleUserContext(string userId)
    {
        // Load user-specific data when iframe navigates to user pages
        var userData = await UserService.GetUserDataAsync(userId);
        await iframeRef.SendTypedMessageAsync("user-context", userData);
    }
    
    private async Task HandleActionRequest(string action, Dictionary<string, string> parameters)
    {
        // Handle action requests from iframe navigation
        switch (action.ToLowerInvariant())
        {
            case "export":
                await HandleExportRequest(parameters);
                break;
            case "share":
                await HandleShareRequest(parameters);
                break;
            case "settings":
                await ShowSettingsDialog(parameters);
                break;
        }
    }
}
```

### Cross-Origin Navigation Limitations

```razor
@code {
    // Navigation tracking has limitations with cross-origin iframes
    private async Task HandleCrossOriginNavigation()
    {
        // For cross-origin iframes, navigation events can be sent via postMessage
        // The iframe content needs to explicitly send navigation messages:
        /*
        JavaScript in iframe:
        window.addEventListener('popstate', function(event) {
            parent.postMessage({
                type: 'navigation',
                url: window.location.href,
                pathname: window.location.pathname,
                search: window.location.search,
                hash: window.location.hash
            }, '*');
        });
        */
    }
}
```

### Navigation Security Considerations

```razor
<BlazorFrame Src="@trustedWidgetUrl"
            EnableNavigationTracking="true"
            SecurityOptions="@navigationSecurityOptions"
            OnNavigation="HandleSecureNavigation" />

@code {
    private readonly MessageSecurityOptions navigationSecurityOptions = new MessageSecurityOptions()
        .ForProduction()
        .WithStrictSandbox(); // Restrict iframe capabilities
    
    private async Task HandleSecureNavigation(NavigationEvent navigation)
    {
        // Validate navigation is to allowed domains
        var allowedDomains = new[] { "trusted-widget.example.com", "api.example.com" };
        var navigationUri = new Uri(navigation.Url);
        
        if (!allowedDomains.Contains(navigationUri.Host))
        {
            Logger.LogWarning("Navigation to untrusted domain: {Domain}", navigationUri.Host);
            await HandleUntrustedNavigation(navigation);
            return;
        }
        
        // Sanitize query parameters
        var sanitizedParams = SanitizeQueryParameters(navigation.QueryParameters);
        
        // Process trusted navigation
        await ProcessTrustedNavigation(navigation, sanitizedParams);
    }
    
    private Dictionary<string, string> SanitizeQueryParameters(Dictionary<string, string> parameters)
    {
        var sanitized = new Dictionary<string, string>();
        var allowedParams = new[] { "userId", "action", "page", "filter" };
        
        foreach (var param in parameters)
        {
            if (allowedParams.Contains(param.Key.ToLowerInvariant()))
            {
                // Basic XSS prevention
                var cleanValue = param.Value
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("'", "&#x27;")
                    .Replace("\"", "&quot;");
                    
                sanitized[param.Key] = cleanValue;
            }
        }
        
        return sanitized;
    }
}
```