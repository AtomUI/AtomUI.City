using AtomUI.City.Generators.PluginSystem;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AtomUI.City.Generators.Tests;

public sealed class PluginMetadataReaderTests
{
    [Fact]
    public void ReadReturnsNullWhenAssemblyHasNoPluginDeclaration()
    {
        var compilation = CreateCompilation(
            """
            namespace Sample.Plugin;

            public sealed class PlainType
            {
            }
            """);

        Assert.Null(PluginMetadataReader.Read(compilation));
    }

    [Fact]
    public void ReadReadsPluginManifestMetadataFromAssemblyAttributes()
    {
        var compilation = CreateCompilation(
            """
            using AtomUI.City.PluginSystem;

            [assembly: Plugin("com.company.sales", "Company.Sales.Plugin", "1.0.0",
                DisplayNameKey = "SalesPlugin.DisplayName",
                DescriptionKey = "SalesPlugin.Description",
                Publisher = "Company",
                MainAssembly = "Company.Sales.Plugin.dll",
                TargetFramework = "net10.0",
                PluginApiVersion = "1.0",
                MinHostVersion = "1.0.0",
                Unloadable = true,
                AotCompatible = false)]
            [assembly: PluginCapability("routes", Scope = new[] { "/sales/**" })]
            [assembly: ContributionManifest("routes", "manifests/routes.json", Required = true)]
            [assembly: PluginDependency("com.company.identity", VersionRange = "[1.0.0,2.0.0)")]

            namespace Sample.Plugin;

            public sealed class SalesPluginMarker
            {
            }
            """);

        var metadata = PluginMetadataReader.Read(compilation);

        Assert.NotNull(metadata);
        Assert.Equal("com.company.sales", metadata.PluginId);
        Assert.Equal("Company.Sales.Plugin", metadata.PackageId);
        Assert.Equal("1.0.0", metadata.Version);
        Assert.Equal("SalesPlugin.DisplayName", metadata.DisplayNameKey);
        Assert.Equal("Company.Sales.Plugin.dll", metadata.MainAssembly);
        Assert.Equal("routes", Assert.Single(metadata.Capabilities).Name);
        Assert.Equal("routes", Assert.Single(metadata.Contributions).Type);
        Assert.Equal("com.company.identity", Assert.Single(metadata.Dependencies).PluginId);
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat([MetadataReference.CreateFromFile(typeof(AtomUI.City.PluginSystem.PluginAttribute).Assembly.Location)])
            .DistinctBy(reference => reference.Display)
            .ToArray();

        return CSharpCompilation.Create(
            "Sample.Plugin",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
