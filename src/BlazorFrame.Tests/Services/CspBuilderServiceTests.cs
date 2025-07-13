using BlazorFrame.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BlazorFrame.Tests.Services;

public class CspBuilderServiceTests
{
    private readonly CspBuilderService _cspBuilderService;
    private readonly Mock<ILogger<CspBuilderService>> _mockLogger;

    public CspBuilderServiceTests()
    {
        _mockLogger = new Mock<ILogger<CspBuilderService>>();
        _cspBuilderService = new CspBuilderService(_mockLogger.Object);
    }

    #region BuildCspHeader Tests

    [Fact]
    public void BuildCspHeader_WithBasicOptions_ReturnsValidHeader()
    {
        // Arrange
        var options = new CspOptions
        {
            FrameSrc = new List<string> { "'self'", "https://example.com" },
            ScriptSrc = new List<string> { "'self'" }
        };

        // Act
        var result = _cspBuilderService.BuildCspHeader(options);

        // Assert
        result.Should().NotBeNull();
        result.HeaderName.Should().Be("Content-Security-Policy");
        result.HeaderValue.Should().Contain("frame-src 'self' https://example.com");
        result.HeaderValue.Should().Contain("script-src 'self'");
        result.IsReportOnly.Should().BeFalse();
        result.Directives.Should().ContainKey("frame-src");
        result.Directives.Should().ContainKey("script-src");
    }

    [Fact]
    public void BuildCspHeader_WithReportOnlyMode_ReturnsReportOnlyHeader()
    {
        // Arrange
        var options = new CspOptions
        {
            FrameSrc = new List<string> { "'self'" },
            ReportOnly = true
        };

        // Act
        var result = _cspBuilderService.BuildCspHeader(options);

        // Assert
        result.HeaderName.Should().Be("Content-Security-Policy-Report-Only");
        result.IsReportOnly.Should().BeTrue();
    }

    [Fact]
    public void BuildCspHeader_WithAutoDeriveFrameSrc_IncludesIframeSources()
    {
        // Arrange
        var options = new CspOptions
        {
            AutoDeriveFrameSrc = true,
            FrameSrc = new List<string> { "'self'" }
        };
        var iframeSources = new List<string> { "https://widget.example.com", "https://api.example.com" };

        // Act
        var result = _cspBuilderService.BuildCspHeader(options, iframeSources);

        // Assert
        result.HeaderValue.Should().Contain("frame-src 'self' https://widget.example.com https://api.example.com");
        result.Directives["frame-src"].Should().Contain("'self'");
        result.Directives["frame-src"].Should().Contain("https://widget.example.com");
        result.Directives["frame-src"].Should().Contain("https://api.example.com");
    }

    [Fact]
    public void BuildCspHeader_WithDuplicateFrameSources_RemovesDuplicates()
    {
        // Arrange
        var options = new CspOptions
        {
            AutoDeriveFrameSrc = true,
            FrameSrc = new List<string> { "'self'", "https://example.com" }
        };
        var iframeSources = new List<string> { "https://example.com", "https://example.com" };

        // Act
        var result = _cspBuilderService.BuildCspHeader(options, iframeSources);

        // Assert
        result.Directives["frame-src"].Count(s => s == "https://example.com").Should().Be(1);
    }

    [Fact]
    public void BuildCspHeader_WithScriptNonce_IncludesNonceInScriptSrc()
    {
        // Arrange
        var options = new CspOptions
        {
            ScriptSrc = new List<string> { "'self'" },
            ScriptNonce = "abc123"
        };

        // Act
        var result = _cspBuilderService.BuildCspHeader(options);

        // Assert
        result.HeaderValue.Should().Contain("script-src 'self' 'nonce-abc123'");
        result.Directives["script-src"].Should().Contain("'nonce-abc123'");
    }

    [Fact]
    public void BuildCspHeader_WithAllowInlineScripts_IncludesUnsafeInline()
    {
        // Arrange
        var options = new CspOptions
        {
            ScriptSrc = new List<string> { "'self'" },
            AllowInlineScripts = true
        };

        // Act
        var result = _cspBuilderService.BuildCspHeader(options);

        // Assert
        result.HeaderValue.Should().Contain("'unsafe-inline'");
        result.Directives["script-src"].Should().Contain("'unsafe-inline'");
    }

