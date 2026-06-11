using AtomUI.City.Modularity;

namespace AtomUI.City.Core.Tests;

public sealed class ModuleAttributeTests
{
    [Fact]
    public void ModuleAttributeCanOverrideMetadataWithoutRequiringName()
    {
        var attribute = new ModuleAttribute
        {
            Version = "1.0.0",
            Description = "Routing module",
        };

        Assert.Null(attribute.Name);
        Assert.Equal("1.0.0", attribute.Version);
        Assert.Equal("Routing module", attribute.Description);
    }

    [Fact]
    public void ModuleAttributeCanDeclareStablePublicName()
    {
        var attribute = new ModuleAttribute("AtomUI.City.Routing");

        Assert.Equal("AtomUI.City.Routing", attribute.Name);
    }

    [Fact]
    public void DependsOnAttributeStoresRequiredModuleTypeAndOptionalFlag()
    {
        var attribute = new DependsOnAttribute(typeof(DependencyModule))
        {
            Optional = true,
        };

        Assert.Equal(typeof(DependencyModule), attribute.ModuleType);
        Assert.True(attribute.Optional);
    }

    private sealed class DependencyModule : ModuleBase;
}
