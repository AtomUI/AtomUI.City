using System.Text.RegularExpressions;

namespace AtomUI.City.Build.Tests;

public sealed class EngineeringGateTests
{
    private const string EngineeringScriptsDirectoryName = "engineering";

    [Fact]
    public void EditorConfigDefinesRepositoryFormattingPolicy()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var editorConfigPath = Path.Combine(repositoryRoot, ".editorconfig");

        Assert.True(File.Exists(editorConfigPath), "Expected a repository-level .editorconfig file.");

        var editorConfig = File.ReadAllText(editorConfigPath);

        Assert.Contains("root = true", editorConfig, StringComparison.Ordinal);
        Assert.Contains("indent_style = space", editorConfig, StringComparison.Ordinal);
        Assert.Contains("indent_size = 4", editorConfig, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet_diagnostic.IDE0073", editorConfig, StringComparison.Ordinal);
        Assert.DoesNotContain("file_header_template", editorConfig, StringComparison.Ordinal);
    }

    [Fact]
    public void ContinuousIntegrationWorkflowRunsRequiredEngineeringGates()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var workflowPath = Path.Combine(repositoryRoot, ".github", "workflows", "ci.yml");

        Assert.True(File.Exists(workflowPath), "Expected GitHub Actions CI workflow at .github/workflows/ci.yml.");

        var workflow = File.ReadAllText(workflowPath);

        Assert.Contains("dotnet restore AtomUICity.slnx", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet build AtomUICity.slnx --no-restore", workflow, StringComparison.Ordinal);
        Assert.Contains("bash engineering/test-ci.sh", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet format AtomUICity.slnx --verify-no-changes --no-restore", workflow, StringComparison.Ordinal);
        Assert.Contains("bash engineering/check-license.sh", workflow, StringComparison.Ordinal);
        Assert.Contains("bash engineering/check-docs.sh", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void RepositoryUsesDescriptiveEngineeringScriptDirectoryName()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var abbreviatedDirectoryPath = Path.Combine(repositoryRoot, "e" + "ng");
        var engineeringDirectoryPath = Path.Combine(repositoryRoot, EngineeringScriptsDirectoryName);

        Assert.False(Directory.Exists(abbreviatedDirectoryPath), "Use engineering/ instead of the abbreviated engineering/ directory.");
        Assert.True(Directory.Exists(engineeringDirectoryPath), "Expected engineering scripts at engineering/.");
    }

    [Fact]
    public void ContinuousIntegrationTestScriptAppliesTestCategoryPolicy()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, EngineeringScriptsDirectoryName, "test-ci.sh");

        Assert.True(File.Exists(scriptPath), "Expected CI test script at engineering/test-ci.sh.");

        var script = File.ReadAllText(scriptPath);

        Assert.Contains("dotnet test AtomUICity.slnx --no-build", script, StringComparison.Ordinal);
        Assert.Contains("Category!=PlatformIntegration", script, StringComparison.Ordinal);
    }

    [Fact]
    public void CentralizedLicenseCheckScriptExists()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, EngineeringScriptsDirectoryName, "check-license.sh");

        Assert.True(File.Exists(scriptPath), "Expected centralized license check script at engineering/check-license.sh.");

        var script = File.ReadAllText(scriptPath);

        Assert.Contains("LICENSE", script, StringComparison.Ordinal);
        Assert.Contains("LGPL-3.0-only", script, StringComparison.Ordinal);
        Assert.Contains("PackageLicenseExpression", script, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentationCheckScriptExists()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, EngineeringScriptsDirectoryName, "check-docs.sh");

        Assert.True(File.Exists(scriptPath), "Expected documentation check script at engineering/check-docs.sh.");

        var script = File.ReadAllText(scriptPath);

        Assert.Contains("README", script, StringComparison.Ordinal);
        Assert.Contains("odd code fences", script, StringComparison.Ordinal);
        Assert.Contains("missing markdown links", script, StringComparison.Ordinal);
    }

    [Fact]
    public void PackScriptAvoidsNounsetUnsafeOptionalArrayExpansion()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, EngineeringScriptsDirectoryName, "pack.sh");

        Assert.True(File.Exists(scriptPath), "Expected package script at engineering/pack.sh.");

        var script = File.ReadAllText(scriptPath);

        Assert.DoesNotContain("\"${no_build[@]}\"", script, StringComparison.Ordinal);
    }

    [Fact]
    public void CSharpSourceFilesDoNotUseRepositoryLicenseHeaders()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var csharpFiles = Directory
            .EnumerateFiles(Path.Combine(repositoryRoot, "src"), "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(Path.Combine(repositoryRoot, "tests"), "*.cs", SearchOption.AllDirectories))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(csharpFiles);

        var filesWithRepositoryLicenseHeaders = csharpFiles
            .Where(path => File.ReadLines(path).FirstOrDefault()?.StartsWith("// Licensed under the GNU Lesser General Public License", StringComparison.Ordinal) is true)
            .Select(path => RepositoryPaths.ToRepositoryRelativePath(repositoryRoot, path))
            .ToArray();

        Assert.Empty(filesWithRepositoryLicenseHeaders);
    }

    [Fact]
    public void MsBuildIncludePathsUseForwardSlashSeparators()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var msbuildFiles = Directory
            .EnumerateFiles(repositoryRoot, "*.*", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}.referenceprojects{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path =>
                path.EndsWith(".csproj", StringComparison.Ordinal) ||
                path.EndsWith(".props", StringComparison.Ordinal) ||
                path.EndsWith(".targets", StringComparison.Ordinal) ||
                path.EndsWith(".slnx", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();

        var includePathsWithBackslashes = msbuildFiles
            .SelectMany(path => FindIncludePathsWithBackslashes(repositoryRoot, path))
            .ToArray();

        Assert.Empty(includePathsWithBackslashes);
    }

    private static IEnumerable<string> FindIncludePathsWithBackslashes(string repositoryRoot, string path)
    {
        var content = File.ReadAllText(path);
        var relativePath = RepositoryPaths.ToRepositoryRelativePath(repositoryRoot, path);

        foreach (Match match in Regex.Matches(content, "Include=\"(?<value>[^\"]*\\\\[^\"]*)\"", RegexOptions.CultureInvariant))
        {
            yield return $"{relativePath}: {match.Groups["value"].Value}";
        }
    }
}
