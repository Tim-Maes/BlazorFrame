using BlazorFrame.Services;
using FluentAssertions;
using Xunit;
using BlazorFrame;

namespace BlazorFrame.Tests;

public class SecurityContractTests
{
    [Fact]
    public void Origins_NormalizeDefaultPortsLikeMessageEventOrigin()
    {
        var service = new MessageValidationService();

        service.NormalizeOrigin("HTTPS://Example.com:443").Should().Be("https://example.com");
        service.NormalizeOrigin("http://Example.com:80").Should().Be("http://example.com");
    }

    [Fact]
    public void Sandbox_IsDisabledByMasterSwitch()
    {
        var options = new MessageSecurityOptions
        {
            SandboxPreset = SandboxPreset.Strict,
            EnableSandbox = false
        };

        options.GetEffectiveSandboxValue().Should().BeNull();
    }

    [Fact]
    public void StrictSandbox_DoesNotGrantSameOrigin()
    {
        var options = new MessageSecurityOptions().WithStrictSandbox();

        options.GetEffectiveSandboxValue().Should().Be("allow-scripts");
    }

    [Fact]
    public void CapabilityToken_RequiresMinimumEntropy()
    {
        var options = new MessageSecurityOptions { CapabilityToken = "short" };

        options.ValidateConfiguration().Errors.Should().ContainSingle(error => error.Contains("CapabilityToken"));
    }
}
