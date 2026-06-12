using AtomUI.City.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AtomUI.City.Generators.Tests;

public sealed class AtomUICityIncrementalGeneratorPresentationTests
{
    [Fact]
    public void GeneratorEmitsPresentationViewRegistrarSource()
    {
        var compilation = CreateCompilation(
            """
            namespace AtomUI.City.Presentation
            {
                [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
                public sealed class ViewForAttribute : System.Attribute
                {
                    public ViewForAttribute(System.Type viewModelType)
                    {
                        ViewModelType = viewModelType;
                    }

                    public System.Type ViewModelType { get; }

                    public string? Key { get; init; }

                    public string? PluginId { get; init; }

                    public string? ContributionId { get; init; }
                }
            }

            namespace Sample.App
            {
                public sealed class SettingsViewModel
                {
                }

                [AtomUI.City.Presentation.ViewFor(typeof(SettingsViewModel), Key = "settings")]
                public sealed class SettingsView
                {
                }
            }
            """);
        var driver = CSharpGeneratorDriver.Create(new AtomUICityIncrementalGenerator());

        var runResult = driver.RunGenerators(compilation).GetRunResult();
        var generatorResult = Assert.Single(runResult.Results);
        var generatedSource = Assert.Single(generatorResult.GeneratedSources);

        Assert.Equal("AtomUI.City/Presentation/Sample.App.Views.g.cs", generatedSource.HintName);
        Assert.Contains("GeneratedPresentationViewRegistrar", generatedSource.SourceText.ToString(), StringComparison.Ordinal);
        Assert.Contains("typeof(global::Sample.App.SettingsViewModel)", generatedSource.SourceText.ToString(), StringComparison.Ordinal);
        Assert.Contains("typeof(global::Sample.App.SettingsView)", generatedSource.SourceText.ToString(), StringComparison.Ordinal);
        Assert.Contains("@\"settings\"", generatedSource.SourceText.ToString(), StringComparison.Ordinal);
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .DistinctBy(reference => reference.Display)
            .ToArray();

        return CSharpCompilation.Create(
            "Sample.App",
            [sourceTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
