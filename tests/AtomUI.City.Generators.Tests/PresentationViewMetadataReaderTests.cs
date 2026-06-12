using AtomUI.City.Generators.Presentation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.City.Generators.Tests;

public sealed class PresentationViewMetadataReaderTests
{
    [Fact]
    public void ReadReturnsEmptyForTypesWithoutViewForAttribute()
    {
        var compilation = CreateCompilation(
            """
            namespace Sample.App;

            public sealed class PlainView
            {
            }
            """);
        var type = GetTypeSymbol(compilation, "Sample.App.PlainView");

        Assert.Empty(PresentationViewMetadataReader.Read(type));
    }

    [Fact]
    public void ReadExtractsViewForMetadata()
    {
        var views = ReadViews(
            """
            using AtomUI.City.Presentation;

            namespace Sample.App;

            public sealed class SettingsViewModel
            {
            }

            [ViewFor(typeof(SettingsViewModel), Key = "settings", PluginId = "com.company.sales", ContributionId = "plugin.settings")]
            public sealed class SettingsView
            {
            }
            """);

        var view = Assert.Single(views);

        Assert.Equal("Sample.App.SettingsView", view.ViewTypeName);
        Assert.Equal("Sample.App.SettingsViewModel", view.ViewModelTypeName);
        Assert.Equal("settings", view.ViewKey);
        Assert.Equal("com.company.sales", view.PluginId);
        Assert.Equal("plugin.settings", view.ContributionId);
    }

    [Fact]
    public void ReadSupportsMultipleViewForAttributesOnTheSameView()
    {
        var views = ReadViews(
            """
            using AtomUI.City.Presentation;

            namespace Sample.App;

            public sealed class SettingsViewModel
            {
            }

            public sealed class DetailsViewModel
            {
            }

            [ViewFor(typeof(SettingsViewModel))]
            [ViewFor(typeof(DetailsViewModel), Key = "details")]
            public sealed class SharedView
            {
            }
            """,
            viewTypeName: "Sample.App.SharedView");

        Assert.Collection(
            views,
            view =>
            {
                Assert.Equal("Sample.App.SharedView", view.ViewTypeName);
                Assert.Equal("Sample.App.SettingsViewModel", view.ViewModelTypeName);
                Assert.Null(view.ViewKey);
            },
            view =>
            {
                Assert.Equal("Sample.App.SharedView", view.ViewTypeName);
                Assert.Equal("Sample.App.DetailsViewModel", view.ViewModelTypeName);
                Assert.Equal("details", view.ViewKey);
            });
    }

    [Fact]
    public void ReadExtractsViewConstructorParameters()
    {
        var views = ReadViews(
            """
            using AtomUI.City.Presentation;

            namespace Sample.App;

            public sealed class SettingsService
            {
            }

            public sealed class SettingsViewModel
            {
            }

            [ViewFor(typeof(SettingsViewModel))]
            public sealed class SettingsView
            {
                public SettingsView(SettingsService service)
                {
                    Service = service;
                }

                public SettingsService Service { get; }
            }
            """);

        var view = Assert.Single(views);
        var parameter = Assert.Single(view.ConstructorParameters);

        Assert.Equal("Sample.App.SettingsService", parameter.TypeName);
    }

    private static IReadOnlyList<PresentationViewMetadata> ReadViews(
        string source,
        string viewTypeName = "Sample.App.SettingsView")
    {
        var compilation = CreateCompilation(source);
        var type = GetTypeSymbol(compilation, viewTypeName);

        return PresentationViewMetadataReader.Read(type);
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var sourceTree = CSharpSyntaxTree.ParseText(source);
        var attributeTree = CSharpSyntaxTree.ParseText(ViewForAttributeSource);
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .DistinctBy(reference => reference.Display)
            .ToArray();

        return CSharpCompilation.Create(
            "Sample.App",
            [sourceTree, attributeTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static INamedTypeSymbol GetTypeSymbol(Compilation compilation, string typeName)
    {
        var syntaxTree = compilation.SyntaxTrees.First();
        var declaration = syntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Single(type => string.Equals(type.Identifier.ValueText, typeName.Split('.').Last(), StringComparison.Ordinal));
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        return (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(declaration)!;
    }

    private const string ViewForAttributeSource =
        """

        namespace AtomUI.City.Presentation;

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
        """;
}
