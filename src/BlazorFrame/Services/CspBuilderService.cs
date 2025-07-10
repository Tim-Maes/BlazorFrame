using Microsoft.Extensions.Logging;
using System.Text;

namespace BlazorFrame.Services;

/// <summary>
/// Service for building and managing Content Security Policy headers for iframe security
/// </summary>
public class CspBuilderService
{
    private readonly ILogger<CspBuilderService>? _logger;
    private readonly MessageValidationService _validationService;

    /// <summary>
    /// Common CSP source values
    /// </summary>
    public static class Sources
    {
        public const string Self = "'self'";
        public const string None = "'none'";
        public const string UnsafeInline = "'unsafe-inline'";
        public const string UnsafeEval = "'unsafe-eval'";
        public const string StrictDynamic = "'strict-dynamic'";
        public const string Data = "data:";
        public const string Blob = "blob:";
        public const string Https = "https:";
        public const string Http = "http:";
        public const string Ws = "ws:";
        public const string Wss = "wss:";
    }

    public CspBuilderService(ILogger<CspBuilderService>? logger = null)
    {
        _logger = logger;
        _validationService = new MessageValidationService();
    }

    /// <summary>
    /// Builds a CSP header from the provided options
    /// </summary>
    /// <param name="options">CSP configuration options</param>
    /// <param name="iframeSources">Optional list of iframe sources to include</param>
    /// <returns>Built CSP header</returns>
    public CspHeader BuildCspHeader(CspOptions options, IEnumerable<string>? iframeSources = null)
    {
        var directives = new Dictionary<string, List<string>>();
        
        // Handle frame-src and child-src directives
        BuildFrameDirectives(directives, options, iframeSources);
        
        // Handle script-src directive
        BuildScriptDirectives(directives, options);
        
        // Handle frame-ancestors directive
        BuildFrameAncestorsDirectives(directives, options);
        
        // Add custom directives
        AddCustomDirectives(directives, options);
        
        // Build the CSP header value
        var headerValue = BuildHeaderValue(directives, options);
        
        var headerName = options.ReportOnly 
            ? "Content-Security-Policy-Report-Only" 
            : "Content-Security-Policy";
            
        _logger?.LogDebug("Built CSP header: {HeaderName}: {HeaderValue}", headerName, headerValue);
        
        return new CspHeader
        {
            HeaderName = headerName,
            HeaderValue = headerValue,
            IsReportOnly = options.ReportOnly,
            Directives = directives
        };
    }

    /// <summary>
    /// Creates a CSP builder with sensible defaults for BlazorFrame
    /// </summary>
    /// <returns>CspOptions with BlazorFrame-friendly defaults</returns>
    public static CspOptions CreateDefaultOptions()
    {
        return new CspOptions
        {
            AutoDeriveFrameSrc = true,
            AllowInlineScripts = false,
            AllowEval = false,
            UseStrictDynamic = false,
            ScriptSrc = new List<string> { Sources.Self },
            FrameAncestors = new List<string> { Sources.Self }
        };
    }

    /// <summary>
    /// Creates a strict CSP configuration for high-security environments
    /// </summary>
    /// <returns>Strict CSP options</returns>
    public static CspOptions CreateStrictOptions()
    {
        return new CspOptions
        {
            AutoDeriveFrameSrc = true,
            AllowInlineScripts = false,
            AllowEval = false,
            UseStrictDynamic = true,
            ScriptSrc = new List<string> { Sources.Self },
            FrameAncestors = new List<string> { Sources.Self }
        };
    }

    /// <summary>
    /// Creates a permissive CSP configuration for development environments
    /// </summary>
    /// <returns>Permissive CSP options</returns>
    public static CspOptions CreateDevelopmentOptions()
    {
        return new CspOptions
        {
            AutoDeriveFrameSrc = true,
            AllowInlineScripts = true,
            AllowEval = true,
            UseStrictDynamic = false,
            ScriptSrc = new List<string> { Sources.Self, Sources.UnsafeInline, Sources.UnsafeEval },
            FrameAncestors = new List<string> { Sources.Self }
        };
    }

    /// <summary>
    /// Validates a CSP configuration and provides recommendations
    /// </summary>
    /// <param name="options">CSP options to validate</param>
    /// <returns>Validation result with warnings and suggestions</returns>
    public CspValidationResult ValidateCspOptions(CspOptions options)
    {
        var result = new CspValidationResult { IsValid = true };
        var warnings = new List<string>();
        var errors = new List<string>();
        var suggestions = new List<string>();

        // Check for unsafe practices
        if (options.AllowInlineScripts)
        {
            warnings.Add("Using 'unsafe-inline' in script-src reduces security. Consider using nonces or strict-dynamic.");
        }

        if (options.AllowEval)
        {
            warnings.Add("Using 'unsafe-eval' in script-src can enable code injection attacks.");
        }

        // Check for missing essential directives
        if (options.FrameSrc.Count == 0 && options.ChildSrc.Count == 0 && options.AutoDeriveFrameSrc == false)
        {
            suggestions.Add("Consider adding frame-src or child-src directives to control iframe sources.");
        }

        // Check for nonce usage
        if (!string.IsNullOrEmpty(options.ScriptNonce))
        {
            if (options.AllowInlineScripts)
            {
                suggestions.Add("When using nonces, 'unsafe-inline' is not needed and reduces security.");
            }
        }

        // Check for strict-dynamic usage
        if (options.UseStrictDynamic)
        {
            if (options.AllowInlineScripts || options.AllowEval)
            {
                warnings.Add("When using 'strict-dynamic', 'unsafe-inline' and 'unsafe-eval' are ignored in modern browsers.");
            }
        }

        // Check for report-only mode
        if (options.ReportOnly && string.IsNullOrEmpty(options.ReportUri))
        {
            suggestions.Add("Consider adding a report-uri when using report-only mode to collect violation reports.");
        }

        return new CspValidationResult
        {
            IsValid = errors.Count == 0,
            Warnings = warnings,
            Errors = errors,
            Suggestions = suggestions
        };
    }

