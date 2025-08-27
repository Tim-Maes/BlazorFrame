export function initialize(iframe, dotNetHelper, enableResize, allowedOrigins = [], enableNavigation = false) {
    console.log('BlazorFrame: Initializing with origins:', allowedOrigins, 'navigation:', enableNavigation);
    
    let cleanupFunctions = [];
    let lastUrl = '';
    
    function isOriginAllowed(origin) {
        if (!allowedOrigins || allowedOrigins.length === 0) {
            console.warn('BlazorFrame: No allowed origins specified');
            return false;
        }
        return allowedOrigins.some(allowed => 
            allowed.toLowerCase() === origin.toLowerCase()
        );
    }

    function parseUrl(url) {
        try {
            const urlObj = new URL(url);
            const queryParams = {};
            
            // Parse query parameters
            urlObj.searchParams.forEach((value, key) => {
                queryParams[key] = value;
            });
            
            return {
                url: url,
                pathname: urlObj.pathname,
                search: urlObj.search || null,
                hash: urlObj.hash || null,
                queryParameters: queryParams,
                origin: urlObj.origin,
                navigationType: 'unknown'
            };
        } catch (error) {
            console.warn('BlazorFrame: Failed to parse URL:', url, error);
            return null;
        }
    }

    function trackNavigation(navigationType = 'unknown') {
        if (!enableNavigation) return;
        
        try {
            const doc = iframe.contentDocument || iframe.contentWindow?.document;
            if (!doc) {
                console.debug('BlazorFrame: Cannot access iframe document for navigation tracking (likely cross-origin)');
                return;
            }
            
            const currentUrl = doc.location.href;
            if (currentUrl === lastUrl) return;
            
            console.log('BlazorFrame: Navigation detected:', currentUrl);
            lastUrl = currentUrl;
            
            const navigationData = parseUrl(currentUrl);
            if (navigationData) {
                navigationData.navigationType = navigationType;
                navigationData.isSameOrigin = isOriginAllowed(navigationData.origin);
                
                dotNetHelper.invokeMethodAsync('OnNavigationEvent', navigationData);
            }
        } catch (error) {
            // Cross-origin restrictions prevent access - this is expected
            console.debug('BlazorFrame: Navigation tracking blocked by CORS policy');
        }
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
            
            // Handle navigation events sent via postMessage
            if (event.data?.type === 'navigation' && event.data?.url) {
                const navigationData = parseUrl(event.data.url);
                if (navigationData) {
                    navigationData.navigationType = 'postmessage';
                    navigationData.isSameOrigin = isOriginAllowed(navigationData.origin);
                    dotNetHelper.invokeMethodAsync('OnNavigationEvent', navigationData);
                }
            }
        } catch (error) {
            console.error('BlazorFrame: Error invoking .NET method:', error);
        }
    }

    // Set up navigation tracking
    if (enableNavigation) {
        console.log('BlazorFrame: Navigation tracking enabled');
        
        // Track initial load
        iframe.addEventListener('load', () => {
            trackNavigation('load');
        });
        
        // Try to set up advanced navigation tracking
        try {
            iframe.addEventListener('load', () => {
                try {
                    const doc = iframe.contentDocument;
                    if (doc && doc.defaultView) {
                        const win = doc.defaultView;
                        
                        // Track history navigation
                        win.addEventListener('popstate', () => trackNavigation('popstate'));
                        win.addEventListener('hashchange', () => trackNavigation('hashchange'));
                        
                        // Track programmatic navigation
                        const originalPushState = win.history.pushState;
                        const originalReplaceState = win.history.replaceState;
                        
                        win.history.pushState = function(...args) {
                            originalPushState.apply(this, args);
                            setTimeout(() => trackNavigation('pushstate'), 0);
                        };
                        
                        win.history.replaceState = function(...args) {
                            originalReplaceState.apply(this, args);
                            setTimeout(() => trackNavigation('replacestate'), 0);
                        },
                        
                        console.log('BlazorFrame: Advanced navigation tracking set up successfully');
                    }
                } catch (error) {
                    console.debug('BlazorFrame: Advanced navigation tracking not available (cross-origin):', error.message);
                }
            });
        } catch (error) {
            console.debug('BlazorFrame: Could not set up advanced navigation tracking:', error.message);
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