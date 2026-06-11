using AtomUI.City.PluginSystem;

namespace AtomUI.City.PluginSystem.Tests;

public sealed class PluginDeclarationAttributeTests
{
    [Fact]
    public void PluginAttributeStoresIdentityAndCompatibilityMetadata()
    {
        var attribute = new PluginAttribute("com.company.sales", "Company.Sales.Plugin", "1.0.0")
        {
            DisplayNameKey = "SalesPlugin.DisplayName",
            DescriptionKey = "SalesPlugin.Description",
            Publisher = "Company",
            MainAssembly = "Company.Sales.Plugin.dll",
            TargetFramework = "net10.0",
            PluginApiVersion = "1.0",
            MinHostVersion = "1.0.0",
            MaxHostVersion = "2.0.0",
            Unloadable = true,
            AotCompatible = false,
        };

        Assert.Equal("com.company.sales", attribute.PluginId);
        Assert.Equal("Company.Sales.Plugin", attribute.PackageId);
        Assert.Equal("1.0.0", attribute.Version);
        Assert.Equal("SalesPlugin.DisplayName", attribute.DisplayNameKey);
        Assert.Equal("SalesPlugin.Description", attribute.DescriptionKey);
        Assert.Equal("Company", attribute.Publisher);
        Assert.Equal("Company.Sales.Plugin.dll", attribute.MainAssembly);
        Assert.Equal("net10.0", attribute.TargetFramework);
        Assert.Equal("1.0", attribute.PluginApiVersion);
        Assert.Equal("1.0.0", attribute.MinHostVersion);
        Assert.Equal("2.0.0", attribute.MaxHostVersion);
        Assert.True(attribute.Unloadable);
        Assert.False(attribute.AotCompatible);
    }

    [Fact]
    public void PluginCapabilityAttributeStoresNameAndScopes()
    {
        var attribute = new PluginCapabilityAttribute("routes")
        {
            Scope = ["/sales/**", "/customers/**"],
        };

        Assert.Equal("routes", attribute.Name);
        Assert.Equal(["/sales/**", "/customers/**"], attribute.Scope);
    }

    [Fact]
    public void ContributionManifestAttributeStoresManifestIndexEntry()
    {
        var attribute = new ContributionManifestAttribute("routes", "manifests/routes.json")
        {
            Required = true,
        };

        Assert.Equal("routes", attribute.Type);
        Assert.Equal("manifests/routes.json", attribute.Path);
        Assert.True(attribute.Required);
    }

    [Fact]
    public void PluginDependencyAttributeStoresVersionRange()
    {
        var attribute = new PluginDependencyAttribute("com.company.identity")
        {
            VersionRange = "[1.0.0,2.0.0)",
        };

        Assert.Equal("com.company.identity", attribute.PluginId);
        Assert.Equal("[1.0.0,2.0.0)", attribute.VersionRange);
    }
}