    [Fact]
    public void BuildCspHeader_WithAllowEval_IncludesUnsafeEval()
    {
        // Arrange
        var options = new CspOptions
        {
            ScriptSrc = new List<string> { "'self'" },
            AllowEval = true
        };

        // Act
        var result = _cspBuilderService.BuildCspHeader(options);

        // Assert
        result.HeaderValue.Should().Contain("'unsafe-eval'");
        result.Directives["script-src"].Should().Contain("'unsafe-eval'");
    }

    [Fact]
    public void BuildCspHeader_WithStrictDynamic_IncludesStrictDynamic()
    {
        // Arrange
        var options = new CspOptions
        {
            ScriptSrc = new List<string> { "'self'" },
            UseStrictDynamic = true
        };

        // Act
        var result = _cspBuilderService.BuildCspHeader(options);

        // Assert
        result.HeaderValue.Should().Contain("'strict-dynamic'");
        result.Directives["script-src"].Should().Contain("'strict-dynamic'");
    }

    [Fact]
    public void BuildCspHeader_WithFrameAncestors_IncludesFrameAncestorsDirective()
    {
        // Arrange
        var options = new CspOptions
        {
            FrameAncestors = new List<string> { "'self'", "https://parent.example.com" }
        };

        // Act
        var result = _cspBuilderService.BuildCspHeader(options);

        // Assert
        result.HeaderValue.Should().Contain("frame-ancestors 'self' https://parent.example.com");
        result.Directives["frame-ancestors"].Should().Contain("'self'");
        result.Directives["frame-ancestors"].Should().Contain("https://parent.example.com");
    }

    [Fact]
    public void BuildCspHeader_WithCustomDirectives_IncludesCustomDirectives()
    {
        // Arrange
        var options = new CspOptions
        {
            CustomDirectives = new Dictionary<string, List<string>>
            {
                ["img-src"] = new List<string> { "'self'", "data:", "https:" },
                ["connect-src"] = new List<string> { "'self'", "https://api.example.com" }
            }
        };

        // Act
        var result = _cspBuilderService.BuildCspHeader(options);

        // Assert
        result.HeaderValue.Should().Contain("img-src 'self' data: https:");
        result.HeaderValue.Should().Contain("connect-src 'self' https://api.example.com");
        result.Directives.Should().ContainKey("img-src");
        result.Directives.Should().ContainKey("connect-src");
    }

    [Fact]
    public void BuildCspHeader_WithReportUri_IncludesReportUriDirective()
    {
        // Arrange
        var options = new CspOptions
        {
            FrameSrc = new List<string> { "'self'" },
            ReportUri = "https://csp-report.example.com/report"
        };

        // Act
        var result = _cspBuilderService.BuildCspHeader(options);

        // Assert
        result.HeaderValue.Should().Contain("report-uri https://csp-report.example.com/report");
    }

    [Fact]
    public void BuildCspHeader_WithChildSrc_IncludesChildSrcDirective()
    {
        // Arrange
        var options = new CspOptions
        {
            ChildSrc = new List<string> { "'self'", "https://legacy.example.com" }
        };

        // Act
        var result = _cspBuilderService.BuildCspHeader(options);

        // Assert
        result.HeaderValue.Should().Contain("child-src 'self' https://legacy.example.com");
        result.Directives["child-src"].Should().Contain("'self'");
        result.Directives["child-src"].Should().Contain("https://legacy.example.com");
    }

    [Fact]
    public void BuildCspHeader_WithEmptyOptions_ReturnsEmptyHeader()
    {
        // Arrange
        var options = new CspOptions();

        // Act
        var result = _cspBuilderService.BuildCspHeader(options);

        // Assert
        result.HeaderValue.Should().BeEmpty();
        result.Directives.Should().BeEmpty();
    }

    #endregion

    #region Static Factory Methods Tests

    [Fact]
    public void CreateDefaultOptions_ReturnsValidDefaultConfiguration()
    {
        // Arrange
        // Act
        var result = CspBuilderService.CreateDefaultOptions();

        // Assert
        result.Should().NotBeNull();
        result.AutoDeriveFrameSrc.Should().BeTrue();
        result.AllowInlineScripts.Should().BeFalse();
        result.AllowEval.Should().BeFalse();
        result.UseStrictDynamic.Should().BeFalse();
        result.ScriptSrc.Should().Contain(CspBuilderService.Sources.Self);
        result.FrameAncestors.Should().Contain(CspBuilderService.Sources.Self);
    }

