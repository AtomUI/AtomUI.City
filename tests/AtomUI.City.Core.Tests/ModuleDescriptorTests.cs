using AtomUI.City.Modularity;

namespace AtomUI.City.Core.Tests;

public sealed class ModuleDescriptorTests
{
    [Fact]
    public void DependenciesRejectExternalListMutation()
    {
        var sourceDependencies = new List<ModuleDependencyDescriptor>
        {
            new(typeof(DependencyModule), optional: false),
        };
        var descriptor = new ModuleDescriptor(
            "TestModule",
            typeof(TestModule),
            version: null,
            description: null,
            sourceDependencies);
        var dependencies = Assert.IsAssignableFrom<IList<ModuleDependencyDescriptor>>(descriptor.Dependencies);

        Assert.Throws<NotSupportedException>(() => dependencies[0] = new ModuleDependencyDescriptor(
            typeof(ReplacementModule),
            optional: true));
        Assert.Equal(typeof(DependencyModule), descriptor.Dependencies[0].ModuleType);
        Assert.False(descriptor.Dependencies[0].Optional);
    }

    private sealed class TestModule : ModuleBase;

    private sealed class DependencyModule : ModuleBase;

    private sealed class ReplacementModule : ModuleBase;
}
