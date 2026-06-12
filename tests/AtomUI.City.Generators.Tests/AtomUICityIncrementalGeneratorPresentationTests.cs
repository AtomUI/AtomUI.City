using AtomUI.City.Generators;
using AtomUI.City.Generators.Diagnostics;
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

    [Fact]
    public void GeneratorReportsPresentationViewManifestDiagnostics()
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

                [AtomUI.City.Presentation.ViewFor(typeof(SettingsViewModel))]
                public sealed class SettingsView
                {
                }

                [AtomUI.City.Presentation.ViewFor(typeof(SettingsViewModel))]
                public sealed class AlternateSettingsView
                {
                }
            }
            """);
        var driver = CSharpGeneratorDriver.Create(new AtomUICityIncrementalGenerator());

        var runResult = driver.RunGenerators(compilation).GetRunResult();
        var generatorResult = Assert.Single(runResult.Results);
        var diagnostic = Assert.Single(generatorResult.Diagnostics);

        Assert.Empty(generatorResult.GeneratedSources);
        Assert.Equal(GeneratorDiagnosticIds.DuplicatePresentationView, diagnostic.Id);
        Assert.Contains("Sample.App.SettingsViewModel", diagnostic.GetMessage(), StringComparison.Ordinal);
        Assert.True(diagnostic.Location.IsInSource);
    }

    [Fact]
    public void GeneratedPresentationViewRegistrarCompilesAgainstPresentationContracts()
    {
        var compilation = CreateCompilation(
            """
            namespace Sample.App
            {
                public sealed class SettingsViewModel
                {
                }

                [AtomUI.City.Presentation.ViewFor(typeof(SettingsViewModel))]
                public sealed class SettingsView
                {
                }
            }
            """,
            MetadataReference.CreateFromFile(typeof(AtomUI.City.Presentation.ViewForAttribute).Assembly.Location));
        var driver = CSharpGeneratorDriver.Create(new AtomUICityIncrementalGenerator());

        driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        Assert.Empty(generatorDiagnostics);
        Assert.Empty(outputCompilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public void GeneratedPresentationViewRegistrarCompilesViewsWithConstructorDependencies()
    {
        var compilation = CreateCompilation(
            """
            namespace Sample.App
            {
                public sealed class SettingsService
                {
                }

                public sealed class SettingsViewModel
                {
                }

                [AtomUI.City.Presentation.ViewFor(typeof(SettingsViewModel))]
                public sealed class SettingsView
                {
                    public SettingsView(SettingsService service)
                    {
                        Service = service;
                    }

                    public SettingsService Service { get; }
                }
            }
            """,
            MetadataReference.CreateFromFile(typeof(AtomUI.City.Presentation.ViewForAttribute).Assembly.Location));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AtomUICityIncrementalGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);
        var generatedSource = Assert.Single(Assert.Single(driver.GetRunResult().Results).GeneratedSources);

        Assert.Empty(generatorDiagnostics);
        Assert.Empty(outputCompilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.Contains("context.Services.GetService(typeof(global::Sample.App.SettingsService))", generatedSource.SourceText.ToString(), StringComparison.Ordinal);
    }

    private static CSharpCompilation CreateCompilation(
        string source,
        params MetadataReference[] additionalReferences)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(additionalReferences)
            .DistinctBy(reference => reference.Display)
            .ToArray();

        return CSharpCompilation.Create(
            "Sample.App",
            [sourceTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
