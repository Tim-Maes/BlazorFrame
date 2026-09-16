using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using BlazorFrame.Services;
using BlazorFrame.Models;
using System.Text.Json;

namespace BlazorFrame;

public partial class BlazorFrame : IAsyncDisposable
{
    private ElementReference iframeElement;
    private IJSObjectReference? module;
    private DotNetObjectReference<BlazorFrame>? objRef;
    private readonly MessageValidationService validationService = new();
    private List<string> computedAllowedOrigins = new();
    private bool isInitialized = false;
    private readonly object initializationLock = new();
    private string currentSrcKey = string.Empty;
    private string? lastConfigurationSignature;
    private bool sourceRejected;
    private bool capabilityHandshakeComplete;
    private string? expectedHandshakeChallenge;

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<BlazorFrame>? Logger { get; set; }

    /// <summary>
    /// The URL to load in the iframe
    /// </summary>
    [Parameter] public string Src { get; set; } = string.Empty;
    
    /// <summary>
    /// The width of the iframe. Default is "100%".
    /// </summary>
    [Parameter] public string Width { get; set; } = "100%";
    
    /// <summary>
    /// The height of the iframe. Default is "600px".
    /// </summary>
    [Parameter] public string Height { get; set; } = "600px";

    /// <summary>
    /// Accessible label for the iframe. A title is strongly recommended for screen-reader users.
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// Controls native browser lazy loading. Defaults to eager loading.
    /// </summary>
    [Parameter] public string Loading { get; set; } = "eager";

    /// <summary>
    /// Referrer policy applied to the iframe request.
    /// </summary>
    [Parameter] public string? ReferrerPolicy { get; set; }

    /// <summary>
    /// Permissions policy for iframe features such as fullscreen, camera, or payment.
    /// </summary>
    [Parameter] public string? Allow { get; set; }

    /// <summary>
    /// Loads the frame without ambient cookies and storage when supported by the browser.
    /// </summary>
    [Parameter] public bool Credentialless { get; set; }
    
    /// <summary>
    /// Enable automatic height adjustment based on iframe content. Default is true.
    /// </summary>
    [Parameter] public bool EnableAutoResize { get; set; } = true;
    
    /// <summary>
    /// Enable scrolling on the iframe wrapper. Default is false.
    /// </summary>
    [Parameter] public bool EnableScroll { get; set; } = false;

    /// <summary>
    /// Configuration options for auto-resize behavior including min/max height, polling interval, and debouncing.
    /// </summary>
    [Parameter] public ResizeOptions? ResizeOptions { get; set; }

    /// <summary>
    /// Enable navigation event tracking to capture URL changes with parameters.
    /// Note: Only works for same-origin iframes due to browser security restrictions.
    /// </summary>
    [Parameter] public bool EnableNavigationTracking { get; set; } = false;

    /// <summary>
    /// List of allowed origins for postMessage communication.
    /// If not specified, will auto-derive from the Src URL.
    /// </summary>
    [Parameter] public List<string>? AllowedOrigins { get; set; }

    /// <summary>
    /// Security options for message validation
    /// </summary>
    [Parameter] public MessageSecurityOptions SecurityOptions { get; set; } = new();

    /// <summary>
    /// Event fired when the iframe loads
    /// </summary>
    [Parameter] public EventCallback OnLoad { get; set; }
    
    /// <summary>
    /// Event fired when a message is received (raw JSON string)
    /// </summary>
    [Parameter] public EventCallback<string> OnMessage { get; set; }

    /// <summary>
    /// Event fired when a message is received with full validation details
    /// </summary>
    [Parameter] public EventCallback<IframeMessage> OnValidatedMessage { get; set; }

    /// <summary>
    /// Event fired when a security violation occurs
    /// </summary>
    [Parameter] public EventCallback<IframeMessage> OnSecurityViolation { get; set; }

    /// <summary>
    /// Event fired when JavaScript initialization fails
    /// </summary>
    [Parameter] public EventCallback<Exception> OnInitializationError { get; set; }

    /// <summary>
    /// Event fired when a message is successfully sent to the iframe
    /// </summary>
    [Parameter] public EventCallback<string> OnMessageSent { get; set; }

