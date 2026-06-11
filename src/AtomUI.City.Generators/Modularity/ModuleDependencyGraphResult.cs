using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.Modularity;

public sealed class ModuleDependencyGraphResult
{
    public ModuleDependencyGraphResult(
        IReadOnlyList<ModuleMetadata> orderedModules,
        IReadOnlyList<GeneratorDiagnostic> diagnostics)
    {
        OrderedModules = orderedModules ?? throw new ArgumentNullException(nameof(orderedModules));
        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
    }

    public IReadOnlyList<ModuleMetadata> OrderedModules { get; }

    public IReadOnlyList<GeneratorDiagnostic> Diagnostics { get; }
}
