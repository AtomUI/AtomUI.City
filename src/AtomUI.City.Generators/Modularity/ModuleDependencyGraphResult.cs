using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.Modularity;

public sealed class ModuleDependencyGraphResult
{
    public ModuleDependencyGraphResult(
        IReadOnlyList<ModuleMetadata> orderedModules,
        IReadOnlyList<GeneratorDiagnostic> diagnostics)
    {
        OrderedModules = Array.AsReadOnly((orderedModules ?? throw new ArgumentNullException(nameof(orderedModules))).ToArray());
        Diagnostics = Array.AsReadOnly((diagnostics ?? throw new ArgumentNullException(nameof(diagnostics))).ToArray());
    }

    public IReadOnlyList<ModuleMetadata> OrderedModules { get; }

    public IReadOnlyList<GeneratorDiagnostic> Diagnostics { get; }
}