    [Fact]
    public void CreateStrictOptions_ReturnsStrictSecurityConfiguration()
    {
        // Arrange
        // Act
        var result = CspBuilderService.CreateStrictOptions();

        // Assert
        result.Should().NotBeNull();
        result.AutoDeriveFrameSrc.Should().BeTrue();
        result.AllowInlineScripts.Should().BeFalse();
        result.AllowEval.Should().BeFalse();
        result.UseStrictDynamic.Should().BeTrue();
        result.ScriptSrc.Should().Contain(CspBuilderService.Sources.Self);
        result.FrameAncestors.Should().Contain(CspBuilderService.Sources.Self);
    }

    [Fact]
    public void CreateDevelopmentOptions_ReturnsPermissiveConfiguration()
    {
        // Arrange
        // Act
        var result = CspBuilderService.CreateDevelopmentOptions();

        // Assert
        result.Should().NotBeNull();
        result.AutoDeriveFrameSrc.Should().BeTrue();
        result.AllowInlineScripts.Should().BeTrue();
        result.AllowEval.Should().BeTrue();
        result.UseStrictDynamic.Should().BeFalse();
        result.ScriptSrc.Should().Contain(CspBuilderService.Sources.Self);
        result.ScriptSrc.Should().Contain(CspBuilderService.Sources.UnsafeInline);
        result.ScriptSrc.Should().Contain(CspBuilderService.Sources.UnsafeEval);
        result.FrameAncestors.Should().Contain(CspBuilderService.Sources.Self);
    }

    #endregion

    #region ValidateCspOptions Tests

