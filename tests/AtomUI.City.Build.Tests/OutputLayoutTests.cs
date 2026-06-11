using System.Xml.Linq;

namespace AtomUI.City.Build.Tests;

public sealed class OutputLayoutTests
{
    [Fact]
    public void BuildArtifactsAreLoadedFromRepositoryOutputDirectory()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var assemblyPath = typeof(OutputLayoutTests).Assembly.Location;
        var relativeAssemblyPath = RepositoryPaths.ToRepositoryRelativePath(repositoryRoot, assemblyPath);

        Assert.StartsWith("output/bin/Debug/AtomUI.City.Build.Tests/", relativeAssemblyPath, StringComparison.Ordinal);
        Assert.EndsWith("/AtomUI.City.Build.Tests.dll", relativeAssemblyPath, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildOutputPropsRedirectBinaryIntermediateAndPackageOutputs()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var outputProps = XDocument.Load(Path.Combine(repositoryRoot, "build", "Output.props"));
        var properties = outputProps
            .Descendants()
            .Where(element => element.Parent?.Name.LocalName == "PropertyGroup")
            .ToDictionary(element => element.Name.LocalName, element => element.Value, StringComparer.Ordinal);

        Assert.Equal("$(MSBuildThisFileDirectory)../output/NuGet/$(Configuration)", properties["PackageOutputPath"]);
        Assert.Equal("$(MSBuildThisFileDirectory)../output/bin/$(Configuration)/$(MSBuildProjectName)", properties["OutputPathWithoutFramework"]);
        Assert.Equal("$(OutputPathWithoutFramework)", properties["OutputPath"]);
        Assert.Equal("$(MSBuildThisFileDirectory)../output/$(MSBuildProjectName)/obj", properties["BaseIntermediateOutputPath"]);
        Assert.Equal("$(BaseIntermediateOutputPath)/$(Configuration)", properties["IntermediateOutputPath"]);
    }

    [Fact]
    public void DirectoryBuildPropsImportsOutputLayout()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var directoryBuildProps = XDocument.Load(Path.Combine(repositoryRoot, "Directory.Build.props"));
        var imports = directoryBuildProps
            .Descendants("Import")
            .Select(import => import.Attribute("Project")?.Value)
            .Where(project => !string.IsNullOrWhiteSpace(project))
            .Select(project => project!)
            .ToArray();

        Assert.Contains("$(MSBuildThisFileDirectory)/build/Output.props", imports);
    }
}