    /// <summary>
    /// Event fired when sending a message to the iframe fails
    /// </summary>
    [Parameter] public EventCallback<Exception> OnMessageSendFailed { get; set; }

    /// <summary>
    /// Event fired when iframe navigation occurs (only for same-origin iframes)
    /// </summary>
    [Parameter] public EventCallback<NavigationEvent> OnNavigation { get; set; }

    /// <summary>
    /// Event fired when URL changes are detected (includes query parameter changes)
    /// </summary>
    [Parameter] public EventCallback<string> OnUrlChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

    private string WrapperClasses =>
      EnableScroll
        ? "iframe-wrapper scrollable"
        : "iframe-wrapper";

    /// <summary>
    /// Gets the effective sandbox attribute value based on security options configuration
    /// </summary>
    private string? EffectiveSandboxValue => SecurityOptions.GetEffectiveSandboxValue();

    /// <summary>
    /// Gets a unique key for the iframe element to force re-render when needed
    /// </summary>
    private string IframeKey => currentSrcKey;

    private string EffectiveSrc => sourceRejected ? "about:blank" : Src;

    /// <summary>
    /// Gets combined iframe attributes including sandbox and additional attributes
    /// </summary>
    private Dictionary<string, object> IframeAttributes
    {
        get
        {
            var attributes = new Dictionary<string, object>(AdditionalAttributes);

            attributes["src"] = EffectiveSrc;

            if (!string.IsNullOrWhiteSpace(Title))
                attributes["title"] = Title;
            if (!string.IsNullOrWhiteSpace(Loading))
                attributes["loading"] = Loading;
            if (!string.IsNullOrWhiteSpace(ReferrerPolicy))
                attributes["referrerpolicy"] = ReferrerPolicy;
            if (!string.IsNullOrWhiteSpace(Allow))
                attributes["allow"] = Allow;
            if (Credentialless)
                attributes["credentialless"] = true;
            
            var sandboxValue = EffectiveSandboxValue;
            if (!string.IsNullOrEmpty(sandboxValue))
            {
                if (attributes.ContainsKey("sandbox"))
                {
                    Logger?.LogWarning("BlazorFrame: sandbox attribute in AdditionalAttributes is replaced by SecurityOptions");
                }

                attributes["sandbox"] = sandboxValue;
            }
            
            return attributes;
        }
    }

    /// <summary>
    /// Reloads the iframe content by forcing a re-render of the iframe element.
    /// This is useful for refreshing content like PDFs without reloading the entire component.
    /// </summary>
    /// <returns>A task that completes when the reload is initiated</returns>
    public async Task ReloadAsync()
    {
        Logger?.LogDebug("BlazorFrame: Reloading iframe for {Src}", Src);
        
        // Generate a new key to force iframe re-render
        currentSrcKey = Guid.NewGuid().ToString("N");
        
        // Reset initialization state so JS will reinitialize
        lock (initializationLock)
        {
            isInitialized = false;
        }
        
        // Cleanup existing JS resources
        await CleanupJavaScriptAsync();
        
        StateHasChanged();
        
        // The iframe will reinitialize on the next render cycle via OnAfterRenderAsync
    }

    /// <summary>
    /// Reloads the iframe with a new source URL.
    /// </summary>
    /// <param name="newSrc">The new URL to load</param>
    /// <returns>A task that completes when the reload is initiated</returns>
    public async Task ReloadAsync(string newSrc)
    {
        Src = newSrc;
        UpdateAllowedOrigins();
        ValidateSource();
        await ReloadAsync();
    }

