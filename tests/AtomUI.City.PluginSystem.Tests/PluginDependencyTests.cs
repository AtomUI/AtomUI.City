using AtomUI.City.PluginSystem;

namespace AtomUI.City.PluginSystem.Tests;

public sealed class PluginDependencyTests
{
    [Fact]
    public void DependencyValidatorRejectsMissingPluginDependency()
    {
        var sales = PluginDescriptor.FromManifest(
            PluginManifestBuilder.Minimal(
                pluginId: "com.company.sales",
                packageId: "Company.Sales.Plugin",
                version: "1.0.0",
                dependencies: [new PluginDependencyDescriptor("com.company.identity", "[1.0.0,2.0.0)")]),
            rootPath: "/plugins/installed/com.company.sales/1.0.0/root");

        var result = PluginDependencyValidator.Validate([sales]);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == PluginDiagnosticIds.PluginDependencyMissing);
    }

    [Fact]
    public void DependencyValidatorRejectsDependencyCycles()
    {
        var first = PluginDescriptor.FromManifest(
            PluginManifestBuilder.Minimal(
                pluginId: "com.company.first",
                packageId: "Company.First.Plugin",
                version: "1.0.0",
                dependencies: [new PluginDependencyDescriptor("com.company.second", null)]),
            rootPath: "/plugins/installed/com.company.first/1.0.0/root");
        var second = PluginDescriptor.FromManifest(
            PluginManifestBuilder.Minimal(
                pluginId: "com.company.second",
                packageId: "Company.Second.Plugin",
                version: "1.0.0",
                dependencies: [new PluginDependencyDescriptor("com.company.first", null)]),
            rootPath: "/plugins/installed/com.company.second/1.0.0/root");

        var result = PluginDependencyValidator.Validate([first, second]);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == PluginDiagnosticIds.PluginDependencyCycle);
    }
}
