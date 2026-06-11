using AtomUI.City.Testing;

namespace AtomUI.City.Testing.Tests;

public sealed class AotCompatibilityCheckTests
{
    [Fact]
    public void EvaluateReportsForbiddenRuntimeReflectionPatterns()
    {
        var check = AotCompatibilityCheck
            .Create()
            .ForbidPattern("AOT001", "Assembly.GetTypes");

        var diagnostics = check.Evaluate(
            [
                new SourceFile("ModuleScanner.cs", "var types = assembly.Assembly.GetTypes();"),
                new SourceFile("StaticManifest.cs", "var manifest = StaticManifest.Instance;"),
            ]);

        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal("AOT001", diagnostic.Id);
        Assert.Equal("ModuleScanner.cs", diagnostic.SourcePath);
        Assert.Contains("Assembly.GetTypes", diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EvaluateReturnsNoDiagnosticsWhenSourcesAvoidForbiddenPatterns()
    {
        var check = AotCompatibilityCheck
            .Create()
            .ForbidPattern("AOT001", "Assembly.GetTypes");

        var diagnostics = check.Evaluate([new SourceFile("StaticManifest.cs", "var manifest = StaticManifest.Instance;")]);

        Assert.Empty(diagnostics);
    }
}