    /// <summary>
    /// Sends a message to the iframe content
    /// </summary>
    /// <param name="data">Message data to send</param>
    /// <param name="targetOrigin">Target origin for security (defaults to iframe origin)</param>
    /// <returns>True if message was sent successfully</returns>
    public async Task<bool> SendMessageAsync(object data, string? targetOrigin = null)
    {
        if (module == null || !isInitialized)
        {
            var ex = new InvalidOperationException("BlazorFrame not initialized. Cannot send message.");
            Logger?.LogError(ex, "Attempted to send message before initialization");
            await OnMessageSendFailed.InvokeAsync(ex);
            return false;
        }

        if (string.IsNullOrEmpty(Src))
        {
            var ex = new InvalidOperationException("BlazorFrame Src is not set. Cannot determine target origin.");
            Logger?.LogError(ex, "Attempted to send message without valid Src");
            await OnMessageSendFailed.InvokeAsync(ex);
            return false;
        }

        try
        {
            // Use specified target origin or derive from Src
            var usesDocumentOrigin = targetOrigin is null && UsesDocumentOrigin(Src);
            var usesOpaqueOrigin = targetOrigin is null && Src.StartsWith("data:", StringComparison.OrdinalIgnoreCase);
            var effectiveOrigin = targetOrigin is null
                ? validationService.ExtractOrigin(Src)
                : validationService.NormalizeOrigin(targetOrigin);
            var validationOrigin = effectiveOrigin;
            if (usesDocumentOrigin)
            {
                effectiveOrigin = "https://__blazorframe_document_origin__";
                validationOrigin = "https://__blazorframe_document_origin__";
            }
            if (usesOpaqueOrigin && SecurityOptions.AllowDataUrls)
                effectiveOrigin = "*";

            if (string.IsNullOrEmpty(effectiveOrigin))
            {
                var ex = new ArgumentException($"Cannot determine valid origin from Src: {Src}");
                Logger?.LogError(ex, "Invalid target origin for message");
                await OnMessageSendFailed.InvokeAsync(ex);
                return false;
            }

            // Validate target origin is allowed
            if (!usesDocumentOrigin && !usesOpaqueOrigin && !computedAllowedOrigins.Contains(effectiveOrigin, StringComparer.Ordinal))
            {
                var ex = new UnauthorizedAccessException($"Target origin '{effectiveOrigin}' is not in allowed origins list");
                Logger?.LogWarning(ex, "Attempted to send message to unauthorized origin");
                await OnMessageSendFailed.InvokeAsync(ex);
                return false;
            }

            // Serialize message data
            var messageJson = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Validate outbound message if strict validation is enabled
            if (SecurityOptions.EnableStrictValidation)
            {
                var validationResult = validationService.ValidateMessage(
                    usesOpaqueOrigin ? "null" : validationOrigin!,
                    messageJson, 
                    usesDocumentOrigin ? new[] { validationOrigin! } : computedAllowedOrigins,
                    SecurityOptions);

                if (!validationResult.IsValid)
                {
                    var ex = new ArgumentException($"Outbound message validation failed: {validationResult.ValidationError}");
                    Logger?.LogWarning(ex, "Outbound message failed validation");
                    await OnMessageSendFailed.InvokeAsync(ex);
                    return false;
                }
            }

            // Send message via JavaScript
            var success = await module.InvokeAsync<bool>("sendMessage", iframeElement, messageJson, usesDocumentOrigin ? null : effectiveOrigin);
            
            if (success)
            {
                Logger?.LogDebug("BlazorFrame: Message sent successfully to {Origin}", effectiveOrigin);
                await OnMessageSent.InvokeAsync(messageJson);
            }
            else
            {
                var ex = new InvalidOperationException("JavaScript sendMessage returned false");
                Logger?.LogWarning(ex, "Failed to send message to iframe");
                await OnMessageSendFailed.InvokeAsync(ex);
            }

            return success;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error sending message to iframe");
            await OnMessageSendFailed.InvokeAsync(ex);
            return false;
        }
    }

