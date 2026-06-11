using AtomUI.City.Generators.Modularity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.City.Generators.Tests;

public sealed class ModuleMetadataReaderTests
{
    [Fact]
    public void TryReadUsesFullTypeNameWhenModuleAttributeDoesNotDeclareName()
    {
        var module = ReadSingleModule(
            """
            using AtomUI.City.Modularity;

            namespace Sample.App;

            [Module(Version = "1.0.0", Description = "Application module")]
            public sealed class AppModule : ModuleBase
            {
            }
            """);

        Assert.Equal("Sample.App.AppModule", module.Name);
        Assert.Equal("Sample.App.AppModule", module.TypeName);
        Assert.Equal("1.0.0", module.Version);
        Assert.Equal("Application module", module.Description);
    }

    [Fact]
    public void TryReadUsesExplicitModuleNameWhenDeclared()
    {
        var module = ReadSingleModule(
            """
            using AtomUI.City.Modularity;

            namespace Sample.App;

            [Module("Sample.PublicModule")]
            public sealed class AppModule : ModuleBase
            {
            }
            """);

        Assert.Equal("Sample.PublicModule", module.Name);
        Assert.Equal("Sample.App.AppModule", module.TypeName);
    }

    [Fact]
    public void TryReadReadsDependsOnAttributes()
    {
        var module = ReadSingleModule(
            """
            using AtomUI.City.Modularity;

            namespace Sample.App;

            public sealed class CoreModule : ModuleBase
            {
            }

            [DependsOn(typeof(CoreModule))]
            [DependsOn(typeof(OptionalModule), Optional = true)]
            public sealed class AppModule : ModuleBase
            {
            }

            public sealed class OptionalModule : ModuleBase
            {
            }
            """,
            "Sample.App.AppModule");

        Assert.Collection(
            module.Dependencies,
            dependency =>
            {
                Assert.Equal("Sample.App.CoreModule", dependency.TypeName);
                Assert.False(dependency.Optional);
            },
            dependency =>
            {
                Assert.Equal("Sample.App.OptionalModule", dependency.TypeName);
                Assert.True(dependency.Optional);
            });
    }

    [Fact]
    public void TryReadReturnsNullForNonModuleTypes()
    {
        var compilation = CreateCompilation(
            """
            namespace Sample.App;

            public sealed class NotAModule
            {
            }
            """);
        var type = GetTypeSymbol(compilation, "Sample.App.NotAModule");

        Assert.Null(ModuleMetadataReader.TryRead(type));
    }

    private static AtomUI.City.Generators.Modularity.ModuleMetadata ReadSingleModule(string source, string typeName = "Sample.App.AppModule")
    {
        var compilation = CreateCompilation(source);
        var type = GetTypeSymbol(compilation, typeName);
        var metadata = ModuleMetadataReader.TryRead(type);

        Assert.NotNull(metadata);

        return metadata;
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat([MetadataReference.CreateFromFile(typeof(AtomUI.City.Modularity.ModuleBase).Assembly.Location)])
            .DistinctBy(reference => reference.Display)
            .ToArray();

        return CSharpCompilation.Create(
            "Sample.App",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static INamedTypeSymbol GetTypeSymbol(Compilation compilation, string typeName)
    {
        var syntaxTree = compilation.SyntaxTrees.Single();
        var declaration = syntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(type => string.Equals(type.Identifier.ValueText, typeName.Split('.').Last(), StringComparison.Ordinal));
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        return semanticModel.GetDeclaredSymbol(declaration)!;
    }
}
