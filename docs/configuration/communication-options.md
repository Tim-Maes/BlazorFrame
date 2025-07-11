# Communication Configuration

**Cross-frame messaging and event handling for BlazorFrame**

This guide covers all aspects of configuring communication between your Blazor application and iframe content, including message validation, origin control, and event handling.

## Message Handling Overview

BlazorFrame provides two main approaches to handling messages:

- **Validated Messages** (`OnValidatedMessage`) - Recommended for new implementations
- **Raw Messages** (`OnMessage`) - Legacy support for simple scenarios

## Basic Message Configuration

### Essential Message Handling

```razor
<BlazorFrame Src="@widgetUrl"
            SecurityOptions="@messageOptions"
            OnValidatedMessage="HandleValidatedMessage"
            OnSecurityViolation="HandleSecurityViolation" />

@code {
    private readonly MessageSecurityOptions messageOptions = new MessageSecurityOptions()
        .ForProduction()
        .WithBasicSandbox();
    
    private async Task HandleValidatedMessage(IframeMessage message)
    {
        Logger.LogInformation("Received message from {Origin}: {Data}", 
            message.Origin, message.Data);
            
        // Process the validated message
        await ProcessMessage(message);
    }
    
    private async Task HandleSecurityViolation(IframeMessage violation)
    {
        Logger.LogWarning("Security violation: {Error}", violation.ValidationError);
        
        // Handle security issues
        await HandleSecurityIssue(violation);
    }
}
```

## Origin Validation

### Explicit Origin Configuration
```razor
<BlazorFrame Src="@trustedWidgetUrl"
            AllowedOrigins="@allowedOrigins"
            SecurityOptions="@securityOptions" />

@code {
    private readonly List<string> allowedOrigins = new()
    {
        "https://widget.example.com",
        "https://api.example.com",
        "https://cdn.example.com"
    };
    
    private readonly MessageSecurityOptions securityOptions = new()
    {
        ValidateOrigins = true,
        StrictOriginMatching = true,
        LogSecurityViolations = true
    };
}
```

### Dynamic Origin Management

```razor
<BlazorFrame Src="@currentWidgetUrl"
            AllowedOrigins="@GetAllowedOrigins()"
            OnValidatedMessage="HandleMessage" />

@code {
    private List<string> GetAllowedOrigins()
    {
        var origins = new List<string>();
        
        // Add origins based on current configuration
        if (IsDevelopment())
        {
            origins.Add("http://localhost:3000");
            origins.Add("http://localhost:8080");
        }
        
        // Add production origins
        origins.Add("https://widgets.myapp.com");
        
        // Add partner origins based on current user/tenant
        origins.AddRange(GetPartnerOrigins());
        
        return origins;
    }
    
    private List<string> GetPartnerOrigins()
    {
        // Dynamically determine allowed partner origins
        return currentUser.PartnerDomains ?? new List<string>();
    }
}
```

### Wildcard and Pattern Matching

```razor
@code {
    private readonly MessageSecurityOptions wildcardOptions = new()
    {
        ValidateOrigins = true,
        AllowWildcardOrigins = true,
        AllowedOriginPatterns = new List<string>
        {
            "https://*.example.com",        // Subdomains of example.com
            "https://app-*.myservice.com",  // Pattern matching
            "https://tenant-*.widgets.com"  // Multi-tenant patterns
        },
        StrictPatternMatching = true
    };
}
```

## Message Validation

### Comprehensive Message Validation

```razor
<BlazorFrame Src="@widgetUrl"
            SecurityOptions="@validationOptions"
            OnValidatedMessage="HandleValidatedMessage"
            OnSecurityViolation="HandleValidationViolation" />

@code {
    private readonly MessageSecurityOptions validationOptions = new()
    {
        EnableStrictValidation = true,
        MaxMessageSize = 32 * 1024,        // 32KB limit
        MaxJsonDepth = 10,                 // Max nesting depth
        MaxObjectProperties = 100,          // Max properties per object
        MaxArrayElements = 1000,           // Max array length
        ValidateMessageStructure = true,    // Validate JSON structure
        FilterMaliciousContent = true,     // Filter dangerous patterns
        SanitizeStringValues = true,       // Sanitize string content
        AllowHtmlContent = false,          // Block HTML in messages
        LogSecurityViolations = true
    };
    
    private async Task HandleValidatedMessage(IframeMessage message)
    {
        // Message has passed all validation
        switch (message.MessageType)
        {
            case "user-action":
                await HandleUserAction(message);
                break;
                
            case "data-update":
                await HandleDataUpdate(message);
                break;
                
            case "navigation":
                await HandleNavigation(message);
                break;
                
            default:
                Logger.LogWarning("Unknown message type: {Type}", message.MessageType);
                break;
        }
    }
}
```

