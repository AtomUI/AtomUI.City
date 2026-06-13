namespace AtomUI.City.PluginSystem;

public static class PluginManifestValidator
{
    public static PluginValidationResult Validate(PluginManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        var diagnostics = new List<PluginDiagnostic>();

        if (string.IsNullOrWhiteSpace(manifest.PluginId))
        {
            diagnostics.Add(new PluginDiagnostic(
                PluginDiagnosticIds.MissingPluginId,
                "Plugin manifest field 'pluginId' is required.",
                Field: "pluginId"));
        }
        else if (IsInvalidPathSegment(manifest.PluginId))
        {
            diagnostics.Add(new PluginDiagnostic(
                PluginDiagnosticIds.InvalidPluginId,
                $"Plugin id '{manifest.PluginId}' must be a stable identifier, not a path segment.",
                manifest.PluginId,
                "pluginId"));
        }

        if (!manifest.SchemaVersion.StartsWith("1.", StringComparison.Ordinal))
        {
            diagnostics.Add(new PluginDiagnostic(
                PluginDiagnosticIds.UnsupportedManifestSchema,
                $"Plugin manifest schema version '{manifest.SchemaVersion}' is not supported.",
                manifest.PluginId,
                "schemaVersion"));
        }

        if (IsInvalidPathSegment(manifest.Version))
        {
            diagnostics.Add(new PluginDiagnostic(
                PluginDiagnosticIds.InvalidPluginVersion,
                $"Plugin version '{manifest.Version}' must be a stable version, not a path segment.",
                manifest.PluginId,
                "version"));
        }

        if (IsInvalidMainAssembly(manifest.MainAssembly))
        {
            diagnostics.Add(new PluginDiagnostic(
                PluginDiagnosticIds.InvalidMainAssembly,
                $"Plugin main assembly '{manifest.MainAssembly}' must be a file name.",
                manifest.PluginId,
                "mainAssembly"));
        }

        return new PluginValidationResult(diagnostics);
    }

    internal static bool IsInvalidMainAssembly(string mainAssembly)
    {
        return string.IsNullOrWhiteSpace(mainAssembly) ||
            mainAssembly.Contains('/') ||
            mainAssembly.Contains('\\') ||
            Path.GetFileName(mainAssembly) != mainAssembly;
    }

    private static bool IsInvalidPathSegment(string value)
    {
        return value is "." or ".." ||
            value.Contains('/') ||
            value.Contains('\\') ||
            Path.IsPathRooted(value);
    }
}
