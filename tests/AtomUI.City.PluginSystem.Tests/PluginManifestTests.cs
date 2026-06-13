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

    [Theory]
    [InlineData("../sales")]
    [InlineData("com/company/sales")]
    [InlineData("com\\company\\sales")]
    [InlineData("..")]
    public void ManifestValidatorRejectsPluginIdPathSegments(string pluginId)
    {
        var manifest = PluginManifestBuilder.Minimal(
            pluginId: pluginId,
            packageId: "Company.Sales.Plugin",
            version: "1.0.0");

        var result = PluginManifestValidator.Validate(manifest);

        Assert.False(result.Succeeded);
        Assert.Contains(
            result.Diagnostics,
            diagnostic => diagnostic.Code == PluginDiagnosticIds.InvalidPluginId
                && diagnostic.Field == "pluginId");
    }

    [Theory]
    [InlineData("../1.0.0")]
    [InlineData("1/0/0")]
    [InlineData("1\\0\\0")]
    [InlineData("..")]
    public void ManifestValidatorRejectsVersionPathSegments(string version)
    {
        var manifest = PluginManifestBuilder.Minimal(
            pluginId: "com.company.sales",
            packageId: "Company.Sales.Plugin",
            version: version);

        var result = PluginManifestValidator.Validate(manifest);

        Assert.False(result.Succeeded);
        Assert.Contains(
            result.Diagnostics,
            diagnostic => diagnostic.Code == PluginDiagnosticIds.InvalidPluginVersion
                && diagnostic.Field == "version");
    }

    [Theory]
    [InlineData("../net10.0")]
    [InlineData("net/10.0")]
    [InlineData("net\\10.0")]
    [InlineData("..")]
    public void ManifestValidatorRejectsTargetFrameworkPathSegments(string targetFramework)
    {
        var manifest = new PluginManifest(
            schemaVersion: "1.0",
            pluginId: "com.company.sales",
            packageId: "Company.Sales.Plugin",
            version: "1.0.0",
            displayNameKey: "SalesPlugin.DisplayName",
            descriptionKey: null,
            publisher: null,
            mainAssembly: "Company.Sales.Plugin.dll",
            targetFramework: targetFramework,
            pluginApiVersion: "1.0",
            minHostVersion: "1.0.0",
            maxHostVersion: null,
            unloadable: true,
            aotCompatible: false,
            capabilities: [],
            contributions: [],
            dependencies: [],
            modules: []);

        var result = PluginManifestValidator.Validate(manifest);

        Assert.False(result.Succeeded);
        Assert.Contains(
            result.Diagnostics,
            diagnostic => diagnostic.Code == PluginDiagnosticIds.InvalidTargetFramework
                && diagnostic.Field == "targetFramework");
    }

    [Theory]
    [InlineData("../routes.json")]
    [InlineData("/atomui-city/routes.json")]
    [InlineData("atomui-city\\routes.json")]
    [InlineData("atomui-city/../routes.json")]
    public void ManifestValidatorRejectsContributionPathsOutsidePackage(string path)
    {
        var manifest = PluginManifestBuilder.Minimal(
            pluginId: "com.company.sales",
            packageId: "Company.Sales.Plugin",
            version: "1.0.0",
            contributions:
            [
                new PluginContributionDescriptor("routes", path, Required: true),
            ]);

        var result = PluginManifestValidator.Validate(manifest);

        Assert.False(result.Succeeded);
        Assert.Contains(
            result.Diagnostics,
            diagnostic => diagnostic.Code == PluginDiagnosticIds.InvalidContributionPath
                && diagnostic.Field == "routes");
    }

    [Fact]
    public void ManifestCollectionsRejectExternalListMutation()
    {
        var capability = new PluginCapabilityDescriptor("routes", ["/sales/**"]);
        var contribution = new PluginContributionDescriptor("routes", "atomui-city/manifests/routes.json", Required: true);
        var dependency = new PluginDependencyDescriptor("com.company.identity", "[1.0.0,2.0.0)");
        var module = new PluginModuleDescriptor("SalesModule", "Company.Sales.SalesModule");
        var manifest = PluginManifestBuilder.Minimal(
            pluginId: "com.company.sales",
            packageId: "Company.Sales.Plugin",
            version: "1.0.0",
            capabilities: [capability],
            contributions: [contribution],
            dependencies: [dependency],
            modules: [module]);

        var capabilities = Assert.IsAssignableFrom<IList<PluginCapabilityDescriptor>>(manifest.Capabilities);
        var contributions = Assert.IsAssignableFrom<IList<PluginContributionDescriptor>>(manifest.Contributions);
        var dependencies = Assert.IsAssignableFrom<IList<PluginDependencyDescriptor>>(manifest.Dependencies);
        var modules = Assert.IsAssignableFrom<IList<PluginModuleDescriptor>>(manifest.Modules);

        Assert.Throws<NotSupportedException>(() => capabilities[0] = new PluginCapabilityDescriptor("commands", []));
        Assert.Throws<NotSupportedException>(() => contributions[0] = new PluginContributionDescriptor("commands", "commands.json", Required: false));
        Assert.Throws<NotSupportedException>(() => dependencies[0] = new PluginDependencyDescriptor("com.company.other", null));
        Assert.Throws<NotSupportedException>(() => modules[0] = new PluginModuleDescriptor("OtherModule", "Company.Other.Module"));
        Assert.Equal(capability.Name, manifest.Capabilities[0].Name);
        Assert.Equal(contribution.Type, manifest.Contributions[0].Type);
        Assert.Equal(dependency.PluginId, manifest.Dependencies[0].PluginId);
        Assert.Equal(module.Name, manifest.Modules[0].Name);
    }

    [Fact]
    public void ManifestNestedCollectionsRejectExternalListMutation()
    {
        string[] scope = ["/sales/**"];
        string[] dependencies = ["IdentityModule"];
        var capability = new PluginCapabilityDescriptor("routes", scope);
        var module = new PluginModuleDescriptor("SalesModule", "Company.Sales.SalesModule", dependencies);
        var scopes = Assert.IsAssignableFrom<IList<string>>(capability.Scope);
        var moduleDependencies = Assert.IsAssignableFrom<IList<string>>(module.Dependencies);

        Assert.Throws<NotSupportedException>(() => scopes[0] = "/other/**");
        Assert.Throws<NotSupportedException>(() => moduleDependencies[0] = "OtherModule");
        Assert.Equal("/sales/**", capability.Scope[0]);
        Assert.Equal("IdentityModule", module.Dependencies![0]);
    }
}