    [Fact]
    public void ValidateCspOptions_WithSecureConfiguration_ReturnsValid()
    {
        // Arrange
        var options = new CspOptions
        {
            FrameSrc = new List<string> { "'self'", "https://example.com" },
            ScriptSrc = new List<string> { "'self'" },
            AllowInlineScripts = false,
            AllowEval = false
        };

        // Act
        var result = _cspBuilderService.ValidateCspOptions(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateCspOptions_WithUnsafeInline_ReturnsWarning()
    {
        // Arrange
        var options = new CspOptions
        {
            ScriptSrc = new List<string> { "'self'" },
            AllowInlineScripts = true
        };

        // Act
        var result = _cspBuilderService.ValidateCspOptions(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Warnings.Should().Contain(w => w.Contains("'unsafe-inline'"));
    }

    [Fact]
    public void ValidateCspOptions_WithUnsafeEval_ReturnsWarning()
    {
        // Arrange
        var options = new CspOptions
        {
            ScriptSrc = new List<string> { "'self'" },
            AllowEval = true
        };

        // Act
        var result = _cspBuilderService.ValidateCspOptions(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Warnings.Should().Contain(w => w.Contains("'unsafe-eval'"));
    }

    [Fact]
    public void ValidateCspOptions_WithNoFrameSourcesAndAutoDeriveDisabled_ReturnsSuggestion()
    {
        // Arrange
        var options = new CspOptions
        {
            AutoDeriveFrameSrc = false,
            FrameSrc = new List<string>(),
            ChildSrc = new List<string>()
        };

        // Act
        var result = _cspBuilderService.ValidateCspOptions(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Suggestions.Should().Contain(s => s.Contains("frame-src or child-src"));
    }

    [Fact]
    public void ValidateCspOptions_WithNonceAndUnsafeInline_ReturnsSuggestion()
    {
        // Arrange
        var options = new CspOptions
        {
            ScriptSrc = new List<string> { "'self'" },
            ScriptNonce = "abc123",
            AllowInlineScripts = true
        };

        // Act
        var result = _cspBuilderService.ValidateCspOptions(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Suggestions.Should().Contain(s => s.Contains("nonces") && s.Contains("'unsafe-inline'"));
    }

    [Fact]
    public void ValidateCspOptions_WithStrictDynamicAndUnsafeInline_ReturnsWarning()
    {
        // Arrange
        var options = new CspOptions
        {
            ScriptSrc = new List<string> { "'self'" },
            UseStrictDynamic = true,
            AllowInlineScripts = true
        };

        // Act
        var result = _cspBuilderService.ValidateCspOptions(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Warnings.Should().Contain(w => w.Contains("'strict-dynamic'"));
    }

    [Fact]
    public void ValidateCspOptions_WithStrictDynamicAndUnsafeEval_ReturnsWarning()
    {
        // Arrange
        var options = new CspOptions
        {
            ScriptSrc = new List<string> { "'self'" },
            UseStrictDynamic = true,
            AllowEval = true
        };

        // Act
        var result = _cspBuilderService.ValidateCspOptions(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Warnings.Should().Contain(w => w.Contains("'strict-dynamic'"));
    }

    [Fact]
    public void ValidateCspOptions_WithReportOnlyAndNoReportUri_ReturnsSuggestion()
    {
        // Arrange
        var options = new CspOptions
        {
            ReportOnly = true,
            ReportUri = null
        };

        // Act
        var result = _cspBuilderService.ValidateCspOptions(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Suggestions.Should().Contain(s => s.Contains("report-uri"));
    }

    #endregion

    #region ExtractValidOrigins Tests

    [Theory]
    [InlineData("https://example.com", "https://example.com")]
    [InlineData("https://example.com:8080", "https://example.com:8080")]
    [InlineData("http://localhost:3000", "http://localhost:3000")]
    public void ExtractValidOrigins_WithValidUrls_ReturnsCorrectOrigins(string url, string expectedOrigin)
    {
        // Arrange
        var urls = new List<string> { url };

        // Act
        var result = _cspBuilderService.ExtractValidOrigins(urls);

        // Assert
        result.Should().Contain(expectedOrigin);
    }

    [Fact]
    public void ExtractValidOrigins_WithMultipleUrls_ReturnsDedupedOrigins()
    {
        // Arrange
        var urls = new List<string>
        {
            "https://example.com/page1",
            "https://example.com/page2",
            "https://other.com",
            "https://example.com/page3"
        };

        // Act
        var result = _cspBuilderService.ExtractValidOrigins(urls);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("https://example.com");
        result.Should().Contain("https://other.com");
    }

    [Fact]
    public void ExtractValidOrigins_WithInvalidUrls_SkipsInvalidUrls()
    {
        // Arrange
        var urls = new List<string>
        {
            "https://valid.com",
            "invalid-url",
            "",
            null!,
            "https://another-valid.com"
        };

        // Act
        var result = _cspBuilderService.ExtractValidOrigins(urls);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("https://valid.com");
        result.Should().Contain("https://another-valid.com");
    }

    [Fact]
    public void ExtractValidOrigins_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var urls = new List<string>();

        // Act
        var result = _cspBuilderService.ExtractValidOrigins(urls);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region BuildCspMetaTag Tests

    [Fact]
    public void BuildCspMetaTag_WithValidOptions_ReturnsCorrectMetaTag()
    {
        // Arrange
        var options = new CspOptions
        {
            FrameSrc = new List<string> { "'self'", "https://example.com" },
            ScriptSrc = new List<string> { "'self'" }
        };

        // Act
        var result = _cspBuilderService.BuildCspMetaTag(options);

        // Assert
        result.Should().StartWith("<meta http-equiv=\"Content-Security-Policy\"");
        result.Should().Contain("frame-src 'self' https://example.com");
        result.Should().Contain("script-src 'self'");
        result.Should().EndWith(" />");
    }

    [Fact]
    public void BuildCspMetaTag_WithReportOnlyOptions_UsesStandardCspHeader()
    {
        // Arrange
        var options = new CspOptions
        {
            FrameSrc = new List<string> { "'self'" },
            ReportOnly = true
        };

        // Act
        var result = _cspBuilderService.BuildCspMetaTag(options);

        // Assert
        result.Should().Contain("http-equiv=\"Content-Security-Policy\"");
        result.Should().NotContain("Report-Only");
    }

    #endregion

    #region BuildCspJavaScript Tests

    [Fact]
    public void BuildCspJavaScript_WithValidOptions_ReturnsCorrectJavaScript()
    {
        // Arrange
        var options = new CspOptions
        {
            FrameSrc = new List<string> { "'self'", "https://example.com" }
        };

        // Act
        var result = _cspBuilderService.BuildCspJavaScript(options);

        // Assert
        result.Should().Contain("document.createElement('meta')");
        result.Should().Contain("setAttribute('http-equiv', 'Content-Security-Policy')");
        result.Should().Contain("frame-src 'self' https://example.com");
        result.Should().Contain("document.head.appendChild(meta)");
    }

    [Fact]
    public void BuildCspJavaScript_WithQuotesInContent_EscapesQuotesProperly()
    {
        // Arrange
        var options = new CspOptions
        {
            ScriptSrc = new List<string> { "'self'", "'nonce-abc\"123'" }
        };

        // Act
        var result = _cspBuilderService.BuildCspJavaScript(options);

        // Assert
        result.Should().Contain("'nonce-abc\\\"123'");
        result.Should().NotContain("'nonce-abc\"123'");
    }

    #endregion

    #region Sources Constants Tests

    [Fact]
    public void Sources_Constants_HaveCorrectValues()
    {
        // Arrange
        // Act
        // Assert
        CspBuilderService.Sources.Self.Should().Be("'self'");
        CspBuilderService.Sources.None.Should().Be("'none'");
        CspBuilderService.Sources.UnsafeInline.Should().Be("'unsafe-inline'");
        CspBuilderService.Sources.UnsafeEval.Should().Be("'unsafe-eval'");
        CspBuilderService.Sources.StrictDynamic.Should().Be("'strict-dynamic'");
        CspBuilderService.Sources.Data.Should().Be("data:");
        CspBuilderService.Sources.Blob.Should().Be("blob:");
        CspBuilderService.Sources.Https.Should().Be("https:");
        CspBuilderService.Sources.Http.Should().Be("http:");
        CspBuilderService.Sources.Ws.Should().Be("ws:");
        CspBuilderService.Sources.Wss.Should().Be("wss:");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void BuildCspHeader_WithComplexConfiguration_GeneratesCompleteHeader()
    {
        // Arrange
        var options = new CspOptions
        {
            FrameSrc = new List<string> { "'self'", "https://widget.example.com" },
            ScriptSrc = new List<string> { "'self'", "https://cdn.example.com" },
            FrameAncestors = new List<string> { "'self'" },
            ScriptNonce = "secure-nonce-123",
            AllowInlineScripts = false,
            AllowEval = false,
            UseStrictDynamic = true,
            CustomDirectives = new Dictionary<string, List<string>>
            {
                ["img-src"] = new List<string> { "'self'", "data:", "https:" },
                ["connect-src"] = new List<string> { "'self'", "https://api.example.com" }
            },
            ReportUri = "https://csp-report.example.com/report"
        };

        var iframeSources = new List<string> { "https://additional-widget.com" };

        // Act
        var result = _cspBuilderService.BuildCspHeader(options, iframeSources);

        // Assert
        result.HeaderName.Should().Be("Content-Security-Policy");
        result.IsReportOnly.Should().BeFalse();
        
        // Check all expected directives are present
        result.HeaderValue.Should().Contain("frame-src 'self' https://widget.example.com https://additional-widget.com");
        result.HeaderValue.Should().Contain("script-src 'self' https://cdn.example.com 'nonce-secure-nonce-123' 'strict-dynamic'");
        result.HeaderValue.Should().Contain("frame-ancestors 'self'");
        result.HeaderValue.Should().Contain("img-src 'self' data: https:");
        result.HeaderValue.Should().Contain("connect-src 'self' https://api.example.com");
        result.HeaderValue.Should().Contain("report-uri https://csp-report.example.com/report");
        
        // Check directives dictionary
        result.Directives.Should().HaveCount(5); // frame-src, script-src, frame-ancestors, img-src, connect-src
        result.Directives["frame-src"].Should().HaveCount(3);
        result.Directives["script-src"].Should().HaveCount(4);
    }

    [Fact]
    public void ValidateCspOptions_WithMultipleIssues_ReturnsAllIssues()
    {
        // Arrange
        var options = new CspOptions
        {
            AllowInlineScripts = true,
            AllowEval = true,
            UseStrictDynamic = true,
            ScriptNonce = "test-nonce",
            ReportOnly = true,
            ReportUri = null
        };

        // Act
        var result = _cspBuilderService.ValidateCspOptions(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Warnings.Should().HaveCountGreaterThan(0);
        result.Suggestions.Should().HaveCountGreaterThan(0);
        
        // Check specific warnings
        result.Warnings.Should().Contain(w => w.Contains("'unsafe-inline'"));
        result.Warnings.Should().Contain(w => w.Contains("'unsafe-eval'"));
        result.Warnings.Should().Contain(w => w.Contains("'strict-dynamic'"));
        
        // Check specific suggestions
        result.Suggestions.Should().Contain(s => s.Contains("'unsafe-inline'") && s.Contains("nonces"));
        result.Suggestions.Should().Contain(s => s.Contains("report-uri"));
    }

    #endregion
}