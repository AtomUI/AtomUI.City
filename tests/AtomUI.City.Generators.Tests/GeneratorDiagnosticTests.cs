using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.Tests;

public sealed class GeneratorDiagnosticTests
{
    [Fact]
    public void DiagnosticIdsUseAucgenPrefixAndThreeDigits()
    {
        Assert.Equal("AUCGEN001", GeneratorDiagnosticIds.DynamicDiscoveryNotAllowed);
        Assert.Equal("AUCGEN002", GeneratorDiagnosticIds.DuplicateModuleName);
        Assert.Equal("AUCGEN003", GeneratorDiagnosticIds.CircularModuleDependency);
        Assert.Equal("AUCGEN004", GeneratorDiagnosticIds.DuplicateRoute);
        Assert.Equal("AUCGEN005", GeneratorDiagnosticIds.InvalidManifestInput);
        Assert.Equal("AUCGEN006", GeneratorDiagnosticIds.DuplicatePresentationView);
    }

    [Fact]
    public void DiagnosticDefinitionsExposeStableMetadata()
    {
        var definition = GeneratorDiagnostics.DynamicDiscoveryNotAllowed;

        Assert.Equal("AUCGEN001", definition.Id);
        Assert.Equal(GeneratorDiagnosticSeverity.Error, definition.Severity);
        Assert.Contains("dynamic discovery", definition.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AllDiagnosticDefinitionsHaveUniqueIds()
    {
        var ids = GeneratorDiagnostics.All.Select(diagnostic => diagnostic.Id).ToArray();

        Assert.Equal(ids.Length, ids.Distinct(StringComparer.Ordinal).Count());
    }
}
