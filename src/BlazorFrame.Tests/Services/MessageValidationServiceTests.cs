using BlazorFrame.Services;
using FluentAssertions;
using Xunit;

namespace BlazorFrame.Tests.Services;

public class MessageValidationServiceTests
{
    private readonly MessageValidationService _validationService;

    public MessageValidationServiceTests()
    {
        _validationService = new MessageValidationService();
    }

    #region ValidateMessage Tests

    [Fact]
    public void ValidateMessage_WithValidOriginAndMessage_ReturnsValidMessage()
    {
        // Arrange
        var origin = "https://example.com";
        var messageJson = "{\"type\": \"test\", \"data\": \"hello\"}";
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions();

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Origin.Should().Be(origin);
        result.Data.Should().Be(messageJson);
        result.ValidationError.Should().BeNull();
        result.MessageType.Should().Be("test");
    }

    [Fact]
    public void ValidateMessage_WithNullOrigin_ReturnsInvalidMessage()
    {
        // Arrange
        string origin = null!;
        var messageJson = "{\"type\": \"test\"}";
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions();

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationError.Should().Be("Origin is null or empty");
    }

    [Fact]
    public void ValidateMessage_WithEmptyOrigin_ReturnsInvalidMessage()
    {
        // Arrange
        var origin = string.Empty;
        var messageJson = "{\"type\": \"test\"}";
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions();

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationError.Should().Be("Origin is null or empty");
    }

    [Fact]
    public void ValidateMessage_WithNullMessage_ReturnsInvalidMessage()
    {
        // Arrange
        var origin = "https://example.com";
        string messageJson = null!;
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions();

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationError.Should().Be("Message data is null or empty");
    }

    [Fact]
    public void ValidateMessage_WithEmptyMessage_ReturnsInvalidMessage()
    {
        // Arrange
        var origin = "https://example.com";
        var messageJson = string.Empty;
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions();

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationError.Should().Be("Message data is null or empty");
    }

    [Fact]
    public void ValidateMessage_WithMessageExceedingMaxSize_ReturnsInvalidMessage()
    {
        // Arrange
        var origin = "https://example.com";
        var messageJson = new string('x', 1000);
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions { MaxMessageSize = 500 };

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationError.Should().Contain("Message size (1000 bytes) exceeds maximum allowed size (500 bytes)");
    }

    [Fact]
    public void ValidateMessage_WithSuspiciousContent_ReturnsInvalidMessage()
    {
        // Arrange
        var origin = "https://example.com";
        var messageJson = "{\"type\": \"test\", \"script\": \"<script>alert('xss')</script>\"}";
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions();

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationError.Should().Be("Message contains potentially malicious content");
    }

    [Fact]
    public void ValidateMessage_WithUnallowedOrigin_ReturnsInvalidMessage()
    {
        // Arrange
        var origin = "https://malicious.com";
        var messageJson = "{\"type\": \"test\"}";
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions();

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationError.Should().Be("Origin 'https://malicious.com' is not in the allowed origins list");
    }

    [Fact]
    public void ValidateMessage_WithStrictValidationAndInvalidJson_ReturnsInvalidMessage()
    {
        // Arrange
        var origin = "https://example.com";
        var messageJson = "{\"type\": \"test\", \"invalid\": json}";
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions { EnableStrictValidation = true };

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationError.Should().Contain("Invalid JSON format");
    }

