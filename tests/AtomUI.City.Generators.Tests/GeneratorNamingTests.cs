using AtomUI.City.Generators.Common;

namespace AtomUI.City.Generators.Tests;

public sealed class GeneratorNamingTests
{
    [Fact]
    public void FeatureNamesAreStable()
    {
        Assert.Equal("Modularity", GeneratorFeatureNames.GetName(GeneratorFeature.Modularity));
        Assert.Equal("Routing", GeneratorFeatureNames.GetName(GeneratorFeature.Routing));
        Assert.Equal("Presentation", GeneratorFeatureNames.GetName(GeneratorFeature.Presentation));
        Assert.Equal("Security", GeneratorFeatureNames.GetName(GeneratorFeature.Security));
        Assert.Equal("EventBus", GeneratorFeatureNames.GetName(GeneratorFeature.EventBus));
        Assert.Equal("Localization", GeneratorFeatureNames.GetName(GeneratorFeature.Localization));
        Assert.Equal("PluginSystem", GeneratorFeatureNames.GetName(GeneratorFeature.PluginSystem));
    }

    [Fact]
    public void HintNamesUseAtomUICityFeatureFolderAndStableSuffix()
    {
        var hintName = GeneratedCodeNames.CreateHintName(GeneratorFeature.Modularity, "Sample.App", "Modules");

        Assert.Equal("AtomUI.City/Modularity/Sample.App.Modules.g.cs", hintName);
    }

    [Fact]
    public void RegistrarTypeNamesUseGeneratedNamespace()
    {
        var typeName = GeneratedCodeNames.CreateRegistrarTypeName(GeneratorFeature.Routing, "RouteManifest");

        Assert.Equal("AtomUI.City.Generated.GeneratedRoutingRouteManifest", typeName.FullName);
        Assert.Equal("AtomUI.City.Generated", typeName.Namespace);
        Assert.Equal("GeneratedRoutingRouteManifest", typeName.Name);
    }
}
