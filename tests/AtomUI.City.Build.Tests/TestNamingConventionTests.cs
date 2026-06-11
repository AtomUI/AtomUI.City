using System.Text.RegularExpressions;

namespace AtomUI.City.Build.Tests;

public sealed partial class TestNamingConventionTests
{
    [Fact]
    public void TestClassesUseTestsSuffix()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var violations = Directory
            .EnumerateFiles(Path.Combine(repositoryRoot, "tests"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !Path.GetFileName(path).EndsWith("AssemblyInfo.cs", StringComparison.Ordinal))
            .SelectMany(path => PublicTestClassRegex()
                .Matches(File.ReadAllText(path))
                .Select(match => new
                {
                    Path = path,
                    ClassName = match.Groups["name"].Value,
                }))
            .Where(match => !match.ClassName.EndsWith("Tests", StringComparison.Ordinal))
            .Select(match => $"{RepositoryPaths.ToRepositoryRelativePath(repositoryRoot, match.Path)}: {match.ClassName}")
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void FactMethodsUsePascalCaseNames()
    {
        var repositoryRoot = RepositoryPaths.FindRepositoryRoot();
        var violations = Directory
            .EnumerateFiles(Path.Combine(repositoryRoot, "tests"), "*.cs", SearchOption.AllDirectories)
            .SelectMany(path => FactMethodRegex()
                .Matches(File.ReadAllText(path))
                .Select(match => new
                {
                    Path = path,
                    MethodName = match.Groups["name"].Value,
                }))
            .Where(match => !PascalCaseRegex().IsMatch(match.MethodName))
            .Select(match => $"{RepositoryPaths.ToRepositoryRelativePath(repositoryRoot, match.Path)}: {match.MethodName}")
            .ToArray();

        Assert.Empty(violations);
    }

    [GeneratedRegex(@"^public\s+(?:sealed\s+|partial\s+|sealed\s+partial\s+|partial\s+sealed\s+)?class\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Multiline)]
    private static partial Regex PublicTestClassRegex();

    [GeneratedRegex(@"\[Fact\]\s+public\s+(?:async\s+)?(?:ValueTask|Task|void)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\(")]
    private static partial Regex FactMethodRegex();

    [GeneratedRegex(@"^[A-Z][A-Za-z0-9]*$")]
    private static partial Regex PascalCaseRegex();
}
