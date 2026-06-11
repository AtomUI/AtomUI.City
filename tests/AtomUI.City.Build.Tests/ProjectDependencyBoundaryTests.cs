using System.Xml.Linq;

namespace AtomUI.City.Build.Tests;

public sealed class ProjectDependencyBoundaryTests
{
    private static readonly IReadOnlyDictionary<string, string[]> AllowedSourceProjectReferences = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["AtomUI.City.Build"] = [],
        ["AtomUI.City.Cli"] = ["AtomUI.City.Build", "AtomUI.City.Core"],
        ["AtomUI.City.Core"] = [],
        ["AtomUI.City.Data"] = ["AtomUI.City.Core", "AtomUI.City.Security", "AtomUI.City.State"],
        ["AtomUI.City.EventBus"] = ["AtomUI.City.Core"],
        ["AtomUI.City.Generators"] = [],
        ["AtomUI.City.Localization"] = ["AtomUI.City.Core", "AtomUI.City.State"],
        ["AtomUI.City.Mvvm"] = ["AtomUI.City.Core"],
        ["AtomUI.City.PluginSystem"] = ["AtomUI.City.Core"],
        ["AtomUI.City.Presentation"] = ["AtomUI.City.Core", "AtomUI.City.Localization", "AtomUI.City.Mvvm", "AtomUI.City.Routing", "AtomUI.City.Security", "AtomUI.City.State"],
        ["AtomUI.City.Routing"] = ["AtomUI.City.Core", "AtomUI.City.State"],
        ["AtomUI.City.Security"] = ["AtomUI.City.Core", "AtomUI.City.Routing", "AtomUI.City.State"],
        ["AtomUI.City.State"] = ["AtomUI.City.Core"],
        ["AtomUI.City.Templates"] = [],
        ["AtomUI.City.Testing"] = ["AtomUI.City.Core", "AtomUI.City.Data", "AtomUI.City.EventBus", "AtomUI.City.Localization", "AtomUI.City.Mvvm", "AtomUI.City.PluginSystem", "AtomUI.City.Presentation", "AtomUI.City.Routing", "AtomUI.City.Security", "AtomUI.City.State"],
    };

    private static readonly HashSet<string> ForbiddenRuntimePackageReferences = new(StringComparer.Ordinal)
    {
        "Microsoft.CodeAnalysis",
        "Microsoft.CodeAnalysis.CSharp",
        "Microsoft.NET.Test.Sdk",
        "ReactiveUI",
        "Spectre.Console",
        "System.Reactive",
        "xunit",
        "xunit.runner.visualstudio",
    };

    [Fact]
    public void SourceProjectReferencesMatchAllowedDependencyBoundaries()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var sourceProjects = Directory
            .EnumerateFiles(Path.Combine(repositoryRoot, "src"), "*.csproj", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var sourceProjectNames = sourceProjects
            .Select(Path.GetFileNameWithoutExtension)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(
            AllowedSourceProjectReferences.Keys.Order(StringComparer.Ordinal),
            sourceProjectNames.Order(StringComparer.Ordinal));

        foreach (var projectPath in sourceProjects)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            var actualReferences = ReadProjectReferences(projectPath)
                .Select(Path.GetFileNameWithoutExtension)
                .Order(StringComparer.Ordinal)
                .ToArray();
            var expectedReferences = AllowedSourceProjectReferences[projectName]
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(expectedReferences, actualReferences);
        }
    }

    [Fact]
    public void SourceProjectsDoNotReferenceTestProjects()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var sourceProjects = Directory.EnumerateFiles(Path.Combine(repositoryRoot, "src"), "*.csproj", SearchOption.AllDirectories);

        foreach (var projectPath in sourceProjects)
        {
            var testReferences = ReadProjectReferences(projectPath)
                .Where(reference => reference.Contains("/tests/", StringComparison.Ordinal) ||
                                    Path.GetFileNameWithoutExtension(reference).EndsWith(".Tests", StringComparison.Ordinal) ||
                                    Path.GetFileNameWithoutExtension(reference).EndsWith("SmokeTests", StringComparison.Ordinal))
                .ToArray();

            Assert.Empty(testReferences);
        }
    }

    [Fact]
    public void RuntimeProjectsDoNotReferenceBuildCliGeneratorOrTestPackages()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var runtimeProjects = Directory
            .EnumerateFiles(Path.Combine(repositoryRoot, "src"), "*.csproj", SearchOption.AllDirectories)
            .Where(path => IsRuntimeProject(Path.GetFileNameWithoutExtension(path)));

        foreach (var projectPath in runtimeProjects)
        {
            var forbiddenReferences = ReadPackageReferences(projectPath)
                .Where(packageId => ForbiddenRuntimePackageReferences.Contains(packageId))
                .ToArray();

            Assert.Empty(forbiddenReferences);
        }
    }

    private static string[] ReadProjectReferences(string projectPath)
    {
        var projectDirectory = Path.GetDirectoryName(projectPath)!;
        var project = XDocument.Load(projectPath);

        return project
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => Path.GetFullPath(include!, projectDirectory).Replace('\\', '/'))
            .ToArray();
    }

    private static string[] ReadPackageReferences(string projectPath)
    {
        var project = XDocument.Load(projectPath);

        return project
            .Descendants("PackageReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(packageId => !string.IsNullOrWhiteSpace(packageId))
            .Select(packageId => packageId!)
            .ToArray();
    }

    private static bool IsRuntimeProject(string projectName)
    {
        return projectName is not "AtomUI.City.Build"
            and not "AtomUI.City.Cli"
            and not "AtomUI.City.Generators"
            and not "AtomUI.City.Templates"
            and not "AtomUI.City.Testing";
    }
}
