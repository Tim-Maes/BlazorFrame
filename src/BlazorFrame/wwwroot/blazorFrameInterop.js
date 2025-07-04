export function initialize(iframe, dotNetHelper) {
    function onMessage(event) {
        if (event.source !== iframe.contentWindow) return;
        const msg = event.data;

        dotNetHelper.invokeMethodAsync('OnIframeMessage', JSON.stringify(msg));

        if (msg?.type === 'resize' && typeof msg.height === 'number') {
            dotNetHelper.invokeMethodAsync('Resize', msg.height);
        }
    }
    window.addEventListener('message', onMessage);

    const intervalId = setInterval(() => {
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
            clearInterval(intervalId);
        }
    }, 500);
}