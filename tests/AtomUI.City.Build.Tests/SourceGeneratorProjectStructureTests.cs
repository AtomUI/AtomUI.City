namespace AtomUI.City.Build.Tests;

public sealed class SourceGeneratorProjectStructureTests
{
    [Fact]
    public void GeneratorsProjectUsesDedicatedSourceGeneratorLayout()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var generatorRoot = Path.Combine(repositoryRoot, "src", "AtomUI.City.Generators");

        Assert.True(File.Exists(Path.Combine(generatorRoot, "AtomUI.City.Generators.csproj")));

        AssertDirectoryExists(generatorRoot, "Common");
        AssertDirectoryExists(generatorRoot, "Diagnostics");
        AssertDirectoryExists(generatorRoot, "EventBus");
        AssertDirectoryExists(generatorRoot, "Localization");
        AssertDirectoryExists(generatorRoot, "Manifest");
        AssertDirectoryExists(generatorRoot, "Modularity");
        AssertDirectoryExists(generatorRoot, "PluginSystem");
        AssertDirectoryExists(generatorRoot, "Presentation");
        AssertDirectoryExists(generatorRoot, "Routing");
        AssertDirectoryExists(generatorRoot, "Security");
    }

    [Fact]
    public void GeneratorsProjectHasDedicatedTestProject()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(repositoryRoot, "tests", "AtomUI.City.Generators.Tests", "AtomUI.City.Generators.Tests.csproj")));
    }

    [Fact]
    public void RuntimeProjectsDoNotReferenceGeneratorsProject()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var runtimeProjects = Directory
            .EnumerateFiles(Path.Combine(repositoryRoot, "src"), "*.csproj", SearchOption.AllDirectories)
            .Where(path => Path.GetFileNameWithoutExtension(path) is not "AtomUI.City.Build"
                and not "AtomUI.City.Cli"
                and not "AtomUI.City.Generators"
                and not "AtomUI.City.Templates"
                and not "AtomUI.City.Testing");

        foreach (var projectPath in runtimeProjects)
        {
            var text = File.ReadAllText(projectPath);

            Assert.DoesNotContain("AtomUI.City.Generators.csproj", text, StringComparison.Ordinal);
        }
    }

    private static void AssertDirectoryExists(string root, string relativePath)
    {
        Assert.True(Directory.Exists(Path.Combine(root, relativePath)), $"Expected generator directory '{relativePath}'.");
    }
}
