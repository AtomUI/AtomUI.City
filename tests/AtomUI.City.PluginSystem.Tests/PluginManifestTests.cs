using AtomUI.City.PluginSystem;

namespace AtomUI.City.PluginSystem.Tests;

public sealed class PluginManifestTests
{
    [Fact]
    public void ManifestReaderReadsPluginManifestSchema()
    {
        using var workspace = new PluginTestWorkspace();
        var manifestPath = workspace.WriteManifest(
            """
            {
              "schemaVersion": "1.0",
              "pluginId": "com.company.sales",
              "packageId": "Company.Sales.Plugin",
              "version": "1.2.3",
              "displayNameKey": "SalesPlugin.DisplayName",
              "descriptionKey": "SalesPlugin.Description",
              "publisher": "Company",
              "mainAssembly": "Company.Sales.Plugin.dll",
              "targetFramework": "net10.0",
              "pluginApiVersion": "1.0",
              "minHostVersion": "1.0.0",
              "unloadable": true,
              "aotCompatible": false,
              "capabilities": [
                { "name": "routes", "scope": ["/sales/**"] }
              ],
              "contributions": {
                "routes": { "path": "atomui-city/manifests/routes.json", "required": true }
              },
              "dependencies": {
                "plugins": [
                  { "pluginId": "com.company.identity", "versionRange": "[1.0.0,2.0.0)" }
                ]
              },
              "modules": [
                { "name": "SalesModule", "type": "Company.Sales.SalesModule" }
              ]
            }
            """);

        var manifest = PluginManifestReader.Read(manifestPath);

        Assert.Equal("com.company.sales", manifest.PluginId);
        Assert.Equal("Company.Sales.Plugin", manifest.PackageId);
        Assert.Equal("Company.Sales.Plugin.dll", manifest.MainAssembly);
        Assert.Equal("routes", Assert.Single(manifest.Capabilities).Name);
        Assert.Equal("atomui-city/manifests/routes.json", Assert.Single(manifest.Contributions).Path);
        Assert.Equal("com.company.identity", Assert.Single(manifest.Dependencies).PluginId);
        Assert.Equal("Company.Sales.SalesModule", Assert.Single(manifest.Modules).TypeName);
    }

    [Fact]
    public void ManifestValidatorRejectsMainAssemblyPaths()
    {
        var manifest = PluginManifestBuilder.Minimal(
            pluginId: "com.company.sales",
            packageId: "Company.Sales.Plugin",
            version: "1.0.0",
            mainAssembly: "../Company.Sales.Plugin.dll");

        var result = PluginManifestValidator.Validate(manifest);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == PluginDiagnosticIds.InvalidMainAssembly);
    }
}
