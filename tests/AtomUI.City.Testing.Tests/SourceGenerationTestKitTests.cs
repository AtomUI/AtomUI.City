using AtomUI.City.Testing;

namespace AtomUI.City.Testing.Tests;

public sealed class SourceGenerationTestKitTests
{
    [Fact]
    public void GeneratedSourceSnapshotOrdersSourcesAndNormalizesLineEndings()
    {
        var snapshot = GeneratedSourceSnapshot.Create(
            [
                new GeneratedSource("B.g.cs", "namespace B\r\n{\r\n}\r\n"),
                new GeneratedSource("A.g.cs", "namespace A\n{\n}\n"),
            ]);

        Assert.Equal(
            """
            // <generated-source hint="A.g.cs">
            namespace A
            {
            }
            // </generated-source>
            // <generated-source hint="B.g.cs">
            namespace B
            {
            }
            // </generated-source>
            """,
            snapshot.Text);
    }

    [Fact]
    public void SourceGenerationTestCaseStoresCompilationInputsAndExpectedDiagnostics()
    {
        var testCase = SourceGenerationTestCase
            .Create("module manifest")
            .AddSource("Module.cs", "public sealed class TestModule {}")
            .ExpectDiagnostic("AUCGEN001");

        Assert.Equal("module manifest", testCase.Name);
        Assert.Collection(testCase.Sources, source => Assert.Equal("Module.cs", source.Path));
        Assert.Collection(testCase.ExpectedDiagnostics, diagnostic => Assert.Equal("AUCGEN001", diagnostic.Id));
    }

    [Fact]
    public void SourceGenerationTestCaseCollectionsRejectExternalMutation()
    {
        var testCase = SourceGenerationTestCase
            .Create("module manifest")
            .AddSource("Module.cs", "public sealed class TestModule {}")
            .ExpectDiagnostic("AUCGEN001");

        var sources = Assert.IsAssignableFrom<IList<SourceFile>>(testCase.Sources);
        var expectedDiagnostics = Assert.IsAssignableFrom<IList<ExpectedDiagnostic>>(testCase.ExpectedDiagnostics);

        Assert.Throws<NotSupportedException>(() => sources.Add(new SourceFile("Other.cs", "public sealed class Other {}")));
        Assert.Throws<NotSupportedException>(() => expectedDiagnostics.Add(new ExpectedDiagnostic("AUCGEN002")));
        Assert.Single(testCase.Sources);
        Assert.Single(testCase.ExpectedDiagnostics);
    }
}
