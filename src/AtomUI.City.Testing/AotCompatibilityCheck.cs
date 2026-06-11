namespace AtomUI.City.Testing;

public sealed class AotCompatibilityCheck
{
    private readonly List<ForbiddenAotPattern> _forbiddenPatterns = [];

    private AotCompatibilityCheck()
    {
    }

    public static AotCompatibilityCheck Create()
    {
        return new AotCompatibilityCheck();
    }

    public AotCompatibilityCheck ForbidPattern(string diagnosticId, string pattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(diagnosticId);
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        _forbiddenPatterns.Add(new ForbiddenAotPattern(diagnosticId, pattern));

        return this;
    }

    public IReadOnlyList<AotCompatibilityDiagnostic> Evaluate(IEnumerable<SourceFile> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var diagnostics = new List<AotCompatibilityDiagnostic>();

        foreach (var source in sources)
        {
            foreach (var pattern in _forbiddenPatterns)
            {
                if (source.Text.Contains(pattern.Pattern, StringComparison.Ordinal))
                {
                    diagnostics.Add(new AotCompatibilityDiagnostic(
                        pattern.DiagnosticId,
                        source.Path,
                        $"Source '{source.Path}' uses forbidden AOT pattern '{pattern.Pattern}'."));
                }
            }
        }

        return diagnostics;
    }

    private sealed record ForbiddenAotPattern(string DiagnosticId, string Pattern);
}
