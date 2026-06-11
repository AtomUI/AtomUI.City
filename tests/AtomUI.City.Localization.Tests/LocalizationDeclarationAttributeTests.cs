using AtomUI.City.Localization;

namespace AtomUI.City.Localization.Tests;

public sealed class LocalizationDeclarationAttributeTests
{
    [Fact]
    public void LanguagePackageAttributeStoresPackageDescriptor()
    {
        var attribute = new LanguagePackageAttribute("Host.zh-CN", "zh-CN")
        {
            Scope = ResourceScope.Host,
            ResourceBaseName = "Sample.App.Resources.Host",
            FallbackCulture = "zh-Hans",
            Version = "1.0.0",
            Checksum = "sha256:sample",
        };

        Assert.Equal("Host.zh-CN", attribute.PackageId);
        Assert.Equal("zh-CN", attribute.Culture);
        Assert.Equal(ResourceScope.Host, attribute.Scope);
        Assert.Equal("Sample.App.Resources.Host", attribute.ResourceBaseName);
        Assert.Equal("zh-Hans", attribute.FallbackCulture);
        Assert.Equal("1.0.0", attribute.Version);
        Assert.Equal("sha256:sample", attribute.Checksum);
    }

    [Fact]
    public void LocalizedResourceAttributeStoresResourceDescriptor()
    {
        var attribute = new LocalizedResourceAttribute("Settings.Title", "Settings.zh-CN")
        {
            Kind = LocalizedResourceKind.FormattedString,
            Scope = ResourceScope.Module,
            Version = "1.0.0",
            Critical = true,
        };

        Assert.Equal("Settings.Title", attribute.Key);
        Assert.Equal("Settings.zh-CN", attribute.PackageId);
        Assert.Equal(LocalizedResourceKind.FormattedString, attribute.Kind);
        Assert.Equal(ResourceScope.Module, attribute.Scope);
        Assert.Equal("1.0.0", attribute.Version);
        Assert.True(attribute.Critical);
    }
}
