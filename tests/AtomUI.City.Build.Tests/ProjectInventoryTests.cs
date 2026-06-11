using System.Xml.Linq;

namespace AtomUI.City.Build.Tests;

public sealed class ProjectInventoryTests
{
    [Fact]
    public void SolutionIncludesEverySourceAndTestProject()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var solutionProjects = ReadSolutionProjects(repositoryRoot);
        var projectFiles = Directory
            .EnumerateFiles(repositoryRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(path => IsRepositoryProject(repositoryRoot, path))
            .Select(path => RepositoryPaths.ToRepositoryRelativePath(repositoryRoot, path))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(projectFiles, solutionProjects);
    }

    [Fact]
    public void EverySourceProjectHasAMatchingTestProject()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var testProjectNames = Directory
            .EnumerateFiles(Path.Combine(repositoryRoot, "tests"), "*.csproj", SearchOption.AllDirectories)
            .Select(path => Path.GetFileNameWithoutExtension(path))
            .ToHashSet(StringComparer.Ordinal);

        var expectedTestProjectNames = Directory
            .EnumerateFiles(Path.Combine(repositoryRoot, "src"), "*.csproj", SearchOption.AllDirectories)
            .Select(path => Path.GetFileNameWithoutExtension(path))
            .Select(GetExpectedTestProjectName)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.All(expectedTestProjectNames, testProjectName => Assert.Contains(testProjectName, testProjectNames));
    }

    [Fact]
    public void SolutionDoesNotIncludeReferenceProjects()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var solutionProjects = ReadSolutionProjects(repositoryRoot);

        Assert.DoesNotContain(solutionProjects, project => project.StartsWith(".referenceprojects/", StringComparison.Ordinal));
    }

    private static string[] ReadSolutionProjects(string repositoryRoot)
    {
        var solutionPath = Path.Combine(repositoryRoot, "AtomUICity.slnx");
        var solution = XDocument.Load(solutionPath);

        return solution
            .Descendants("Project")
            .Select(project => project.Attribute("Path")?.Value)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path!.Replace('\\', '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsRepositoryProject(string repositoryRoot, string projectPath)
    {
        var relativePath = RepositoryPaths.ToRepositoryRelativePath(repositoryRoot, projectPath);

        return relativePath.StartsWith("src/", StringComparison.Ordinal) ||
               relativePath.StartsWith("tests/", StringComparison.Ordinal);
    }

    private static string GetExpectedTestProjectName(string sourceProjectName)
    {
        return sourceProjectName switch
        {
            "AtomUI.City.Templates" => "AtomUI.City.TemplateSmokeTests",
            _ => $"{sourceProjectName}.Tests",
        };
    }
}
