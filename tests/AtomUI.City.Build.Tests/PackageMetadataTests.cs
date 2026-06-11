using System.Xml.Linq;

namespace AtomUI.City.Build.Tests;

public sealed class PackageMetadataTests
{
    [Fact]
    public void PackageMetadataDefinesRequiredNuGetFields()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var packageMetadata = XDocument.Load(Path.Combine(repositoryRoot, "build", "PackageMetaInfo.props"));
        var properties = ReadProperties(packageMetadata);

        Assert.Equal("$(MSBuildProjectName)", properties["PackageId"]);
        Assert.Equal("AtomUI.City", properties["Title"]);
        Assert.Equal("Qinware Technologies Ltd.", properties["Author"]);
        Assert.Equal("$(Author)", properties["Authors"]);
        Assert.Equal("AtomUI.City is a full-stack application framework for Avalonia and AtomUI applications.", properties["Description"]);
        Assert.Equal("avalonia;AtomUI;full-stack;framework;desktop", properties["PackageTags"]);
        Assert.Equal("https://github.com/AtomUI/AtomUICity", properties["ProjectUrl"]);
        Assert.Equal("https://github.com/AtomUI/AtomUICity", properties["RepositoryUrl"]);
        Assert.Equal("LGPL-3.0-only", properties["PackageLicenseExpression"]);
        Assert.Equal("$(AtomUICityVersion)", properties["Version"]);
    }

    [Fact]
    public void LicenseFileContainsLgplV3Text()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var licenseText = File.ReadAllText(Path.Combine(repositoryRoot, "LICENSE"));

        Assert.Contains("GNU LESSER GENERAL PUBLIC LICENSE", licenseText, StringComparison.Ordinal);
        Assert.Contains("Version 3", licenseText, StringComparison.Ordinal);
    }

    [Fact]
    public void DirectoryBuildPropsImportsPackageMetadata()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var directoryBuildProps = XDocument.Load(Path.Combine(repositoryRoot, "Directory.Build.props"));
        var imports = directoryBuildProps
            .Descendants("Import")
            .Select(import => import.Attribute("Project")?.Value)
            .Where(project => !string.IsNullOrWhiteSpace(project))
            .Select(project => project!)
            .ToArray();

        Assert.Contains("$(MSBuildThisFileDirectory)/build/PackageMetaInfo.props", imports);
    }

    [Fact]
    public void ProjectPackageReferencesUseCentralPackageVersions()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var projectFiles = Directory
            .EnumerateFiles(repositoryRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(path => RepositoryPaths.ToRepositoryRelativePath(repositoryRoot, path) is var relativePath &&
                           (relativePath.StartsWith("src/", StringComparison.Ordinal) ||
                            relativePath.StartsWith("tests/", StringComparison.Ordinal)))
            .Order(StringComparer.Ordinal)
            .ToArray();

        var packageReferencesWithInlineVersions = projectFiles
            .SelectMany(projectPath => ReadPackageReferencesWithVersions(repositoryRoot, projectPath))
            .ToArray();

        Assert.Empty(packageReferencesWithInlineVersions);
    }

    private static IReadOnlyDictionary<string, string> ReadProperties(XDocument document)
    {
        return document
            .Descendants()
            .Where(element => element.Parent?.Name.LocalName == "PropertyGroup")
            .ToDictionary(element => element.Name.LocalName, element => element.Value, StringComparer.Ordinal);
    }

    private static string[] ReadPackageReferencesWithVersions(string repositoryRoot, string projectPath)
    {
        var project = XDocument.Load(projectPath);

        return project
            .Descendants("PackageReference")
            .Where(reference => reference.Attribute("Version") is not null)
            .Select(reference =>
            {
                var relativeProjectPath = RepositoryPaths.ToRepositoryRelativePath(repositoryRoot, projectPath);
                var packageId = reference.Attribute("Include")?.Value ?? "<unknown>";

                return $"{relativeProjectPath}: {packageId}";
            })
            .ToArray();
    }
}
