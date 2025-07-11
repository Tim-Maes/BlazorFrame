using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using BlazorFrame.Services;

namespace BlazorFrame;

public partial class BlazorFrame : IAsyncDisposable
{
    private ElementReference iframeElement;
    private IJSObjectReference? module;
    private DotNetObjectReference<BlazorFrame>? objRef;
    private readonly MessageValidationService validationService = new();
    private readonly CspBuilderService cspBuilderService = new();
    private List<string> computedAllowedOrigins = new();
    private bool isInitialized = false;
    private readonly object initializationLock = new();

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<BlazorFrame>? Logger { get; set; }

    [Parameter] public string Src { get; set; } = string.Empty;
    [Parameter] public string Width { get; set; } = "100%";
    [Parameter] public string Height { get; set; } = "600px";
    [Parameter] public bool EnableAutoResize { get; set; } = true;
    [Parameter] public bool EnableScroll { get; set; } = false;

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
    /// Content Security Policy options for iframe security.
    /// When provided, CSP header recommendations will be generated.
    /// </summary>
    [Parameter] public CspOptions? CspOptions { get; set; }

    [Parameter] public EventCallback OnLoad { get; set; }
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
    /// Event fired when CSP header is generated, allowing the application to apply it
    /// </summary>
    [Parameter] public EventCallback<CspHeader> OnCspHeaderGenerated { get; set; }

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
    /// Gets the recommended CSP header for the current configuration
    /// </summary>
    /// <returns>CSP header or null if CSP is not configured</returns>
    public CspHeader? GetRecommendedCspHeader()
    {
        if (CspOptions == null) return null;
        
        var iframeSources = new List<string>();
        if (!string.IsNullOrEmpty(Src))
        {
            iframeSources.Add(Src);
        }
        
        return cspBuilderService.BuildCspHeader(CspOptions, iframeSources);
    }

    /// <summary>
    /// Validates the current CSP configuration
    /// </summary>
    /// <returns>CSP validation result or null if CSP is not configured</returns>
    public CspValidationResult? ValidateCspConfiguration()
    {
        if (CspOptions == null) return null;
        
        return cspBuilderService.ValidateCspOptions(CspOptions);
    }

    /// <summary>
    /// Gets CSP helper methods for creating common configurations
    /// </summary>
    /// <returns>CSP builder service for advanced configuration</returns>
    public CspBuilderService GetCspBuilder() => cspBuilderService;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        UpdateAllowedOrigins();
        ValidateSrcUrl();
    }

    private void ValidateSrcUrl()
    {
        if (string.IsNullOrEmpty(Src))
            return;

        var urlValidation = validationService.ValidateUrl(Src, SecurityOptions);
        if (!urlValidation.IsValid)
        {
            Logger?.LogWarning("BlazorFrame Src URL validation failed: {Error}. URL: {Src}", 
                urlValidation.ErrorMessage, Src);

            // Create a security violation for URL validation failure
            var violationMessage = new IframeMessage
            {
                Origin = Src,
                Data = $"URL validation failed: {urlValidation.ErrorMessage}",
                IsValid = false,
                ValidationError = urlValidation.ErrorMessage,
                MessageType = "url-validation"
            };

            // Fire security violation event asynchronously
            _ = Task.Run(async () =>
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || isInitialized) return;
        
        lock (initializationLock)
        {
            if (isInitialized) return;
            isInitialized = true;
        }
        
        try
        {
            module = await JSRuntime.InvokeAsync<IJSObjectReference>(
              "import",
              "/_content/BlazorFrame/BlazorFrame.js");
            objRef = DotNetObjectReference.Create(this);
            
            await module.InvokeVoidAsync(
              "initialize",
              iframeElement,
              objRef,
              EnableAutoResize,
              computedAllowedOrigins.ToArray());
              
            Logger?.LogDebug("BlazorFrame initialized successfully for {Src}", Src);
            
            // Generate and fire CSP header event if configured
            await HandleCspHeaderGeneration();
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

    private async Task HandleCspHeaderGeneration()
    {
        try
        {
            var cspHeader = GetRecommendedCspHeader();
            if (cspHeader != null)
            {
                Logger?.LogDebug("Generated CSP header for BlazorFrame: {Header}", cspHeader.HeaderValue);
                
                // Validate CSP configuration and log warnings
                var validationResult = ValidateCspConfiguration();
                if (validationResult != null)
                {
                    foreach (var warning in validationResult.Warnings)
                    {
                        Logger?.LogWarning("CSP Warning: {Warning}", warning);
                    }
                    
                    foreach (var suggestion in validationResult.Suggestions)
                    {
                        Logger?.LogInformation("CSP Suggestion: {Suggestion}", suggestion);
                    }
                }
                
                await OnCspHeaderGenerated.InvokeAsync(cspHeader);
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error generating CSP header for BlazorFrame");
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
    public async Task OnIframeMessage(string origin, string messageJson)
    {
        try
        {
            var validatedMessage = validationService.ValidateMessage(
                origin, 
                messageJson, 
                computedAllowedOrigins, 
                SecurityOptions);

            if (validatedMessage.IsValid)
            {
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
            if (h > 0 && h <= 50000)
            {
                Height = $"{h}px";
                StateHasChanged();
            }
            else
            {
                Logger?.LogWarning("BlazorFrame received invalid height value: {Height}", h);
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error resizing BlazorFrame");
        }
        
        return Task.CompletedTask;
    }

    private void UpdateAllowedOrigins()
    {
        computedAllowedOrigins.Clear();

        if (AllowedOrigins?.Count > 0)
        {
            computedAllowedOrigins.AddRange(AllowedOrigins);
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
            if (module is not null) 
                await module.DisposeAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "Error disposing BlazorFrame JavaScript module");
        }
        finally
        {
            objRef?.Dispose();
            isInitialized = false;
        }
    }
}