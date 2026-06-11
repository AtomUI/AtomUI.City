using AtomUI.City.Generators.Localization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AtomUI.City.Generators.Tests;

public sealed class LocalizationMetadataReaderTests
{
    [Fact]
    public void ReadReturnsEmptyMetadataWhenAssemblyHasNoLocalizationDeclarations()
    {
        var compilation = CreateCompilation(
            """
            namespace Sample.App;

            public sealed class EmptyType
            {
            }
            """);

        var metadata = LocalizationMetadataReader.Read(compilation);

        Assert.Empty(metadata.Packages);
        Assert.Empty(metadata.Resources);
    }

    [Fact]
    public void ReadReadsLanguagePackagesAndLocalizedResourcesFromAssemblyAttributes()
    {
        var compilation = CreateCompilation(
            """
            using AtomUI.City.Localization;

            [assembly: LanguagePackage("Settings.zh-CN", "zh-CN", Scope = ResourceScope.Module, ResourceBaseName = "Sample.App.Resources.Settings", FallbackCulture = "zh-Hans")]
            [assembly: LocalizedResource("Settings.Title", "Settings.zh-CN", Kind = LocalizedResourceKind.String, Scope = ResourceScope.Module, Critical = true)]

            namespace Sample.App;

            public sealed class SettingsResources
            {
            }
            """);

        var metadata = LocalizationMetadataReader.Read(compilation);
        var package = Assert.Single(metadata.Packages);
        var resource = Assert.Single(metadata.Resources);

        Assert.Equal("Settings.zh-CN", package.PackageId);
        Assert.Equal("zh-CN", package.Culture);
        Assert.Equal(ResourceScopeMetadata.Module, package.Scope);
        Assert.Equal("Sample.App.Resources.Settings", package.ResourceBaseName);
        Assert.Equal("zh-Hans", package.FallbackCulture);
        Assert.Equal("Settings.Title", resource.Key);
        Assert.Equal("Settings.zh-CN", resource.PackageId);
        Assert.Equal(LocalizedResourceMetadataKind.String, resource.Kind);
        Assert.True(resource.Critical);
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat([MetadataReference.CreateFromFile(typeof(AtomUI.City.Localization.LanguagePackageAttribute).Assembly.Location)])
            .DistinctBy(reference => reference.Display)
            .ToArray();

        return CSharpCompilation.Create(
            "Sample.App",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
