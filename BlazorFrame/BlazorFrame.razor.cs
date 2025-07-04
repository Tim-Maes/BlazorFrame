using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorFrame;

public partial class BlazorFrame
{
    private ElementReference iframeElement;

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter] public string Src { get; set; } = string.Empty;
    [Parameter] public string Width { get; set; } = "100%";
    [Parameter] public string Height { get; set; } = "600px";
    [Parameter] public EventCallback OnLoad { get; set; }
    [Parameter] public EventCallback<string> OnMessage { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

    private IJSObjectReference? module;
    private DotNetObjectReference<BlazorFrame>? objRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            module = await JSRuntime.InvokeAsync<IJSObjectReference>(
              "import",
              "./_content/BlazorFrame/blazorFrameInterop.js");

            objRef = DotNetObjectReference.Create(this);
            await module.InvokeVoidAsync("initialize", iframeElement, objRef);
        }
    }

    private Task OnLoadHandler() => OnLoad.InvokeAsync(null);

    [JSInvokable]
    public Task OnIframeMessage(string messageJson)
      => OnMessage.InvokeAsync(messageJson);

    [JSInvokable]
    public Task Resize(double height)
    {
        Height = $"{height}px";
        StateHasChanged();
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
            await module.DisposeAsync();
        objRef?.Dispose();
    }
}