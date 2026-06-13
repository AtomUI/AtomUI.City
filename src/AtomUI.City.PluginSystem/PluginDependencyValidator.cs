namespace AtomUI.City.PluginSystem;

public static class PluginDependencyValidator
{
    public static PluginValidationResult Validate(IReadOnlyList<PluginDescriptor> plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        var diagnostics = new List<PluginDiagnostic>();
        var byPluginId = BuildPluginIndex(plugins, diagnostics);

        foreach (var plugin in plugins)
        {
            foreach (var dependency in plugin.Manifest.Dependencies)
            {
                if (!byPluginId.TryGetValue(dependency.PluginId, out var dependencyPlugin))
                {
                    diagnostics.Add(new PluginDiagnostic(
                        PluginDiagnosticIds.PluginDependencyMissing,
                        $"Plugin '{plugin.PluginId}' depends on missing plugin '{dependency.PluginId}'.",
                        plugin.PluginId,
                        "dependencies"));
                }
                else if (!SatisfiesVersionRange(dependencyPlugin.Version, dependency.VersionRange))
                {
                    diagnostics.Add(new PluginDiagnostic(
                        PluginDiagnosticIds.PluginDependencyVersionMismatch,
                        $"Plugin '{plugin.PluginId}' depends on plugin '{dependency.PluginId}' version '{dependency.VersionRange}', but version '{dependencyPlugin.Version}' is available.",
                        plugin.PluginId,
                        "dependencies"));
                }
            }
        }

        AddCycleDiagnostics(plugins, byPluginId, diagnostics);

        return new PluginValidationResult(diagnostics);
    }

    private static IReadOnlyDictionary<string, PluginDescriptor> BuildPluginIndex(
        IReadOnlyList<PluginDescriptor> plugins,
        ICollection<PluginDiagnostic> diagnostics)
    {
        var byPluginId = new Dictionary<string, PluginDescriptor>(StringComparer.Ordinal);

        foreach (var plugin in plugins)
        {
            if (!byPluginId.TryAdd(plugin.PluginId, plugin))
            {
                diagnostics.Add(new PluginDiagnostic(
                    PluginDiagnosticIds.PluginIdConflict,
                    $"Plugin id '{plugin.PluginId}' is provided by multiple plugin descriptors.",
                    plugin.PluginId,
                    "pluginId"));
            }
        }

        return byPluginId;
    }

    private static bool SatisfiesVersionRange(string version, string? versionRange)
    {
        if (string.IsNullOrWhiteSpace(versionRange))
        {
            return true;
        }

        if (!Version.TryParse(version, out var parsedVersion))
        {
            return false;
        }

        var range = versionRange.Trim();
        if (range.Length < 2)
        {
            return Version.TryParse(range, out var exactVersion)
                && parsedVersion == exactVersion;
        }

        var hasLowerBound = range[0] is '[' or '(';
        var hasUpperBound = range[^1] is ']' or ')';
        if (!hasLowerBound || !hasUpperBound)
        {
            return Version.TryParse(range, out var exactVersion)
                && parsedVersion == exactVersion;
        }

        var parts = range[1..^1].Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        if (!SatisfiesLowerBound(parsedVersion, parts[0], range[0] == '['))
        {
            return false;
        }

        return SatisfiesUpperBound(parsedVersion, parts[1], range[^1] == ']');
    }

    private static bool SatisfiesLowerBound(Version version, string bound, bool inclusive)
    {
        if (bound.Length == 0)
        {
            return true;
        }

        if (!Version.TryParse(bound, out var lowerBound))
        {
            return false;
        }

        var comparison = version.CompareTo(lowerBound);
        return inclusive
            ? comparison >= 0
            : comparison > 0;
    }

    private static bool SatisfiesUpperBound(Version version, string bound, bool inclusive)
    {
        if (bound.Length == 0)
        {
            return true;
        }

        if (!Version.TryParse(bound, out var upperBound))
        {
            return false;
        }

        var comparison = version.CompareTo(upperBound);
        return inclusive
            ? comparison <= 0
            : comparison < 0;
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
