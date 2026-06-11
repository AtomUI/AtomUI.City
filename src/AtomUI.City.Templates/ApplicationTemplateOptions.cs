namespace AtomUI.City.Templates;

public sealed class ApplicationTemplateOptions
{
    public required string AppName { get; init; }

    public required string RootNamespace { get; init; }

    public required string OutputPath { get; init; }

    public string TargetFramework { get; init; } = "net10.0";

    public bool IncludeTests { get; init; } = true;

    public bool UseAot { get; init; }

    public bool UseDynamicPlugins { get; init; }

    public bool IncludeSample { get; init; }
}
