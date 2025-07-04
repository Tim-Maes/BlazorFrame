export function initialize(iframe, dotNetHelper, enableResize, allowedOrigins = []) {
    function isOriginAllowed(origin) {
        if (!allowedOrigins || allowedOrigins.length === 0) {
            return false;
        }
        return allowedOrigins.some(allowed => 
            allowed.toLowerCase() === origin.toLowerCase()
        );
    }

    function onMessage(event) {
        if (event.source !== iframe.contentWindow) {
            return;
        }

        if (!isOriginAllowed(event.origin)) {
            console.warn(`BlazorFrame: Rejected message from unauthorized origin: ${event.origin}`);
            return;
        }

        if (event.data === null || event.data === undefined) {
            console.warn('BlazorFrame: Rejected null/undefined message data');
            return;
        }

        let messageJson;
        try {
            messageJson = typeof event.data === 'string' 
                ? event.data 
                : JSON.stringify(event.data);
        } catch (error) {
            console.warn('BlazorFrame: Failed to serialize message data:', error);
            return;
        }

        dotNetHelper.invokeMethodAsync(
            'OnIframeMessage',
            event.origin,
            messageJson
        );

        if (event.data?.type === 'resize' && typeof event.data.height === 'number') {
            dotNetHelper.invokeMethodAsync('Resize', event.data.height);
        }
    }

    window.addEventListener('message', onMessage);

    if (enableResize) {
        const resizeInterval = setInterval(() => {
            try {
                const doc = iframe.contentDocument || iframe.contentWindow?.document;
                if (!doc) {
                    clearInterval(resizeInterval);
                    return;
                }

                const height = Math.max(
                    doc.documentElement?.scrollHeight || 0,
                    doc.body?.scrollHeight || 0,
                    doc.documentElement?.offsetHeight || 0,
                    doc.body?.offsetHeight || 0
                );

                if (height > 0) {
                    iframe.style.height = height + 'px';
                }
            } catch (error) {
                clearInterval(resizeInterval);
            }
        }, 500);

        return () => {
            clearInterval(resizeInterval);
            window.removeEventListener('message', onMessage);
        };
    }

    return () => {
        window.removeEventListener('message', onMessage);
    };
}