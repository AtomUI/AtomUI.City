using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.Modularity;

public static class ModuleDependencyGraphBuilder
{
    public static ModuleDependencyGraphResult Build(IReadOnlyList<ModuleMetadata> modules)
    {
        if (modules is null)
        {
            throw new ArgumentNullException(nameof(modules));
        }

        var diagnostics = new List<GeneratorDiagnostic>();
        var modulesByName = new Dictionary<string, ModuleMetadata>(StringComparer.Ordinal);
        var modulesByTypeName = new Dictionary<string, ModuleMetadata>(StringComparer.Ordinal);

        foreach (var module in modules)
        {
            if (modulesByName.ContainsKey(module.Name))
            {
                diagnostics.Add(new GeneratorDiagnostic(
                    GeneratorDiagnostics.DuplicateModuleName,
                    $"Module name '{module.Name}' is declared more than once.",
                    module.Name));
            }
            else
            {
                modulesByName.Add(module.Name, module);
            }

            if (!modulesByTypeName.ContainsKey(module.TypeName))
            {
                modulesByTypeName.Add(module.TypeName, module);
            }
        }

        foreach (var module in modules)
        {
            foreach (var dependency in module.Dependencies)
            {
                if (dependency.Optional || modulesByTypeName.ContainsKey(dependency.TypeName))
                {
                    continue;
                }

                diagnostics.Add(new GeneratorDiagnostic(
                    GeneratorDiagnostics.InvalidManifestInput,
                    $"Module '{module.TypeName}' depends on missing module '{dependency.TypeName}'.",
                    module.TypeName));
            }
        }

        if (diagnostics.Count > 0)
        {
            return new ModuleDependencyGraphResult([], diagnostics);
        }

        var orderedModules = new List<ModuleMetadata>();
        var visitStates = new Dictionary<string, ModuleVisitState>(StringComparer.Ordinal);

        foreach (var module in modules)
        {
            if (!Visit(module, modulesByTypeName, visitStates, orderedModules, diagnostics))
            {
                return new ModuleDependencyGraphResult([], diagnostics);
            }
        }

        return new ModuleDependencyGraphResult(orderedModules, diagnostics);
    }

    private static bool Visit(
        ModuleMetadata module,
        IReadOnlyDictionary<string, ModuleMetadata> modulesByTypeName,
        IDictionary<string, ModuleVisitState> visitStates,
        ICollection<ModuleMetadata> orderedModules,
        ICollection<GeneratorDiagnostic> diagnostics)
    {
        if (visitStates.TryGetValue(module.TypeName, out var state))
        {
            if (state == ModuleVisitState.Visited)
            {
                return true;
            }

            diagnostics.Add(new GeneratorDiagnostic(
                GeneratorDiagnostics.CircularModuleDependency,
                $"Module dependency graph contains a cycle at '{module.TypeName}'.",
                module.TypeName));

            return false;
        }

        visitStates.Add(module.TypeName, ModuleVisitState.Visiting);

        foreach (var dependency in module.Dependencies)
        {
            if (!modulesByTypeName.TryGetValue(dependency.TypeName, out var dependencyModule))
            {
                continue;
            }

            if (!Visit(dependencyModule, modulesByTypeName, visitStates, orderedModules, diagnostics))
            {
                return false;
            }
        }

        visitStates[module.TypeName] = ModuleVisitState.Visited;
        orderedModules.Add(module);

        return true;
    }

    private enum ModuleVisitState
    {
        Visiting,
        Visited,
    }
}
