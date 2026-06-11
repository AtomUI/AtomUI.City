using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.PluginSystem;

public static class PluginManifestBuilder
{
    public static PluginManifestResult Build(PluginMetadata metadata)
    {
        if (metadata is null)
        {
            throw new ArgumentNullException(nameof(metadata));
        }

        var diagnostics = new List<GeneratorDiagnostic>();

        ValidateRequired(metadata.PluginId, "pluginId", diagnostics);
        ValidateRequired(metadata.PackageId, "packageId", diagnostics);
        ValidateRequired(metadata.Version, "version", diagnostics);
        ValidateRequired(metadata.DisplayNameKey, "displayNameKey", diagnostics);
        ValidateRequired(metadata.MainAssembly, "mainAssembly", diagnostics);
        ValidateRequired(metadata.TargetFramework, "targetFramework", diagnostics);
        ValidateRequired(metadata.PluginApiVersion, "pluginApiVersion", diagnostics);
        ValidateRequired(metadata.MinHostVersion, "minHostVersion", diagnostics);

        if (!string.IsNullOrWhiteSpace(metadata.MainAssembly) && IsInvalidMainAssembly(metadata.MainAssembly))
        {
            diagnostics.Add(new GeneratorDiagnostic(
                GeneratorDiagnostics.InvalidManifestInput,
                $"Plugin main assembly '{metadata.MainAssembly}' must be a file name.",
                "mainAssembly"));
        }

        AddDuplicateContributionDiagnostics(metadata.Contributions, diagnostics);
        AddDuplicateCapabilityDiagnostics(metadata.Capabilities, diagnostics);
        AddContributionPathDiagnostics(metadata.Contributions, diagnostics);

        if (diagnostics.Count > 0)
        {
            return new PluginManifestResult(CreateManifest(metadata, [], [], []), diagnostics);
        }

        var capabilities = metadata
            .Capabilities
            .Select(capability => new PluginCapabilityManifestEntry(
                capability.Name,
                capability.Scope.OrderBy(scope => scope, StringComparer.Ordinal).ToArray()))
            .OrderBy(capability => capability.Name, StringComparer.Ordinal)
            .ToArray();
        var contributions = metadata
            .Contributions
            .Select(contribution => new PluginContributionManifestEntry(
                contribution.Type,
                contribution.Path,
                contribution.Required))
            .OrderBy(contribution => contribution.Type, StringComparer.Ordinal)
            .ToArray();
        var dependencies = metadata
            .Dependencies
            .Select(dependency => new PluginDependencyManifestEntry(
                dependency.PluginId,
                dependency.VersionRange))
            .OrderBy(dependency => dependency.PluginId, StringComparer.Ordinal)
            .ToArray();

        return new PluginManifestResult(
            CreateManifest(metadata, capabilities, contributions, dependencies),
            diagnostics);
    }

    private static PluginManifest CreateManifest(
        PluginMetadata metadata,
        IReadOnlyList<PluginCapabilityManifestEntry> capabilities,
        IReadOnlyList<PluginContributionManifestEntry> contributions,
        IReadOnlyList<PluginDependencyManifestEntry> dependencies)
    {
        return new PluginManifest(
            metadata.SchemaVersion,
            metadata.PluginId,
            metadata.PackageId,
            metadata.Version,
            metadata.DisplayNameKey,
            metadata.DescriptionKey,
            metadata.Publisher,
            metadata.MainAssembly,
            metadata.TargetFramework,
            metadata.PluginApiVersion,
            metadata.MinHostVersion,
            metadata.MaxHostVersion,
            metadata.Unloadable,
            metadata.AotCompatible,
            capabilities,
            contributions,
            dependencies);
    }

    private static void ValidateRequired(
        string value,
        string fieldName,
        ICollection<GeneratorDiagnostic> diagnostics)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        diagnostics.Add(new GeneratorDiagnostic(
            GeneratorDiagnostics.InvalidManifestInput,
            $"Plugin manifest field '{fieldName}' is required.",
            fieldName));
    }

    private static void AddDuplicateContributionDiagnostics(
        IEnumerable<PluginContributionManifestMetadata> contributions,
        ICollection<GeneratorDiagnostic> diagnostics)
    {
        var types = new HashSet<string>(StringComparer.Ordinal);

        foreach (var contribution in contributions)
        {
            if (types.Add(contribution.Type))
            {
                continue;
            }

            diagnostics.Add(new GeneratorDiagnostic(
                GeneratorDiagnostics.InvalidManifestInput,
                $"Plugin contribution manifest '{contribution.Type}' is declared more than once.",
                contribution.Type));
        }
    }

    private static void AddDuplicateCapabilityDiagnostics(
        IEnumerable<PluginCapabilityMetadata> capabilities,
        ICollection<GeneratorDiagnostic> diagnostics)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);

        foreach (var capability in capabilities)
        {
            if (names.Add(capability.Name))
            {
                continue;
            }

            diagnostics.Add(new GeneratorDiagnostic(
                GeneratorDiagnostics.InvalidManifestInput,
                $"Plugin capability '{capability.Name}' is declared more than once.",
                capability.Name));
        }
    }

    private static void AddContributionPathDiagnostics(
        IEnumerable<PluginContributionManifestMetadata> contributions,
        ICollection<GeneratorDiagnostic> diagnostics)
    {
        foreach (var contribution in contributions)
        {
            if (!IsInvalidContributionPath(contribution.Path))
            {
                continue;
            }

            diagnostics.Add(new GeneratorDiagnostic(
                GeneratorDiagnostics.InvalidManifestInput,
                $"Plugin contribution manifest path '{contribution.Path}' must be a relative package path using '/'.",
                contribution.Type));
        }
    }

    private static bool IsInvalidMainAssembly(string mainAssembly)
    {
        return mainAssembly.Contains("/", StringComparison.Ordinal) ||
            mainAssembly.Contains("\\", StringComparison.Ordinal) ||
            Path.GetFileName(mainAssembly) != mainAssembly;
    }

    private static bool IsInvalidContributionPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            path.StartsWith("/", StringComparison.Ordinal) ||
            path.Contains("\\", StringComparison.Ordinal))
        {
            return true;
        }

        var firstSeparatorIndex = path.IndexOf("/", StringComparison.Ordinal);
        var firstSegment = firstSeparatorIndex < 0
            ? path
            : path.Substring(0, firstSeparatorIndex);

        if (firstSegment.Contains(":", StringComparison.Ordinal))
        {
            return true;
        }

        return path
            .Split('/')
            .Any(segment => string.Equals(segment, ".", StringComparison.Ordinal) ||
                string.Equals(segment, "..", StringComparison.Ordinal));
    }
}