    /// <summary>
    /// Extracts and validates iframe sources from URLs
    /// </summary>
    /// <param name="urls">List of URLs to extract origins from</param>
    /// <returns>List of valid origins for CSP</returns>
    public List<string> ExtractValidOrigins(IEnumerable<string> urls)
    {
        var origins = new List<string>();
        
        foreach (var url in urls)
        {
            if (string.IsNullOrEmpty(url)) continue;
            
            var origin = _validationService.ExtractOrigin(url);
            if (!string.IsNullOrEmpty(origin) && !origins.Contains(origin))
            {
                origins.Add(origin);
            }
        }
        
        return origins;
    }

    /// <summary>
    /// Generates a CSP header string that can be used in HTML meta tags
    /// </summary>
    /// <param name="options">CSP configuration options</param>
    /// <param name="iframeSources">Optional list of iframe sources to include</param>
    /// <returns>HTML meta tag string for CSP</returns>
    public string BuildCspMetaTag(CspOptions options, IEnumerable<string>? iframeSources = null)
    {
        var cspHeader = BuildCspHeader(options, iframeSources);
        
        // CSP meta tags should only use Content-Security-Policy, not the report-only variant
        var headerName = "Content-Security-Policy";
        var headerValue = cspHeader.HeaderValue;
        
        return $"<meta http-equiv=\"{headerName}\" content=\"{headerValue}\" />";
    }

    /// <summary>
    /// Generates JavaScript code to set CSP via document.head manipulation
    /// </summary>
    /// <param name="options">CSP configuration options</param>
    /// <param name="iframeSources">Optional list of iframe sources to include</param>
    /// <returns>JavaScript code to set CSP</returns>
    public string BuildCspJavaScript(CspOptions options, IEnumerable<string>? iframeSources = null)
    {
        var cspHeader = BuildCspHeader(options, iframeSources);
        var escapedContent = cspHeader.HeaderValue.Replace("\"", "\\\"");
        
        return $@"
(function() {{
    var meta = document.createElement('meta');
    meta.setAttribute('http-equiv', 'Content-Security-Policy');
    meta.setAttribute('content', ""{escapedContent}"");
    document.head.appendChild(meta);
}})();";
    }

    private void BuildFrameDirectives(Dictionary<string, List<string>> directives, CspOptions options, IEnumerable<string>? iframeSources)
    {
        var frameSources = new List<string>();
        
        // Add explicitly configured frame sources
        frameSources.AddRange(options.FrameSrc);
        
        // Auto-derive from iframe sources if enabled
        if (options.AutoDeriveFrameSrc && iframeSources != null)
        {
            var derivedOrigins = ExtractValidOrigins(iframeSources);
            frameSources.AddRange(derivedOrigins);
        }
        
        // Remove duplicates and add to directives
        if (frameSources.Count > 0)
        {
            directives["frame-src"] = frameSources.Distinct().ToList();
        }
        
        // Handle child-src (fallback for older browsers)
        if (options.ChildSrc.Count > 0)
        {
            directives["child-src"] = options.ChildSrc.Distinct().ToList();
        }
    }

    private void BuildScriptDirectives(Dictionary<string, List<string>> directives, CspOptions options)
    {
        var scriptSources = new List<string>(options.ScriptSrc);
        
        // Add nonce if specified
        if (!string.IsNullOrEmpty(options.ScriptNonce))
        {
            scriptSources.Add($"'nonce-{options.ScriptNonce}'");
        }
        
        // Add unsafe-inline if allowed
        if (options.AllowInlineScripts)
        {
            scriptSources.Add(Sources.UnsafeInline);
        }
        
        // Add unsafe-eval if allowed
        if (options.AllowEval)
        {
            scriptSources.Add(Sources.UnsafeEval);
        }
        
        // Add strict-dynamic if enabled
        if (options.UseStrictDynamic)
        {
            scriptSources.Add(Sources.StrictDynamic);
        }
        
        if (scriptSources.Count > 0)
        {
            directives["script-src"] = scriptSources.Distinct().ToList();
        }
    }

    private void BuildFrameAncestorsDirectives(Dictionary<string, List<string>> directives, CspOptions options)
    {
        if (options.FrameAncestors.Count > 0)
        {
            directives["frame-ancestors"] = options.FrameAncestors.Distinct().ToList();
        }
    }

    private void AddCustomDirectives(Dictionary<string, List<string>> directives, CspOptions options)
    {
        foreach (var customDirective in options.CustomDirectives)
        {
            if (customDirective.Value.Count > 0)
            {
                directives[customDirective.Key] = customDirective.Value.Distinct().ToList();
            }
        }
    }

    private string BuildHeaderValue(Dictionary<string, List<string>> directives, CspOptions options)
    {
        var sb = new StringBuilder();
        
        foreach (var directive in directives)
        {
            if (sb.Length > 0)
            {
                sb.Append("; ");
            }
            
            sb.Append(directive.Key);
            
            if (directive.Value.Count > 0)
            {
                sb.Append(" ");
                sb.Append(string.Join(" ", directive.Value));
            }
        }
        
        // Add report-uri if specified
        if (!string.IsNullOrEmpty(options.ReportUri))
        {
            if (sb.Length > 0)
            {
                sb.Append("; ");
            }
            sb.Append($"report-uri {options.ReportUri}");
        }
        
        return sb.ToString();
    }
}