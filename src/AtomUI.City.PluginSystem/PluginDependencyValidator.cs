namespace AtomUI.City.PluginSystem;

public static class PluginDependencyValidator
{
    public static PluginValidationResult Validate(IReadOnlyList<PluginDescriptor> plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        var diagnostics = new List<PluginDiagnostic>();
        var byPluginId = plugins.ToDictionary(plugin => plugin.PluginId, StringComparer.Ordinal);

        foreach (var plugin in plugins)
        {
            foreach (var dependency in plugin.Manifest.Dependencies)
            {
                if (!byPluginId.ContainsKey(dependency.PluginId))
                {
                    diagnostics.Add(new PluginDiagnostic(
                        PluginDiagnosticIds.PluginDependencyMissing,
                        $"Plugin '{plugin.PluginId}' depends on missing plugin '{dependency.PluginId}'.",
                        plugin.PluginId,
                        "dependencies"));
                }
            }
        }

        AddCycleDiagnostics(plugins, byPluginId, diagnostics);

        return new PluginValidationResult(diagnostics);
    }

    private static void AddCycleDiagnostics(
        IReadOnlyList<PluginDescriptor> plugins,
        IReadOnlyDictionary<string, PluginDescriptor> byPluginId,
        ICollection<PluginDiagnostic> diagnostics)
    {
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        foreach (var plugin in plugins)
        {
            Visit(plugin, byPluginId, visiting, visited, diagnostics);
        }
    }

    private static void Visit(
        PluginDescriptor plugin,
        IReadOnlyDictionary<string, PluginDescriptor> byPluginId,
        ISet<string> visiting,
        ISet<string> visited,
        ICollection<PluginDiagnostic> diagnostics)
    {
        if (visited.Contains(plugin.PluginId))
        {
            return;
        }

        if (!visiting.Add(plugin.PluginId))
        {
            diagnostics.Add(new PluginDiagnostic(
                PluginDiagnosticIds.PluginDependencyCycle,
                $"Plugin dependency cycle contains '{plugin.PluginId}'.",
                plugin.PluginId,
                "dependencies"));
            return;
        }

        foreach (var dependency in plugin.Manifest.Dependencies)
        {
            if (byPluginId.TryGetValue(dependency.PluginId, out var dependencyPlugin))
            {
                Visit(dependencyPlugin, byPluginId, visiting, visited, diagnostics);
            }
        }

        visiting.Remove(plugin.PluginId);
        visited.Add(plugin.PluginId);
    }
}