    /// <summary>
    /// Sends a message to the iframe content with automatic type detection
    /// </summary>
    /// <param name="messageType">Type identifier for the message</param>
    /// <param name="data">Message payload</param>
    /// <param name="targetOrigin">Target origin for security (defaults to iframe origin)</param>
    /// <returns>True if message was sent successfully</returns>
    public async Task<bool> SendTypedMessageAsync(string messageType, object? data = null, string? targetOrigin = null)
    {
        var message = new
        {
            type = messageType,
            data = data,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        return await SendMessageAsync(message, targetOrigin);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        UpdateAllowedOrigins();
        ValidateSource();
        ValidateConfiguration();
        ValidateResizeOptions();
    }

    private void ValidateConfiguration()
    {
        try
        {
            var validationResult = SecurityOptions.ValidateConfiguration();
            
            foreach (var error in validationResult.Errors)
            {
                Logger?.LogError("BlazorFrame configuration error: {Error}", error);
            }
            
            foreach (var warning in validationResult.Warnings)
            {
                Logger?.LogWarning("BlazorFrame configuration warning: {Warning}", warning);
            }
            
            foreach (var suggestion in validationResult.Suggestions)
            {
                Logger?.LogDebug("BlazorFrame configuration suggestion: {Suggestion}", suggestion);
            }
            
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join("; ", validationResult.Errors);
                var violationMessage = new IframeMessage
                {
                    Origin = "configuration",
                    Data = $"Configuration validation failed: {errorMessage}",
                    IsValid = false,
                    ValidationError = errorMessage,
                    MessageType = "configuration-validation"
                };

                _ = InvokeAsync(async () =>
                {
                    try
                    {
                        await OnSecurityViolation.InvokeAsync(violationMessage);
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex, "Error invoking security violation callback for configuration validation");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error validating BlazorFrame configuration");
        }
    }

    private void ValidateSource()
    {
        sourceRejected = string.IsNullOrWhiteSpace(Src);
        if (string.IsNullOrWhiteSpace(Src))
            return;

        var urlValidation = validationService.ValidateUrl(Src, SecurityOptions);
        if (!urlValidation.IsValid)
        {
            sourceRejected = true;
            Logger?.LogWarning("BlazorFrame Src URL validation failed: {Error}. URL: {Src}", 
                urlValidation.ErrorMessage, Src);

            var violationMessage = new IframeMessage
            {
                Origin = "configuration",
                Data = $"URL validation failed: {urlValidation.ErrorMessage}",
                IsValid = false,
                ValidationError = urlValidation.ErrorMessage,
                MessageType = "url-validation"
            };

            _ = InvokeAsync(async () =>
            {
                try
                {
                    await OnSecurityViolation.InvokeAsync(violationMessage);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Error invoking security violation callback for URL validation");
                }
            });
        }
    }

    private void ValidateResizeOptions()
    {
        if (ResizeOptions == null)
            return;

        var errors = ResizeOptions.Validate();
        foreach (var error in errors)
        {
            Logger?.LogError("BlazorFrame ResizeOptions validation error: {Error}", error);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var configurationSignature = GetConfigurationSignature();
        if (isInitialized && string.Equals(lastConfigurationSignature, configurationSignature, StringComparison.Ordinal))
            return;

        if (isInitialized)
        {
            await CleanupJavaScriptAsync();
        }
        
        try
        {
            lock (initializationLock)
            {
                isInitialized = true;
            }

            module = await JSRuntime.InvokeAsync<IJSObjectReference>(
              "import",
              "./_content/BlazorFrame/BlazorFrame.js");
            objRef = DotNetObjectReference.Create(this);
            
            // Build resize options object to pass to JS
            object? resizeOptionsJs = null;
            if (ResizeOptions != null)
            {
                resizeOptionsJs = new
                {
                    minHeight = ResizeOptions.MinHeight,
                    maxHeight = ResizeOptions.MaxHeight,
                    pollingInterval = ResizeOptions.PollingInterval,
                    useResizeObserver = ResizeOptions.UseResizeObserver,
                    debounceMs = ResizeOptions.DebounceMs
                };
            }
            
            await module.InvokeVoidAsync(
              "initialize",
              iframeElement,
              objRef,
              EnableAutoResize,
              computedAllowedOrigins.ToArray(),
              EnableNavigationTracking,
              resizeOptionsJs,
              SecurityOptions.CapabilityToken,
              SecurityOptions.HandshakeTimeoutMs);

            lastConfigurationSignature = configurationSignature;
              
            Logger?.LogDebug("BlazorFrame initialized successfully for {Src} with navigation tracking: {NavigationEnabled}", Src, EnableNavigationTracking);
            
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to initialize BlazorFrame JavaScript module for {Src}", Src);
            
            lock (initializationLock)
            {
                isInitialized = false;
            }
            
            await OnInitializationError.InvokeAsync(ex);
        }
    }

    private async Task OnLoadHandler()
    {
        try
        {
            await OnLoad.InvokeAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "Error in OnLoad callback for BlazorFrame");
        }
    }

    [JSInvokable]
    public void OnHandshakeChallenge(string challenge)
    {
        expectedHandshakeChallenge = challenge;
        capabilityHandshakeComplete = false;
    }

    [JSInvokable]
    public async Task OnIframeMessage(string origin, string messageJson)
    {
        try
        {
            IReadOnlyList<string> allowedOrigins = UsesDocumentOrigin(Src) && AllowedOrigins is null
                ? new[] { origin }
                : computedAllowedOrigins;
            var validatedMessage = validationService.ValidateMessage(
                origin, 
                messageJson, 
                allowedOrigins,
                SecurityOptions);

            if (validatedMessage.IsValid)
            {
                if (!capabilityHandshakeComplete && !string.IsNullOrEmpty(SecurityOptions.CapabilityToken))
                {
                    if (!IsCapabilityAcknowledgement(validatedMessage))
                    {
                        await OnSecurityViolation.InvokeAsync(CreateViolation(origin, messageJson, "Capability handshake is incomplete"));
                        return;
                    }

                    capabilityHandshakeComplete = true;
                    return;
                }

                await OnMessage.InvokeAsync(messageJson);
                await OnValidatedMessage.InvokeAsync(validatedMessage);
            }
            else
            {
                if (SecurityOptions.LogSecurityViolations)
                {
                    Logger?.LogWarning(
                        "BlazorFrame security violation: {Error}. Origin: {Origin}, Message: {Message}",
                        validatedMessage.ValidationError,
                        validatedMessage.Origin,
                        validatedMessage.Data.Length > 100 
                            ? validatedMessage.Data[..100] + "..." 
                            : validatedMessage.Data);
                }

                await OnSecurityViolation.InvokeAsync(validatedMessage);
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error processing iframe message from {Origin}", origin);
        }
    }

    [JSInvokable]
    public Task Resize(double h)
    {
        try
        {
            var maxHeight = ResizeOptions?.MaxHeight ?? 50000;
            var minHeight = ResizeOptions?.MinHeight ?? 0;
            
            if (h >= minHeight && h <= maxHeight)
            {
                Height = $"{h}px";
                StateHasChanged();
            }
            else
            {
                Logger?.LogWarning("BlazorFrame received height value outside configured range: {Height} (min: {Min}, max: {Max})", h, minHeight, maxHeight);
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error resizing BlazorFrame");
        }
        
        return Task.CompletedTask;
    }

    [JSInvokable]
    public async Task OnNavigationEvent(JsonElement navigationData)
    {
        try
        {
            var navigationEvent = new NavigationEvent
            {
                Url = navigationData.GetProperty("url").GetString() ?? string.Empty,
                Pathname = navigationData.GetProperty("pathname").GetString() ?? string.Empty,
                Search = navigationData.TryGetProperty("search", out var searchProp) && searchProp.ValueKind != JsonValueKind.Null 
                    ? searchProp.GetString() 
                    : null,
                Hash = navigationData.TryGetProperty("hash", out var hashProp) && hashProp.ValueKind != JsonValueKind.Null 
                    ? hashProp.GetString() 
                    : null,
                Origin = navigationData.GetProperty("origin").GetString() ?? string.Empty,
                IsSameOrigin = navigationData.TryGetProperty("isSameOrigin", out var sameOriginProp) && sameOriginProp.GetBoolean(),
                NavigationType = navigationData.TryGetProperty("navigationType", out var navTypeProp) 
                    ? navTypeProp.GetString() ?? "unknown" 
                    : "unknown",
                QueryParameters = new Dictionary<string, string>()
            };

            // Parse query parameters
            if (navigationData.TryGetProperty("queryParameters", out var queryParams) && queryParams.ValueKind == JsonValueKind.Object)
            {
                foreach (var param in queryParams.EnumerateObject())
                {
                    navigationEvent.QueryParameters[param.Name] = param.Value.GetString() ?? string.Empty;
                }
            }

            Logger?.LogDebug("BlazorFrame navigation event: {Url} (type: {Type}, same-origin: {SameOrigin})", 
                navigationEvent.Url, navigationEvent.NavigationType, navigationEvent.IsSameOrigin);

            // Fire navigation event
            await OnNavigation.InvokeAsync(navigationEvent);
            
            // Fire simple URL changed event
            await OnUrlChanged.InvokeAsync(navigationEvent.Url);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error processing navigation event");
        }
    }

    private void UpdateAllowedOrigins()
    {
        computedAllowedOrigins.Clear();

        if (AllowedOrigins?.Count > 0)
        {
            foreach (var origin in AllowedOrigins)
            {
                if (origin.Equals("null", StringComparison.OrdinalIgnoreCase))
                    computedAllowedOrigins.Add("null");
                else if (validationService.NormalizeOrigin(origin) is { } normalizedOrigin)
                    computedAllowedOrigins.Add(normalizedOrigin);
            }
        }
        else if (Src.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && SecurityOptions.AllowDataUrls)
        {
            computedAllowedOrigins.Add("null");
        }
        else if (!string.IsNullOrEmpty(Src))
        {
            var derivedOrigin = validationService.ExtractOrigin(Src);
            if (!string.IsNullOrEmpty(derivedOrigin))
            {
                computedAllowedOrigins.Add(derivedOrigin);
            }
        }

        computedAllowedOrigins = computedAllowedOrigins
            .Where(o => !string.IsNullOrEmpty(o))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
            
        Logger?.LogDebug("BlazorFrame computed allowed origins: {Origins}", 
            string.Join(", ", computedAllowedOrigins));
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await CleanupJavaScriptAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "Error disposing BlazorFrame JavaScript module");
        }
        finally
        {
            objRef?.Dispose();
            isInitialized = false;
            lastConfigurationSignature = null;
        }
        
        GC.SuppressFinalize(this);
    }

    private async Task CleanupJavaScriptAsync()
    {
        if (module is not null)
        {
            try
            {
                await module.InvokeVoidAsync("cleanup", iframeElement);
                await module.DisposeAsync();
            }
            catch (Exception ex)
            {
                Logger?.LogDebug(ex, "BlazorFrame JavaScript cleanup failed");
            }
        }

        objRef?.Dispose();
        objRef = null;
        module = null;
        capabilityHandshakeComplete = false;
        isInitialized = false;
    }

    private string GetConfigurationSignature() => string.Join("|",
        EffectiveSrc,
        EnableAutoResize,
        EnableNavigationTracking,
        string.Join(",", computedAllowedOrigins),
        ResizeOptions?.MinHeight,
        ResizeOptions?.MaxHeight,
        ResizeOptions?.PollingInterval,
        ResizeOptions?.UseResizeObserver,
        ResizeOptions?.DebounceMs,
        SecurityOptions.CapabilityToken,
        SecurityOptions.HandshakeTimeoutMs);

    private static bool UsesDocumentOrigin(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return false;

        if (source.Equals("about:blank", StringComparison.OrdinalIgnoreCase))
            return true;

        return !Uri.TryCreate(source, UriKind.Absolute, out _)
            && !source.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            && !source.StartsWith("blob:", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsCapabilityAcknowledgement(IframeMessage message)
    {
        try
        {
            using var document = JsonDocument.Parse(message.Data);
            var root = document.RootElement;
            return root.TryGetProperty("type", out var type) &&
                   type.GetString() == "blazorframe.handshake.ack" &&
                   root.TryGetProperty("token", out var token) &&
                   string.Equals(token.GetString(), SecurityOptions.CapabilityToken, StringComparison.Ordinal) &&
                   root.TryGetProperty("challenge", out var challenge) &&
                   string.Equals(challenge.GetString(), expectedHandshakeChallenge, StringComparison.Ordinal);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static IframeMessage CreateViolation(string origin, string data, string error) => new()
    {
        Origin = origin,
        Data = data,
        IsValid = false,
        ValidationError = error,
        MessageType = "handshake"
    };
}
