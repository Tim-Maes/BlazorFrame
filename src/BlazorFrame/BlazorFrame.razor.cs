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

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

    private string WrapperClasses =>
      EnableScroll
        ? "iframe-wrapper scrollable"
        : "iframe-wrapper";

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        UpdateAllowedOrigins();
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
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to initialize BlazorFrame JavaScript module for {Src}", Src);
            
            // Reset initialization flag to allow retry
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
            if (h > 0 && h <= 50000) // Reasonable height limit
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