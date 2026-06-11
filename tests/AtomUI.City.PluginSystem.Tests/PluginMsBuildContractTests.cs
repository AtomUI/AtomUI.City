using AtomUI.City.PluginSystem;

namespace AtomUI.City.PluginSystem.Tests;

public sealed class PluginMsBuildContractTests
{
    [Fact]
    public void PluginMsBuildContractContainsDocumentedPropertiesItemsAndTargets()
    {
        Assert.Contains("AtomUICityPlugin", PluginMsBuildContract.Properties);
        Assert.Contains("AtomUICityPluginId", PluginMsBuildContract.Properties);
        Assert.Contains("AtomUICityPackageAsPlugin", PluginMsBuildContract.Properties);
        Assert.Contains("AtomUICityPluginCapability", PluginMsBuildContract.Items);
        Assert.Contains("AtomUICityPluginDependency", PluginMsBuildContract.Items);
        Assert.Contains("GenerateAtomUICityPluginManifest", PluginMsBuildContract.Targets);
        Assert.Contains("ValidateAtomUICityPluginPackage", PluginMsBuildContract.Targets);
        Assert.Contains("PackAtomUICityPlugin", PluginMsBuildContract.Targets);
        Assert.Contains("InstallAtomUICityPluginToLocalCache", PluginMsBuildContract.Targets);
    }
}
