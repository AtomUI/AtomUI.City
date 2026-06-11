using AtomUI.City.Generators.Diagnostics;
using AtomUI.City.Generators.Modularity;

namespace AtomUI.City.Generators.Tests;

public sealed class ModuleDependencyGraphBuilderTests
{
    [Fact]
    public void BuildOrdersDependenciesBeforeDependents()
    {
        var result = ModuleDependencyGraphBuilder.Build(
            [
                Module("Sample.App.AppModule", [Dependency("Sample.App.CoreModule")]),
                Module("Sample.App.CoreModule"),
            ]);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(
            ["Sample.App.CoreModule", "Sample.App.AppModule"],
            result.OrderedModules.Select(module => module.TypeName));
    }

    [Fact]
    public void BuildReportsDuplicateModuleNames()
    {
        var result = ModuleDependencyGraphBuilder.Build(
            [
                Module("Sample.App.FirstModule", name: "Sample.Duplicate"),
                Module("Sample.App.SecondModule", name: "Sample.Duplicate"),
            ]);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.DuplicateModuleName, diagnostic.Id);
    }

    [Fact]
    public void BuildReportsCircularDependencies()
    {
        var result = ModuleDependencyGraphBuilder.Build(
            [
                Module("Sample.App.FirstModule", [Dependency("Sample.App.SecondModule")]),
                Module("Sample.App.SecondModule", [Dependency("Sample.App.FirstModule")]),
            ]);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.CircularModuleDependency, diagnostic.Id);
    }

    [Fact]
    public void BuildReportsMissingRequiredDependencies()
    {
        var result = ModuleDependencyGraphBuilder.Build(
            [
                Module("Sample.App.AppModule", [Dependency("Sample.App.MissingModule")]),
            ]);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.InvalidManifestInput, diagnostic.Id);
        Assert.Empty(result.OrderedModules);
    }

    [Fact]
    public void BuildIgnoresMissingOptionalDependencies()
    {
        var result = ModuleDependencyGraphBuilder.Build(
            [
                Module("Sample.App.AppModule", [Dependency("Sample.App.OptionalModule", optional: true)]),
            ]);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(["Sample.App.AppModule"], result.OrderedModules.Select(module => module.TypeName));
    }

    private static ModuleDependencyMetadata Dependency(string typeName, bool optional = false)
    {
        return new ModuleDependencyMetadata(typeName, optional);
    }

    private static ModuleMetadata Module(
        string typeName,
        IReadOnlyList<ModuleDependencyMetadata>? dependencies = null,
        string? name = null)
    {
        return new ModuleMetadata(name ?? typeName, typeName, null, null, dependencies ?? []);
    }
}