### Custom Message Validation

```razor
@code {
    private readonly MessageSecurityOptions customValidationOptions = new()
    {
        EnableStrictValidation = true,
        CustomValidators = new List<IMessageValidator>
        {
            new BusinessRuleValidator(),
            new SchemaValidator(),
            new SecurityPolicyValidator()
        }
    };
    
    public class BusinessRuleValidator : IMessageValidator
    {
        public ValidationResult Validate(IframeMessage message)
        {
            // Custom business rule validation
            if (message.MessageType == "payment" && !IsValidPaymentMessage(message))
            {
                return ValidationResult.Failure("Invalid payment message format");
            }
            
            return ValidationResult.Success();
        }
        
        private bool IsValidPaymentMessage(IframeMessage message)
        {
            // Implement payment-specific validation
            var data = JsonSerializer.Deserialize<PaymentMessage>(message.Data);
            return data.Amount > 0 && !string.IsNullOrEmpty(data.Currency);
        }
    }
}
```

## Message Types and Routing

### Structured Message Handling

```razor
<BlazorFrame Src="@widgetUrl"
            OnValidatedMessage="RouteMessage" />

@code {
    private async Task RouteMessage(IframeMessage message)
    {
        try
        {
            switch (message.MessageType?.ToLowerInvariant())
            {
                case "user-event":
                    await HandleUserEvent(message);
                    break;
                    
                case "data-request":
                    await HandleDataRequest(message);
                    break;
                    
                case "configuration-change":
                    await HandleConfigurationChange(message);
                    break;
                    
                case "error-report":
                    await HandleErrorReport(message);
                    break;
                    
                case "analytics-event":
                    await HandleAnalyticsEvent(message);
                    break;
                    
                default:
                    Logger.LogWarning("Unhandled message type: {Type}", message.MessageType);
                    await HandleUnknownMessage(message);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing message: {Type}", message.MessageType);
            await HandleMessageError(message, ex);
        }
    }
    
    private async Task HandleUserEvent(IframeMessage message)
    {
        var userEvent = JsonSerializer.Deserialize<UserEventMessage>(message.Data);
        
        // Process user interaction
        await Analytics.TrackUserEvent(userEvent.EventName, userEvent.Properties);
        
        // Update application state
        await UpdateUserInterface(userEvent);
        
        // Send response back to iframe
        await SendResponseToIframe(message.Origin, new
        {
            type = "user-event-processed",
            eventId = userEvent.EventId,
            status = "success"
        });
    }
    
    private async Task HandleDataRequest(IframeMessage message)
    {
        var dataRequest = JsonSerializer.Deserialize<DataRequestMessage>(message.Data);
        
        // Validate request permissions
        if (!await IsAuthorizedForData(dataRequest.ResourceType))
        {
            await SendErrorResponse(message.Origin, "Unauthorized data request");
            return;
        }
        
        // Fetch requested data
        var responseData = await DataService.GetData(dataRequest.ResourceType, dataRequest.Parameters);
        
        // Send data back to iframe
        await SendResponseToIframe(message.Origin, new
        {
            type = "data-response",
            requestId = dataRequest.RequestId,
            data = responseData
        });
    }
}
```

### Type-Safe Message Handling

```csharp
// Define message types
public record UserEventMessage(string EventId, string EventName, Dictionary<string, object> Properties);
public record DataRequestMessage(string RequestId, string ResourceType, Dictionary<string, object> Parameters);
public record ConfigurationChangeMessage(string Setting, object Value);
public record ErrorReportMessage(string ErrorType, string Message, string StackTrace);

// Type-safe message processor
public class MessageProcessor
{
    public async Task<T> ProcessMessage<T>(IframeMessage message) where T : class
    {
        try
        {
            var options = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            };
            
            return JsonSerializer.Deserialize<T>(message.Data, options);
        }
        catch (JsonException ex)
        {
            throw new MessageValidationException($"Failed to deserialize message of type {typeof(T).Name}", ex);
        }
    }
}
```

