using System.Globalization;
using AtomUI.City.Localization;

namespace AtomUI.City.Localization.Tests;

public sealed class LanguagePackageProviderTests
{
    [Fact]
    public async Task FileProviderLoadsLocPackForRequestedCulture()
    {
        using var workspace = new LocalizationTestWorkspace();
        var locpackPath = workspace.WriteLocPack(
            """
            {
              "packageId": "Host.zh-CN",
              "culture": "zh-CN",
              "resources": {
                "Settings.Title": "Settings zh"
              }
            }
            """);
        var descriptor = new LanguagePackageDescriptor(
            "Host.zh-CN",
            CultureInfo.GetCultureInfo("zh-CN"),
            ResourceScope.Host)
        {
            ProviderKind = LanguagePackageProviderKind.File,
            Location = locpackPath,
        };
        var provider = new FileLanguagePackageProvider();

        var result = await provider.LoadAsync(descriptor);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Package);
        Assert.True(result.Package.TryGetString("Settings.Title", out var value));
        Assert.Equal("Settings zh", value);
    }

    [Fact]
    public async Task AssemblyProviderLoadsEmbeddedLocPackResource()
    {
        var descriptor = new LanguagePackageDescriptor(
            "Host.en-US",
            CultureInfo.GetCultureInfo("en-US"),
            ResourceScope.Host)
        {
            ProviderKind = LanguagePackageProviderKind.Assembly,
            Location = typeof(LanguagePackageProviderTests).Assembly.Location,
            ResourceBaseName = "AtomUI.City.Localization.Tests.Fixtures.Host.en-US.locpack.json",
        };
        var provider = new AssemblyLanguagePackageProvider();

        var result = await provider.LoadAsync(descriptor);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Package);
        Assert.True(result.Package.TryGetString("Settings.Title", out var value));
        Assert.Equal("Settings", value);
    }

    [Fact]
    public async Task FileProviderRejectsCultureMismatch()
    {
        using var workspace = new LocalizationTestWorkspace();
        var locpackPath = workspace.WriteLocPack(
            """
            {
              "packageId": "Host.zh-CN",
              "culture": "en-US",
              "resources": {
                "Settings.Title": "Settings"
              }
            }
            """);
        var descriptor = new LanguagePackageDescriptor(
            "Host.zh-CN",
            CultureInfo.GetCultureInfo("zh-CN"),
            ResourceScope.Host)
        {
            ProviderKind = LanguagePackageProviderKind.File,
            Location = locpackPath,
        };
        var provider = new FileLanguagePackageProvider();

        var result = await provider.LoadAsync(descriptor);

        Assert.False(result.Succeeded);
        Assert.Equal(LocalizationErrorKind.PackageCultureMismatch, result.Error?.Kind);
    }
}
