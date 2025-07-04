# BlazorFrame

A Blazor component that provides an enhanced iframe wrapper with automatic resizing, cross-frame communication, and seamless JavaScript interop.

## Features

- **Automatic Height Adjustment** - Dynamically resizes iframe based on content height
- **Cross-Frame Messaging** - Built-in support for postMessage communication
- **Event Callbacks** - OnLoad and OnMessage event handling
- **Flexible Styling** - Customizable width, height, and additional attributes
- **JavaScript Interop** - Seamless integration with Blazor's JS interop
- **Disposal Pattern** - Proper cleanup of resources and event listeners

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package BlazorFrame
```

Or via Package Manager Console:

```powershell
Install-Package BlazorFrame
```

## Usage

### Basic Usage

```razor
@using BlazorFrame

<BlazorFrame Src="https://example.com" />
```

### Advanced Usage with Event Handling

```razor
@using BlazorFrame

<BlazorFrame Src="@iframeUrl"
            Width="100%"
            Height="400px"
            OnLoad="HandleIframeLoad"
            OnMessage="HandleIframeMessage"
            class="my-custom-iframe" />

@code {
    private string iframeUrl = "https://example.com";

    private Task HandleIframeLoad()
    {
        Console.WriteLine("Iframe loaded successfully!");
        return Task.CompletedTask;
    }

    private Task HandleIframeMessage(string messageJson)
    {
        Console.WriteLine($"Received message: {messageJson}");
        
        // Parse and handle the message
        var message = JsonSerializer.Deserialize<dynamic>(messageJson);
        
        return Task.CompletedTask;
    }
}
```

### Custom Styling and Attributes

```razor
<BlazorFrame Src="@iframeUrl"
            Width="800px"
            Height="600px"
            class="border rounded shadow"
            style="margin: 20px;"
            sandbox="allow-scripts allow-same-origin" />
```

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Src` | `string` | `""` | The URL to load in the iframe |
| `Width` | `string` | `"100%"` | The width of the iframe |
| `Height` | `string` | `"600px"` | The initial height of the iframe |
| `OnLoad` | `EventCallback` | - | Callback fired when the iframe loads |
| `OnMessage` | `EventCallback<string>` | - | Callback fired when receiving postMessage |
| `AdditionalAttributes` | `Dictionary<string, object>` | - | Additional HTML attributes to apply |

## Automatic Resizing

BlazorFrame automatically adjusts the iframe height based on the content inside. The component:

1. Monitors the iframe content document every 500ms
2. Calculates the maximum height from various document properties
3. Updates the iframe height dynamically
4. Handles cross-origin restrictions gracefully

## Cross-Frame Communication

The component supports bidirectional communication through the browser's `postMessage` API:

### Receiving Messages from Iframe

Messages sent from the iframe content are automatically captured and forwarded to your `OnMessage` callback:

```javascript
// Inside your iframe content
window.parent.postMessage({ 
    type: 'custom', 
    data: 'Hello from iframe!' 
}, '*');
```

### Special Resize Messages

Send resize messages from iframe content to manually control height:

```javascript
// Inside your iframe content
window.parent.postMessage({ 
    type: 'resize', 
    height: 800 
}, '*');
```

## Requirements

- .NET 8.0 or later
- Blazor Server or Blazor WebAssembly

## Browser Support

BlazorFrame works in all modern browsers that support:
- ES6 modules
- postMessage API
- Blazor JavaScript interop

## Examples

### Loading Different Content Types

```razor
<!-- Web pages -->
<BlazorFrame Src="https://docs.microsoft.com" />

<!-- Local HTML files -->
<BlazorFrame Src="./local-content.html" />

<!-- Data URLs -->
<BlazorFrame Src="data:text/html,<h1>Hello World!</h1>" />
```

### Responsive Design

```razor
<div class="container-fluid">
    <BlazorFrame Src="@contentUrl"
                Width="100%"
                Height="calc(100vh - 200px)"
                style="min-height: 400px;" />
</div>
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions, please [open an issue](https://github.com/Tim-Maes/BlazorFrame/issues) on GitHub.