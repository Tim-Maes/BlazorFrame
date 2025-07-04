using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorFrame;

public partial class BlazorFrame
{
    private ElementReference iframeElement;
    private IJSObjectReference? module;
    private DotNetObjectReference<BlazorFrame>? objRef;

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter] public string Src { get; set; } = string.Empty;
    [Parameter] public string Width { get; set; } = "100%";
    [Parameter] public string Height { get; set; } = "600px";
    [Parameter] public bool EnableAutoResize { get; set; } = true;
    [Parameter] public bool EnableScroll { get; set; } = false;

    [Parameter] public EventCallback OnLoad { get; set; }
    [Parameter] public EventCallback<string> OnMessage { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

    private string WrapperClasses =>
      EnableScroll
        ? "iframe-wrapper scrollable"
        : "iframe-wrapper";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        module = await JSRuntime.InvokeAsync<IJSObjectReference>(
          "import",
          "./_content/BlazorFrame/blazorFrameInterop.js");
        objRef = DotNetObjectReference.Create(this);
        await module.InvokeVoidAsync(
          "initialize",
          iframeElement,
          objRef,
          EnableAutoResize);
    }

    private Task OnLoadHandler() => OnLoad.InvokeAsync(null);

    [JSInvokable]
    public Task OnIframeMessage(string messageJson)
      => OnMessage.InvokeAsync(messageJson);

    [JSInvokable]
    public Task Resize(double h)
    {
        Height = $"{h}px";
        StateHasChanged();
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (module is not null) await module.DisposeAsync();
        objRef?.Dispose();
    }
}