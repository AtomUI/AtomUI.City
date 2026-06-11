namespace AtomUI.City.Testing;

public sealed class SourceGenerationTestCase
{
    private readonly List<ExpectedDiagnostic> _expectedDiagnostics = [];
    private readonly List<SourceFile> _sources = [];

    private SourceGenerationTestCase(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public IReadOnlyList<SourceFile> Sources => _sources;

    public IReadOnlyList<ExpectedDiagnostic> ExpectedDiagnostics => _expectedDiagnostics;

    public static SourceGenerationTestCase Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new SourceGenerationTestCase(name);
    }

    public SourceGenerationTestCase AddSource(string path, string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(text);

        _sources.Add(new SourceFile(path, text));

        return this;
    }

    public SourceGenerationTestCase ExpectDiagnostic(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        _expectedDiagnostics.Add(new ExpectedDiagnostic(id));

        return this;
    }
}
