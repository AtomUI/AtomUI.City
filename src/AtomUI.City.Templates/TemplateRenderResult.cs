namespace AtomUI.City.Templates;

public sealed class TemplateRenderResult
{
    private TemplateRenderResult(TemplatePlan? plan, IReadOnlyList<TemplateDiagnostic> diagnostics)
    {
        Plan = plan;
        Diagnostics = diagnostics.ToArray();
    }

    public TemplatePlan? Plan { get; }

    public IReadOnlyList<TemplateDiagnostic> Diagnostics { get; }

    public bool Succeeded => Diagnostics.Count == 0;

    public static TemplateRenderResult Success(TemplatePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return new TemplateRenderResult(plan, []);
    }

    public static TemplateRenderResult Failed(params TemplateDiagnostic[] diagnostics)
    {
        return new TemplateRenderResult(null, diagnostics);
    }
}

public sealed record TemplateDiagnostic(string Code, string Message);