## Bidirectional Communication

### Sending Messages to Iframe

```razor
<BlazorFrame @ref="iframeRef"
            Src="@widgetUrl"
            OnValidatedMessage="HandleMessage" />

<button class="btn btn-primary" @onclick="SendDataToIframe">
    Send Data to Iframe
</button>

@code {
    private BlazorFrame? iframeRef;
    
    private async Task SendDataToIframe()
    {
        if (iframeRef == null) return;
        
        var messageData = new
        {
            type = "data-update",
            timestamp = DateTime.UtcNow,
            data = new
            {
                userId = currentUser.Id,
                preferences = currentUser.Preferences,
                theme = currentTheme
            }
        };
        
        await iframeRef.SendMessageAsync(messageData);
    }
    
    private async Task HandleMessage(IframeMessage message)
    {
        if (message.MessageType == "request-user-data")
        {
            // Respond to iframe's request for user data
            await SendUserDataToIframe();
        }
    }
    
    private async Task SendUserDataToIframe()
    {
        var userData = new
        {
            type = "user-data-response",
            user = new
            {
                id = currentUser.Id,
                name = currentUser.Name,
                email = currentUser.Email,
                permissions = currentUser.Permissions
            }
        };
        
        await iframeRef.SendMessageAsync(userData);
    }
}
```

### Request-Response Pattern

```razor
@code {
    private readonly Dictionary<string, TaskCompletionSource<object>> pendingRequests = new();
    
    private async Task<T> SendRequestToIframe<T>(string requestType, object data)
    {
        var requestId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<object>();
        
        pendingRequests[requestId] = tcs;
        
        try
        {
            // Send request to iframe
            await iframeRef.SendMessageAsync(new
            {
                type = requestType,
                requestId = requestId,
                data = data
            });
            
            // Wait for response with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            cts.Token.Register(() => tcs.TrySetCanceled());
            
            var response = await tcs.Task;
            return JsonSerializer.Deserialize<T>(response.ToString());
        }
        finally
        {
            pendingRequests.Remove(requestId);
        }
    }
    
    private async Task HandleMessage(IframeMessage message)
    {
        // Handle responses to our requests
        if (message.MessageType?.EndsWith("-response") == true)
        {
            var responseData = JsonSerializer.Deserialize<ResponseMessage>(message.Data);
            
            if (pendingRequests.TryGetValue(responseData.RequestId, out var tcs))
            {
                tcs.SetResult(responseData.Data);
            }
        }
    }
    
    // Usage example
    private async Task GetIframeData()
    {
        try
        {
            var iframeData = await SendRequestToIframe<IframeDataResponse>(
                "get-data", 
                new { category = "user-preferences" }
            );
            
            Logger.LogInformation("Received iframe data: {Data}", iframeData);
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("Request to iframe timed out");
        }
    }
}
```

## Event Handling

### Comprehensive Event Configuration

```razor
<BlazorFrame Src="@widgetUrl"
            OnValidatedMessage="HandleValidatedMessage"
            OnSecurityViolation="HandleSecurityViolation"
            OnLoad="HandleIframeLoad"
            OnError="HandleIframeError"
            OnResize="HandleIframeResize"
            OnInitializationError="HandleInitializationError" />

@code {
    private async Task HandleIframeLoad()
    {
        Logger.LogInformation("Iframe loaded successfully");
        
        // Send initial configuration to iframe
        await SendInitialConfiguration();
        
        // Update UI state
        isIframeReady = true;
        StateHasChanged();
    }
    
    private async Task HandleIframeError(Exception error)
    {
        Logger.LogError(error, "Iframe error occurred");
        
        // Show error message to user
        await ShowErrorMessage("Widget failed to load. Please try again.");
        
        // Attempt reload if appropriate
        if (ShouldAttemptReload(error))
        {
            await ReloadIframe();
        }
    }
    
    private async Task HandleIframeResize(ResizeEventArgs args)
    {
        Logger.LogDebug("Iframe resized to {Width}x{Height}", args.Width, args.Height);
        
        // Adjust surrounding layout
        await AdjustLayoutForNewSize(args.Width, args.Height);
    }
    
    private async Task HandleInitializationError(Exception error)
    {
        Logger.LogError(error, "Iframe initialization failed");
        
        // Fallback to alternative content
        await ShowFallbackContent();
    }
}
```

