using AtomUI.City.Generators.Diagnostics;
using AtomUI.City.Generators.Localization;

namespace AtomUI.City.Generators.Tests;

public sealed class LocalizationManifestBuilderTests
{
    [Fact]
    public void BuildCreatesDeterministicManifestWithSupportedCulturesAndFallbacks()
    {
        var result = LocalizationManifestBuilder.Build(
            [
                Package("Settings.en-US", "en-US"),
                Package("Settings.zh-CN", "zh-CN", fallbackCulture: "zh-Hans"),
                Package("Settings.zh-Hans", "zh-Hans"),
            ],
            [
                Resource("Settings.Title", "Settings.zh-CN"),
                Resource("Settings.Title", "Settings.en-US"),
            ]);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(["Settings.en-US", "Settings.zh-CN", "Settings.zh-Hans"], result.Manifest.Packages.Select(package => package.PackageId));
        Assert.Equal(["en-US", "zh-CN", "zh-Hans"], result.Manifest.SupportedCultures);
        Assert.Collection(
            result.Manifest.Fallbacks,
            fallback =>
            {
                Assert.Equal("zh-CN", fallback.Culture);
                Assert.Equal("zh-Hans", fallback.FallbackCulture);
            });
        Assert.Equal(["Settings.en-US:Settings.Title", "Settings.zh-CN:Settings.Title"], result.Manifest.Resources.Select(resource => resource.PackageId + ":" + resource.Key));
    }

    [Fact]
    public void BuildReportsResourcesThatReferenceMissingPackages()
    {
        var result = LocalizationManifestBuilder.Build(
            [],
            [
                Resource("Settings.Title", "Settings.zh-CN"),
            ]);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.InvalidManifestInput, diagnostic.Id);
        Assert.Empty(result.Manifest.Resources);
    }

    [Fact]
    public void BuildReportsDuplicateResourceKeysWithinSamePackageAndScope()
    {
        var result = LocalizationManifestBuilder.Build(
            [
                Package("Settings.zh-CN", "zh-CN"),
            ],
            [
                Resource("Settings.Title", "Settings.zh-CN"),
                Resource("Settings.Title", "Settings.zh-CN"),
            ]);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.InvalidManifestInput, diagnostic.Id);
        Assert.Empty(result.Manifest.Resources);
    }

    [Fact]
    public void BuildReportsFallbackCulturesWithoutLanguagePackage()
    {
        var result = LocalizationManifestBuilder.Build(
            [
                Package("Settings.zh-CN", "zh-CN", fallbackCulture: "zh-Hans"),
            ],
            [
                Resource("Settings.Title", "Settings.zh-CN"),
            ]);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.InvalidManifestInput, diagnostic.Id);
        Assert.Empty(result.Manifest.Packages);
        Assert.Empty(result.Manifest.Fallbacks);
    }

    private static LanguagePackageMetadata Package(
        string packageId,
        string culture,
        string? fallbackCulture = null)
    {
        return new LanguagePackageMetadata(
            packageId,
            culture,
            ResourceScopeMetadata.Module,
            resourceBaseName: "Sample.App.Resources." + packageId,
            fallbackCulture,
            version: null,
            checksum: null);
    }

    private static LocalizedResourceMetadata Resource(string key, string packageId)
    {
        return new LocalizedResourceMetadata(
            key,
            packageId,
            LocalizedResourceMetadataKind.String,
            ResourceScopeMetadata.Module,
            version: null,
            critical: false);
    }
}
