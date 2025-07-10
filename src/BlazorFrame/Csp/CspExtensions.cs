using BlazorFrame.Services;

namespace BlazorFrame;

/// <summary>
/// Extension methods for easier CSP configuration
/// </summary>
public static class CspExtensions
{
    /// <summary>
    /// Adds a source to the frame-src directive
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <param name="source">Source to add (e.g., 'https://example.com' or 'self')</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions AllowFrameSource(this CspOptions options, string source)
    {
        if (!string.IsNullOrEmpty(source) && !options.FrameSrc.Contains(source))
        {
            options.FrameSrc.Add(source);
        }
        return options;
    }

    /// <summary>
    /// Adds multiple sources to the frame-src directive
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <param name="sources">Sources to add</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions AllowFrameSources(this CspOptions options, params string[] sources)
    {
        foreach (var source in sources)
        {
            options.AllowFrameSource(source);
        }
        return options;
    }

    /// <summary>
    /// Adds a source to the script-src directive
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <param name="source">Source to add</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions AllowScriptSource(this CspOptions options, string source)
    {
        if (!string.IsNullOrEmpty(source) && !options.ScriptSrc.Contains(source))
        {
            options.ScriptSrc.Add(source);
        }
        return options;
    }

    /// <summary>
    /// Adds multiple sources to the script-src directive
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <param name="sources">Sources to add</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions AllowScriptSources(this CspOptions options, params string[] sources)
    {
        foreach (var source in sources)
        {
            options.AllowScriptSource(source);
        }
        return options;
    }

    /// <summary>
    /// Adds a source to the frame-ancestors directive
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <param name="source">Source to add</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions AllowFrameAncestor(this CspOptions options, string source)
    {
        if (!string.IsNullOrEmpty(source) && !options.FrameAncestors.Contains(source))
        {
            options.FrameAncestors.Add(source);
        }
        return options;
    }

    /// <summary>
    /// Adds multiple sources to the frame-ancestors directive
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <param name="sources">Sources to add</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions AllowFrameAncestors(this CspOptions options, params string[] sources)
    {
        foreach (var source in sources)
        {
            options.AllowFrameAncestor(source);
        }
        return options;
    }

    /// <summary>
    /// Configures CSP to allow self as a source for all relevant directives
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions AllowSelf(this CspOptions options)
    {
        return options
            .AllowFrameSource(CspBuilderService.Sources.Self)
            .AllowScriptSource(CspBuilderService.Sources.Self)
            .AllowFrameAncestor(CspBuilderService.Sources.Self);
    }

    /// <summary>
    /// Configures CSP to allow data: URLs for frames
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions AllowDataUrls(this CspOptions options)
    {
        return options.AllowFrameSource(CspBuilderService.Sources.Data);
    }

    /// <summary>
    /// Configures CSP to allow blob: URLs for frames
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions AllowBlobUrls(this CspOptions options)
    {
        return options.AllowFrameSource(CspBuilderService.Sources.Blob);
    }

    /// <summary>
    /// Configures CSP to allow HTTPS sources for frames
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions AllowHttpsFrames(this CspOptions options)
    {
        return options.AllowFrameSource(CspBuilderService.Sources.Https);
    }

    /// <summary>
    /// Sets the script nonce for inline scripts
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <param name="nonce">Nonce value</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions WithScriptNonce(this CspOptions options, string nonce)
    {
        options.ScriptNonce = nonce;
        return options;
    }

    /// <summary>
    /// Enables strict-dynamic for script sources
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions UseStrictDynamic(this CspOptions options)
    {
        options.UseStrictDynamic = true;
        return options;
    }

    /// <summary>
    /// Enables report-only mode
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <param name="reportUri">Optional URI to send violation reports to</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions AsReportOnly(this CspOptions options, string? reportUri = null)
    {
        options.ReportOnly = true;
        if (!string.IsNullOrEmpty(reportUri))
        {
            options.ReportUri = reportUri;
        }
        return options;
    }

    /// <summary>
    /// Adds a custom directive to the CSP
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <param name="directive">Directive name</param>
    /// <param name="sources">Sources for the directive</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions WithCustomDirective(this CspOptions options, string directive, params string[] sources)
    {
        if (!options.CustomDirectives.ContainsKey(directive))
        {
            options.CustomDirectives[directive] = new List<string>();
        }
        
        foreach (var source in sources)
        {
            if (!string.IsNullOrEmpty(source) && !options.CustomDirectives[directive].Contains(source))
            {
                options.CustomDirectives[directive].Add(source);
            }
        }
        
        return options;
    }

    /// <summary>
    /// Configures CSP for common iframe hosting scenarios
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <param name="allowedDomains">Domains that are allowed to host iframes</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions ForIframeHosting(this CspOptions options, params string[] allowedDomains)
    {
        options.AllowSelf();
        
        foreach (var domain in allowedDomains)
        {
            if (!string.IsNullOrEmpty(domain))
            {
                options.AllowFrameSource(domain);
            }
        }
        
        return options;
    }

    /// <summary>
    /// Configures CSP for development environments with relaxed security
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions ForDevelopment(this CspOptions options)
    {
        options.AllowInlineScripts = true;
        options.AllowEval = true;
        options.AllowSelf();
        options.AllowDataUrls();
        options.AllowBlobUrls();
        
        return options;
    }

    /// <summary>
    /// Configures CSP for production environments with strict security
    /// </summary>
    /// <param name="options">CSP options to modify</param>
    /// <returns>Modified CSP options for chaining</returns>
    public static CspOptions ForProduction(this CspOptions options)
    {
        options.AllowInlineScripts = false;
        options.AllowEval = false;
        options.UseStrictDynamic = true;
        options.AllowSelf();
        
        return options;
    }
}