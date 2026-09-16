export function initialize(iframe, dotNetHelper, enableResize, allowedOrigins = [], enableNavigation = false, resizeOptions = null, capabilityToken = null, handshakeTimeoutMs = 5000) {
    const sourceAttribute = iframe.getAttribute('src') || '';
    if (allowedOrigins.length === 0 && iframe.src &&
        (!/^[a-z][a-z0-9+.-]*:/i.test(sourceAttribute) || sourceAttribute.toLowerCase() === 'about:blank')) {
        allowedOrigins = [window.location.origin];
    }
    
    const defaultResizeOptions = {
        minHeight: 100,
        maxHeight: 50000,
        pollingInterval: 500,
        useResizeObserver: true,
        debounceMs: 100
    };
    
    const resizeConfig = resizeOptions ? { ...defaultResizeOptions, ...resizeOptions } : defaultResizeOptions;
    
    let cleanupFunctions = [];
    let lastUrl = '';
    let handshakeTimer = null;
    let handshakeChallenge = null;
    
    function isOriginAllowed(origin) {
        if (!allowedOrigins || allowedOrigins.length === 0) {
            return false;
        }
        return allowedOrigins.some(allowed => 
            allowed.toLowerCase() === origin.toLowerCase()
        );
    }

    function sendHandshakeChallenge() {
        if (!capabilityToken || !iframe.contentWindow) return;

        const targetOrigin = allowedOrigins.find(origin => origin !== 'null') || '*';
        const challenge = `${Date.now().toString(36)}-${crypto.randomUUID()}`;
        handshakeChallenge = challenge;
        dotNetHelper.invokeMethodAsync('OnHandshakeChallenge', challenge);
        iframe.contentWindow.postMessage({
            type: 'blazorframe.handshake.challenge',
            challenge
        }, targetOrigin);

        handshakeTimer = setTimeout(() => {
        }, handshakeTimeoutMs);
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
            return;
        }

        if (capabilityToken && event.data?.type === 'blazorframe.handshake.ack') {
            if (event.data.challenge !== handshakeChallenge || event.data.token !== capabilityToken) {
                return;
            }
            dotNetHelper.invokeMethodAsync('OnIframeMessage', event.origin, JSON.stringify(event.data));
            return;
        }

        if (event.data === null || event.data === undefined) {
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
                        };
                        
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
    iframe.addEventListener('load', sendHandshakeChallenge);
    cleanupFunctions.push(() => iframe.removeEventListener('load', sendHandshakeChallenge));
    cleanupFunctions.push(() => {
        if (handshakeTimer) {
            clearTimeout(handshakeTimer);
            handshakeTimer = null;
        }
    });
    sendHandshakeChallenge();

    if (enableResize) {
        console.log('BlazorFrame: Auto-resize enabled with options:', resizeConfig);
        
        let resizeObserver;
        let fallbackInterval;
        let lastHeight = 0;
        let debounceTimer = null;
        
        function updateHeight() {
            try {
                const doc = iframe.contentDocument || iframe.contentWindow?.document;
                if (!doc) {
                    return false;
                }

                let height = Math.max(
                    doc.documentElement?.scrollHeight || 0,
                    doc.body?.scrollHeight || 0,
                    doc.documentElement?.offsetHeight || 0,
                    doc.body?.offsetHeight || 0
                );
                
                // Apply min/max constraints
                height = Math.max(resizeConfig.minHeight, Math.min(resizeConfig.maxHeight, height));

                if (height > 0 && height !== lastHeight) {
                    iframe.style.height = height + 'px';
                    lastHeight = height;
                    dotNetHelper.invokeMethodAsync('Resize', height);
                }
                return true;
            } catch (error) {
                // Cross-origin access denied, stop trying
                return false;
            }
        }
        
        function debouncedUpdateHeight() {
            if (resizeConfig.debounceMs > 0) {
                if (debounceTimer) {
                    clearTimeout(debounceTimer);
                }
                debounceTimer = setTimeout(updateHeight, resizeConfig.debounceMs);
            } else {
                updateHeight();
            }
        }

        if (resizeConfig.useResizeObserver && window.ResizeObserver) {
            try {
                const doc = iframe.contentDocument || iframe.contentWindow?.document;
                if (doc && doc.body) {
                    resizeObserver = new ResizeObserver(() => {
                        debouncedUpdateHeight();
                    });
                    resizeObserver.observe(doc.body);
                    resizeObserver.observe(doc.documentElement);
                    console.log('BlazorFrame: Using ResizeObserver for auto-resize');
                }
            } catch (error) {
                resizeObserver = null;
            }
        }

        if (!resizeObserver) {
            console.log('BlazorFrame: Using polling fallback for auto-resize');
            fallbackInterval = setInterval(() => {
                if (!updateHeight()) {
                    clearInterval(fallbackInterval);
                }
            }, resizeConfig.pollingInterval);
        }

        cleanupFunctions.push(() => {
            if (debounceTimer) {
                clearTimeout(debounceTimer);
            }
            if (resizeObserver) {
                resizeObserver.disconnect();
            }
            if (fallbackInterval) {
                clearInterval(fallbackInterval);
            }
            if (handshakeTimer) {
                clearTimeout(handshakeTimer);
            }
        });
    }

    // Store cleanup function on the iframe element for later retrieval
    const cleanupFn = () => {
        cleanupFunctions.forEach(cleanup => {
            try {
                cleanup();
            } catch (error) {
                console.error('BlazorFrame: Error during cleanup:', error);
            }
        });
        delete iframe._blazorFrameCleanup;
        console.log('BlazorFrame: Cleanup completed');
    };
    
    iframe._blazorFrameCleanup = cleanupFn;
    
    return cleanupFn;
}

// Cleanup function that can be called from .NET
export function cleanup(iframe) {
    if (iframe && iframe._blazorFrameCleanup) {
        iframe._blazorFrameCleanup();
        return true;
    }
    return false;
}

// Reload the iframe by setting the src again
export function reload(iframe) {
    if (iframe) {
        const currentSrc = iframe.src;
        iframe.src = '';
        iframe.src = currentSrc;
        console.log('BlazorFrame: Iframe reloaded');
        return true;
    }
    return false;
}

// New function for bidirectional communication - sending messages to iframe
export function sendMessage(iframe, messageJson, targetOrigin) {
    if (!iframe || !iframe.contentWindow) {
        console.warn('BlazorFrame: Cannot send message - iframe not ready');
        return false;
    }

    const effectiveTargetOrigin = targetOrigin ?? window.location.origin;

    try {
        const messageData = JSON.parse(messageJson);
        iframe.contentWindow.postMessage(messageData, effectiveTargetOrigin);
        return true;
    } catch (error) {
        console.error('BlazorFrame: Failed to send message to iframe:', error);
        return false;
    }
}