    [Fact]
    public void ValidateMessage_WithStrictValidationAndTooDeepNesting_ReturnsInvalidMessage()
    {
        // Arrange
        var origin = "https://example.com";
        var deepJson = "{\"level1\": {\"level2\": {\"level3\": {\"level4\": {\"level5\": {\"level6\": {\"level7\": {\"level8\": {\"level9\": {\"level10\": {\"level11\": \"too deep\"}}}}}}}}}}}";
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions { EnableStrictValidation = true, MaxJsonDepth = 5 };

        // Act
        var result = _validationService.ValidateMessage(origin, deepJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeFalse();
        
        // The error message can be either our custom message or the JSON parser's depth limit message
        (result.ValidationError.Contains("JSON structure is too complex or deeply nested") ||
         (result.ValidationError.Contains("Invalid JSON format") && result.ValidationError.Contains("maximum configured depth")))
        .Should().BeTrue("because deep nesting should be detected and reported");
    }

    [Fact]
    public void ValidateMessage_WithStrictValidationAndTooManyProperties_ReturnsInvalidMessage()
    {
        // Arrange
        var origin = "https://example.com";
        var properties = string.Join(", ", Enumerable.Range(1, 101).Select(i => $"\"prop{i}\": \"value{i}\""));
        var messageJson = "{" + properties + "}";
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions { EnableStrictValidation = true, MaxObjectProperties = 50 };

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationError.Should().Contain("JSON structure is too complex or deeply nested");
    }

    [Fact]
    public void ValidateMessage_WithStrictValidationAndTooManyArrayElements_ReturnsInvalidMessage()
    {
        // Arrange
        var origin = "https://example.com";
        var array = "[" + string.Join(", ", Enumerable.Range(1, 1001).Select(i => $"\"{i}\"")) + "]";
        var messageJson = "{\"data\": " + array + "}";
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions { EnableStrictValidation = true, MaxArrayElements = 500 };

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationError.Should().Contain("JSON structure is too complex or deeply nested");
    }

    [Fact]
    public void ValidateMessage_WithCustomValidatorReturningFalse_ReturnsInvalidMessage()
    {
        // Arrange
        var origin = "https://example.com";
        var messageJson = "{\"type\": \"test\"}";
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions
        {
            CustomValidator = (org, msg) => false
        };

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationError.Should().Be("Custom validation failed");
    }

    [Fact]
    public void ValidateMessage_WithCustomValidatorReturningTrue_ReturnsValidMessage()
    {
        // Arrange
        var origin = "https://example.com";
        var messageJson = "{\"type\": \"test\"}";
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions
        {
            CustomValidator = (org, msg) => true
        };

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ValidationError.Should().BeNull();
    }

    [Fact]
    public void ValidateMessage_WithCustomValidatorThrowingException_ReturnsInvalidMessage()
    {
        // Arrange
        var origin = "https://example.com";
        var messageJson = "{\"type\": \"test\"}";
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions
        {
            CustomValidator = (org, msg) => throw new InvalidOperationException("Custom error")
        };

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationError.Should().Be("Custom validation error: Custom error");
    }

    [Theory]
    [InlineData("javascript:alert('xss')")]
    [InlineData("vbscript:MsgBox('xss')")]
    [InlineData("onload=alert('xss')")]
    [InlineData("onerror=alert('xss')")]
    [InlineData("eval(maliciousCode)")]
    [InlineData("Function('return evil')()")]
    [InlineData("setTimeout(hack, 1000)")]
    [InlineData("setInterval(malware, 100)")]
    public void ValidateMessage_WithSuspiciousPatterns_ReturnsInvalidMessage(string suspiciousContent)
    {
        // Arrange
        var origin = "https://example.com";
        var messageJson = "{\"content\": \"" + suspiciousContent + "\"}";
        var allowedOrigins = new List<string> { "https://example.com" };
        var options = new MessageSecurityOptions();

        // Act
        var result = _validationService.ValidateMessage(origin, messageJson, allowedOrigins, options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationError.Should().Be("Message contains potentially malicious content");
    }

    #endregion

    #region ExtractOrigin Tests

    [Theory]
    [InlineData("https://example.com", "https://example.com")]
    [InlineData("https://example.com:8080", "https://example.com:8080")]
    [InlineData("https://subdomain.example.com", "https://subdomain.example.com")]
    [InlineData("http://localhost:3000", "http://localhost:3000")]
    [InlineData("https://example.com/path/to/page", "https://example.com")]
    [InlineData("https://example.com/path?query=value", "https://example.com")]
    [InlineData("https://example.com:443/secure/path", "https://example.com:443")]
    public void ExtractOrigin_WithValidUrl_ReturnsCorrectOrigin(string url, string expectedOrigin)
    {
        // Arrange
        // Act
        var result = _validationService.ExtractOrigin(url);

        // Assert
        result.Should().Be(expectedOrigin);
    }

    [Theory]
    [InlineData("data:text/html,<html></html>", "data:")]
    [InlineData("blob:https://example.com/uuid", "blob:")]
    public void ExtractOrigin_WithSpecialSchemes_ReturnsSchemePrefix(string url, string expectedOrigin)
    {
        // Arrange
        // Act
        var result = _validationService.ExtractOrigin(url);

        // Assert
        result.Should().Be(expectedOrigin);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/relative/path")]
    [InlineData("invalid-url")]
    [InlineData("ftp://unsupported.com")]
    [InlineData("file:///local/file")]
    public void ExtractOrigin_WithInvalidUrl_ReturnsNull(string url)
    {
        // Arrange
        // Act
        var result = _validationService.ExtractOrigin(url);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ValidateUrl Tests

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("https://subdomain.example.com:8080")]
    [InlineData("http://localhost:3000")]
    [InlineData("data:text/html,<html></html>")]
    [InlineData("blob:https://example.com/uuid")]
    public void ValidateUrl_WithValidUrl_ReturnsValid(string url)
    {
        // Arrange
        var options = new MessageSecurityOptions();

        // Act
        var (isValid, errorMessage) = _validationService.ValidateUrl(url, options);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ValidateUrl_WithNullOrEmptyUrl_ReturnsInvalid(string url)
    {
        // Arrange
        var options = new MessageSecurityOptions();

        // Act
        var (isValid, errorMessage) = _validationService.ValidateUrl(url, options);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("URL cannot be null or empty");
    }

    [Fact]
    public void ValidateUrl_WithRelativeUrl_ReturnsValid()
    {
        // Arrange
        var url = "/relative/path";
        var options = new MessageSecurityOptions();

        // Act
        var (isValid, errorMessage) = _validationService.ValidateUrl(url, options);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Fact]
    public void ValidateUrl_WithHttpUrlAndRequireHttps_ReturnsInvalid()
    {
        // Arrange
        var url = "http://example.com";
        var options = new MessageSecurityOptions 
        { 
            RequireHttps = true, 
            AllowInsecureConnections = false 
        };

        // Act
        var (isValid, errorMessage) = _validationService.ValidateUrl(url, options);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("HTTPS is required but URL uses HTTP protocol");
    }

    [Fact]
    public void ValidateUrl_WithHttpUrlAndRequireHttpsButAllowInsecure_ReturnsValid()
    {
        // Arrange
        var url = "http://example.com";
        var options = new MessageSecurityOptions 
        { 
            RequireHttps = true, 
            AllowInsecureConnections = true 
        };

        // Act
        var (isValid, errorMessage) = _validationService.ValidateUrl(url, options);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData("javascript:alert('xss')")]
    [InlineData("vbscript:MsgBox('hello')")]
    [InlineData("livescript:doSomething()")]
    public void ValidateUrl_WithScriptProtocolsDisallowed_ReturnsInvalid(string url)
    {
        // Arrange
        var options = new MessageSecurityOptions { AllowScriptProtocols = false };

        // Act
        var (isValid, errorMessage) = _validationService.ValidateUrl(url, options);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("is not allowed. Enable AllowScriptProtocols");
    }

    [Theory]
    [InlineData("javascript:alert('xss')")]
    [InlineData("vbscript:MsgBox('hello')")]
    [InlineData("livescript:doSomething()")]
    public void ValidateUrl_WithScriptProtocolsAllowed_ReturnsValid(string url)
    {
        // Arrange
        var options = new MessageSecurityOptions { AllowScriptProtocols = true };

        // Act
        var (isValid, errorMessage) = _validationService.ValidateUrl(url, options);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("file:///local/file")]
    [InlineData("ldap://directory.com")]
    public void ValidateUrl_WithUnsupportedScheme_ReturnsInvalid(string url)
    {
        // Arrange
        var options = new MessageSecurityOptions();

        // Act
        var (isValid, errorMessage) = _validationService.ValidateUrl(url, options);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("is not allowed. Allowed schemes: http, https, data, blob");
    }

    [Fact]
    public void ValidateUrl_WithMalformedUrl_ReturnsInvalid()
    {
        // Arrange
        var url = "not-a-valid-url";
        var options = new MessageSecurityOptions();

        // Act
        var (isValid, errorMessage) = _validationService.ValidateUrl(url, options);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("Invalid URL format");
    }

    #endregion
}