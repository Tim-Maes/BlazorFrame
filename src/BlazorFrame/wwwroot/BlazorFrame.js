export function initialize(iframe, dotNetHelper, enableResize, allowedOrigins = []) {
    console.log('BlazorFrame: Initializing with origins:', allowedOrigins);
    
    let cleanupFunctions = [];
    
    function isOriginAllowed(origin) {
        if (!allowedOrigins || allowedOrigins.length === 0) {
            console.warn('BlazorFrame: No allowed origins specified');
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

        console.log('BlazorFrame: Processing valid message from', event.origin);
        
        try {
            dotNetHelper.invokeMethodAsync(
                'OnIframeMessage',
                event.origin,
                messageJson
            );

            if (event.data?.type === 'resize' && typeof event.data.height === 'number') {
                dotNetHelper.invokeMethodAsync('Resize', event.data.height);
            }
        } catch (error) {
            console.error('BlazorFrame: Error invoking .NET method:', error);
        }
    }

    window.addEventListener('message', onMessage);
    cleanupFunctions.push(() => window.removeEventListener('message', onMessage));
    console.log('BlazorFrame: Message listener added');

    if (enableResize) {
        console.log('BlazorFrame: Auto-resize enabled');
        
        let resizeObserver;
        let fallbackInterval;
        let lastHeight = 0;
        
        function updateHeight() {
            try {
                const doc = iframe.contentDocument || iframe.contentWindow?.document;
                if (!doc) {
                    return false;
                }

                const height = Math.max(
                    doc.documentElement?.scrollHeight || 0,
                    doc.body?.scrollHeight || 0,
                    doc.documentElement?.offsetHeight || 0,
                    doc.body?.offsetHeight || 0
                );

                if (height > 0 && height !== lastHeight) {
                    iframe.style.height = height + 'px';
                    lastHeight = height;
                }
                return true;
            } catch (error) {
                // Cross-origin access denied, stop trying
                return false;
            }
        }

        if (window.ResizeObserver) {
            try {
                const doc = iframe.contentDocument || iframe.contentWindow?.document;
                if (doc && doc.body) {
                    resizeObserver = new ResizeObserver(() => {
                        updateHeight();
                    });
                    resizeObserver.observe(doc.body);
                    resizeObserver.observe(doc.documentElement);
                }
            } catch (error) {
                resizeObserver = null;
            }
        }

        if (!resizeObserver) {
            fallbackInterval = setInterval(() => {
                if (!updateHeight()) {
                    clearInterval(fallbackInterval);
                }
            }, 500);
        }

        cleanupFunctions.push(() => {
            if (resizeObserver) {
                resizeObserver.disconnect();
            }
            if (fallbackInterval) {
                clearInterval(fallbackInterval);
            }
        });
    }

    return () => {
        cleanupFunctions.forEach(cleanup => cleanup());
        console.log('BlazorFrame: Cleanup completed');
    };
}

// New function for bidirectional communication - sending messages to iframe
export function sendMessage(iframe, messageJson, targetOrigin) {
    if (!iframe || !iframe.contentWindow) {
        console.warn('BlazorFrame: Cannot send message - iframe not ready');
        return false;
    }

    if (!targetOrigin) {
        console.warn('BlazorFrame: Cannot send message - target origin not specified');
        return false;
    }

    try {
        const messageData = JSON.parse(messageJson);
        iframe.contentWindow.postMessage(messageData, targetOrigin);
        console.log('BlazorFrame: Message sent to iframe:', targetOrigin);
        return true;
    } catch (error) {
        console.error('BlazorFrame: Failed to send message to iframe:', error);
        return false;
    }
}