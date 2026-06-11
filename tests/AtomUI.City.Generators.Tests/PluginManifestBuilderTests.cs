using AtomUI.City.Generators.Diagnostics;
using AtomUI.City.Generators.PluginSystem;

namespace AtomUI.City.Generators.Tests;

public sealed class PluginManifestBuilderTests
{
    [Fact]
    public void BuildCreatesDeterministicManifestWithCapabilitiesContributionsAndDependencies()
    {
        var result = PluginManifestBuilder.Build(
            Metadata(
                capabilities:
                [
                    Capability("localization"),
                    Capability("routes", ["/sales/**"]),
                ],
                contributions:
                [
                    Contribution("routes", "manifests/routes.json", required: true),
                    Contribution("localization", "manifests/localization.json", required: false),
                ],
                dependencies:
                [
                    Dependency("com.company.identity", "[1.0.0,2.0.0)"),
                ]));

        Assert.Empty(result.Diagnostics);
        Assert.Equal("1.0", result.Manifest.SchemaVersion);
        Assert.Equal("com.company.sales", result.Manifest.PluginId);
        Assert.Equal("Company.Sales.Plugin", result.Manifest.PackageId);
        Assert.Equal(["localization", "routes"], result.Manifest.Capabilities.Select(capability => capability.Name));
        Assert.Equal(["localization", "routes"], result.Manifest.Contributions.Select(contribution => contribution.Type));
        Assert.Equal("com.company.identity", Assert.Single(result.Manifest.Dependencies).PluginId);
    }

    [Fact]
    public void BuildReportsInvalidMainAssemblyPath()
    {
        var result = PluginManifestBuilder.Build(Metadata(mainAssembly: "../Company.Sales.Plugin.dll"));

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.InvalidManifestInput, diagnostic.Id);
        Assert.Empty(result.Manifest.Capabilities);
        Assert.Empty(result.Manifest.Contributions);
    }

    [Fact]
    public void BuildReportsDuplicateContributionTypes()
    {
        var result = PluginManifestBuilder.Build(
            Metadata(
                contributions:
                [
                    Contribution("routes", "manifests/routes.json", required: true),
                    Contribution("routes", "manifests/plugin-routes.json", required: false),
                ]));

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.InvalidManifestInput, diagnostic.Id);
        Assert.Empty(result.Manifest.Contributions);
    }

    [Fact]
    public void BuildReportsDuplicateCapabilities()
    {
        var result = PluginManifestBuilder.Build(
            Metadata(
                capabilities:
                [
                    Capability("routes"),
                    Capability("routes"),
                ]));

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.InvalidManifestInput, diagnostic.Id);
        Assert.Empty(result.Manifest.Capabilities);
    }

    [Theory]
    [InlineData("../manifests/routes.json")]
    [InlineData("/manifests/routes.json")]
    [InlineData("manifests\\routes.json")]
    public void BuildReportsInvalidContributionManifestPaths(string path)
    {
        var result = PluginManifestBuilder.Build(
            Metadata(
                contributions:
                [
                    Contribution("routes", path, required: true),
                ]));

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.InvalidManifestInput, diagnostic.Id);
        Assert.Empty(result.Manifest.Contributions);
    }

    private static PluginMetadata Metadata(
        string mainAssembly = "Company.Sales.Plugin.dll",
        IReadOnlyList<PluginCapabilityMetadata>? capabilities = null,
        IReadOnlyList<PluginContributionManifestMetadata>? contributions = null,
        IReadOnlyList<PluginDependencyMetadata>? dependencies = null)
    {
        return new PluginMetadata(
            schemaVersion: "1.0",
            pluginId: "com.company.sales",
            packageId: "Company.Sales.Plugin",
            version: "1.0.0",
            displayNameKey: "SalesPlugin.DisplayName",
            descriptionKey: "SalesPlugin.Description",
            publisher: "Company",
            mainAssembly,
            targetFramework: "net10.0",
            pluginApiVersion: "1.0",
            minHostVersion: "1.0.0",
            maxHostVersion: null,
            unloadable: true,
            aotCompatible: false,
            capabilities ?? [],
            contributions ?? [],
            dependencies ?? []);
    }

    private static PluginCapabilityMetadata Capability(string name, IReadOnlyList<string>? scope = null)
    {
        return new PluginCapabilityMetadata(name, scope ?? []);
    }

    private static PluginContributionManifestMetadata Contribution(string type, string path, bool required)
    {
        return new PluginContributionManifestMetadata(type, path, required);
    }

    private static PluginDependencyMetadata Dependency(string pluginId, string? versionRange)
    {
        return new PluginDependencyMetadata(pluginId, versionRange);
    }
}
