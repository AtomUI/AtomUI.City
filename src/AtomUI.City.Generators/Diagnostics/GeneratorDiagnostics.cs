namespace AtomUI.City.Generators.Diagnostics;

public static class GeneratorDiagnostics
{
    public static readonly GeneratorDiagnosticDefinition DynamicDiscoveryNotAllowed = new GeneratorDiagnosticDefinition(
        GeneratorDiagnosticIds.DynamicDiscoveryNotAllowed,
        "Dynamic discovery is not allowed",
        "Runtime dynamic discovery is not allowed in strict source generation mode.",
        GeneratorDiagnosticSeverity.Error);

    public static readonly GeneratorDiagnosticDefinition DuplicateModuleName = new GeneratorDiagnosticDefinition(
        GeneratorDiagnosticIds.DuplicateModuleName,
        "Duplicate module name",
        "Module names must be unique within the generated module manifest.",
        GeneratorDiagnosticSeverity.Error);

    public static readonly GeneratorDiagnosticDefinition CircularModuleDependency = new GeneratorDiagnosticDefinition(
        GeneratorDiagnosticIds.CircularModuleDependency,
        "Circular module dependency",
        "Module dependency graph contains a circular dependency.",
        GeneratorDiagnosticSeverity.Error);

    public static readonly GeneratorDiagnosticDefinition DuplicateRoute = new GeneratorDiagnosticDefinition(
        GeneratorDiagnosticIds.DuplicateRoute,
        "Duplicate route",
        "Route patterns and route names must be unique within the generated route manifest.",
        GeneratorDiagnosticSeverity.Error);

    public static readonly GeneratorDiagnosticDefinition InvalidManifestInput = new GeneratorDiagnosticDefinition(
        GeneratorDiagnosticIds.InvalidManifestInput,
        "Invalid manifest input",
        "Generator manifest input is invalid or incomplete.",
        GeneratorDiagnosticSeverity.Error);

    public static IReadOnlyList<GeneratorDiagnosticDefinition> All { get; } = new[]
    {
        DynamicDiscoveryNotAllowed,
        DuplicateModuleName,
        CircularModuleDependency,
        DuplicateRoute,
        InvalidManifestInput,
    };
}
