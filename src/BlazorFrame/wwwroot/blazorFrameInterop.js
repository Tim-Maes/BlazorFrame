export function initialize(iframe, dotNetHelper, enableResize) {
    function onMessage(event) {
        if (event.source !== iframe.contentWindow) return;
        dotNetHelper.invokeMethodAsync(
            'OnIframeMessage',
            JSON.stringify(event.data));
        if (event.data?.type === 'resize' && typeof event.data.height === 'number') {
            dotNetHelper.invokeMethodAsync('Resize', event.data.height);
        }
    }
    window.addEventListener('message', onMessage);

    if (enableResize) {
        const id = setInterval(() => {
            try {
                const doc = iframe.contentDocument || iframe.contentWindow.document;
                const h = Math.max(
                    doc.documentElement.scrollHeight,
                    doc.body.scrollHeight,
                    doc.documentElement.offsetHeight,
                    doc.body.offsetHeight
                );
                iframe.style.height = h + 'px';
            } catch {
                clearInterval(id);
            }
        }, 500);
    }
}