### Error Recovery and Retry Logic

```razor
@code {
    private int retryCount = 0;
    private const int MaxRetries = 3;
    
    private async Task HandleIframeError(Exception error)
    {
        retryCount++;
        
        if (retryCount <= MaxRetries)
        {
            Logger.LogWarning("Iframe error (attempt {Count}/{Max}): {Error}", 
                retryCount, MaxRetries, error.Message);
                
            // Exponential backoff
            var delayMs = Math.Pow(2, retryCount) * 1000;
            await Task.Delay(TimeSpan.FromMilliseconds(delayMs));
            
            // Retry loading
            await ReloadIframe();
        }
        else
        {
            Logger.LogError("Iframe failed after {Count} attempts: {Error}", 
                MaxRetries, error.Message);
                
            // Show permanent error state
            await ShowPermanentError();
        }
    }
    
    private async Task ReloadIframe()
    {
        // Reset iframe by changing source temporarily
        var originalSrc = widgetUrl;
        widgetUrl = "about:blank";
        StateHasChanged();
        
        await Task.Delay(100); // Brief pause
        
        widgetUrl = originalSrc;
        StateHasChanged();
    }
}
```

## Performance Optimization

### Message Throttling

```razor
@code {
    private readonly Dictionary<string, DateTime> lastMessageTimes = new();
    private readonly TimeSpan messageThrottle = TimeSpan.FromMilliseconds(100);
    
    private async Task HandleValidatedMessage(IframeMessage message)
    {
        var messageKey = $"{message.Origin}:{message.MessageType}";
        
        // Throttle rapid messages
        if (lastMessageTimes.TryGetValue(messageKey, out var lastTime))
        {
            if (DateTime.UtcNow - lastTime < messageThrottle)
            {
                // Skip this message due to throttling
                return;
            }
        }
        
        lastMessageTimes[messageKey] = DateTime.UtcNow;
        
        // Process the message
        await ProcessMessage(message);
    }
}
```

### Message Queuing

```razor
@code {
    private readonly Queue<IframeMessage> messageQueue = new();
    private bool isProcessingQueue = false;
    
    private async Task HandleValidatedMessage(IframeMessage message)
    {
        // Add message to queue
        messageQueue.Enqueue(message);
        
        // Process queue if not already processing
        if (!isProcessingQueue)
        {
            await ProcessMessageQueue();
        }
    }
    
    private async Task ProcessMessageQueue()
    {
        isProcessingQueue = true;
        
        try
        {
            while (messageQueue.Count > 0)
            {
                var message = messageQueue.Dequeue();
                
                try
                {
                    await ProcessMessage(message);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error processing queued message: {Type}", message.MessageType);
                }
                
                // Small delay to prevent overwhelming the UI thread
                await Task.Delay(1);
            }
        }
        finally
        {
            isProcessingQueue = false;
        }
    }
}
```

## Communication Best Practices

### Do
- **Use OnValidatedMessage** for new implementations instead of OnMessage
- **Validate all message origins** to prevent malicious content
- **Implement proper error handling** for all communication scenarios
- **Use type-safe message structures** with proper serialization
- **Log security violations** for monitoring and debugging
- **Implement message throttling** to prevent abuse
- **Handle iframe load failures** gracefully with retry logic
- **Use request-response patterns** for structured communication

### Don't
- **Trust messages blindly** - Always validate content and origin
- **Ignore security violations** - Investigate all security events
- **Send sensitive data** without proper encryption
- **Use overly large message limits** that could enable DoS attacks
- **Block the UI thread** with message processing
- **Forget to handle communication errors** - Always have fallback plans
- **Use wildcard origins** in production without careful consideration
- **Skip message validation** for performance reasons

---
