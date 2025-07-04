# BlazorFrame

A Blazor component that provides an enhanced iframe wrapper with automatic resizing, cross-frame communication, and seamless JavaScript interop.

## Features

- **Automatic Height Adjustment** - Dynamically resizes iframe based on content height (can be enabled/disabled)
- **Cross-Frame Messaging** - Built-in support for postMessage communication
- **Event Callbacks** - OnLoad and OnMessage event handling
- **Flexible Styling** - Customizable width, height, and additional attributes
- **JavaScript Interop** - Seamless integration with Blazor's JS interop
- **Scrolling Control** - Enable or disable scrolling within the iframe wrapper
- **Disposal Pattern** - Proper cleanup of resources and event listeners

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package BlazorFrame
```

Or via Package Manager Console:

```bash
Install-Package BlazorFrame
```

## Usage

### Basic Usage

```
@using BlazorFrame

<BlazorFrame Src="https://example.com" />
```

### Advanced Usage with Event Handling

```razor
@using BlazorFrame

<BlazorFrame Src="@iframeUrl"
            Width="100%"
            Height="400px"
            EnableAutoResize="true"
            EnableScroll="false"
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
            EnableAutoResize="false"
            EnableScroll="true"
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
| `EnableAutoResize` | `bool` | `true` | Whether to automatically resize the iframe based on content height |
| `EnableScroll` | `bool` | `false` | Whether to enable scrolling within the iframe wrapper |
| `OnLoad` | `EventCallback` | - | Callback fired when the iframe loads |
| `OnMessage` | `EventCallback<string>` | - | Callback fired when receiving postMessage |
| `AdditionalAttributes` | `Dictionary<string, object>` | - | Additional HTML attributes to apply |

## Automatic Resizing

BlazorFrame can automatically adjust the iframe height based on the content inside when `EnableAutoResize` is set to `true` (default). The component:

1. Monitors the iframe content document every 500ms
2. Calculates the maximum height from various document properties
3. Updates the iframe height dynamically
4. Handles cross-origin restrictions gracefully
5. Can be disabled by setting `EnableAutoResize="false"`

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

```javascript
Send resize messages from iframe content to manually control height:
// Inside your iframe content
window.parent.postMessage({ 
    type: 'resize', 
    height: 800 
}, '*');
```

## Styling and CSS

The component includes built-in CSS styling with wrapper classes:

- **iframe-wrapper** - Applied to all iframe wrappers
- **iframe-wrapper scrollable** - Applied when `EnableScroll="true"`

The wrapper provides:
- 100% width by default
- Hidden overflow (unless scrollable)
- Borderless iframe display

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
                EnableAutoResize="false"
                style="min-height: 400px;" />
</div>
```

### Disabling Auto-Resize for Fixed Height

```razor
<BlazorFrame Src="@contentUrl"
            Width="100%"
            Height="500px"
            EnableAutoResize="false"
            EnableScroll="true" />
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions, please [open an issue](https://github.com/Tim-Maes/BlazorFrame/issues) on GitHub.