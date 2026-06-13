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

        if (IsInvalidPluginVersion(manifest.Version))
        {
            diagnostics.Add(new PluginDiagnostic(
                PluginDiagnosticIds.InvalidPluginVersion,
                $"Plugin version '{manifest.Version}' must be a semantic version and not a path segment.",
                manifest.PluginId,
                "version"));
        }

        if (IsInvalidPathSegment(manifest.TargetFramework))
        {
            diagnostics.Add(new PluginDiagnostic(
                PluginDiagnosticIds.InvalidTargetFramework,
                $"Plugin target framework '{manifest.TargetFramework}' must be a framework moniker, not a path segment.",
                manifest.PluginId,
                "targetFramework"));
        }

        if (IsInvalidMainAssembly(manifest.MainAssembly))
        {
            diagnostics.Add(new PluginDiagnostic(
                PluginDiagnosticIds.InvalidMainAssembly,
                $"Plugin main assembly '{manifest.MainAssembly}' must be a file name.",
                manifest.PluginId,
                "mainAssembly"));
        }

        foreach (var contribution in manifest.Contributions)
        {
            if (!IsInvalidPackageRelativePath(contribution.Path))
            {
                continue;
            }

            diagnostics.Add(new PluginDiagnostic(
                PluginDiagnosticIds.InvalidContributionPath,
                $"Plugin contribution path '{contribution.Path}' must stay inside the package.",
                manifest.PluginId,
                contribution.Type,
                contribution.Path));
        }

        return new PluginValidationResult(diagnostics);
    }

    internal static bool IsInvalidPluginVersion(string version)
    {
        return IsInvalidPathSegment(version) ||
            !IsSemanticVersion(version);
    }

    internal static bool IsInvalidMainAssembly(string mainAssembly)
    {
        return string.IsNullOrWhiteSpace(mainAssembly) ||
            mainAssembly.Contains('/') ||
            mainAssembly.Contains('\\') ||
            Path.GetFileName(mainAssembly) != mainAssembly;
    }

    internal static bool IsInvalidPathSegment(string value)
    {
        return value is "." or ".." ||
            value.Contains('/') ||
            value.Contains('\\') ||
            Path.IsPathRooted(value);
    }

    internal static bool IsInvalidPackageRelativePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            path.Contains('\\') ||
            Path.IsPathRooted(path))
        {
            return true;
        }

        return path
            .Split('/')
            .Any(segment => string.IsNullOrWhiteSpace(segment) || segment is "." or "..");
    }

    private static bool IsSemanticVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return false;
        }

        var buildSeparatorIndex = version.IndexOf('+', StringComparison.Ordinal);
        var versionWithoutBuild = buildSeparatorIndex < 0
            ? version
            : version[..buildSeparatorIndex];
        var build = buildSeparatorIndex < 0
            ? null
            : version[(buildSeparatorIndex + 1)..];

        if (build is not null && !IsValidSemanticIdentifierList(build, allowLeadingZeroNumbers: true))
        {
            return false;
        }

        var prereleaseSeparatorIndex = versionWithoutBuild.IndexOf('-', StringComparison.Ordinal);
        var core = prereleaseSeparatorIndex < 0
            ? versionWithoutBuild
            : versionWithoutBuild[..prereleaseSeparatorIndex];
        var prerelease = prereleaseSeparatorIndex < 0
            ? null
            : versionWithoutBuild[(prereleaseSeparatorIndex + 1)..];

        if (prerelease is not null && !IsValidSemanticIdentifierList(prerelease, allowLeadingZeroNumbers: false))
        {
            return false;
        }

        var coreParts = core.Split('.');
        return coreParts.Length == 3 &&
            coreParts.All(IsValidSemanticNumericIdentifier);
    }

    private static bool IsValidSemanticIdentifierList(string value, bool allowLeadingZeroNumbers)
    {
        return value.Length > 0 &&
            value
                .Split('.')
                .All(identifier => IsValidSemanticIdentifier(identifier, allowLeadingZeroNumbers));
    }

    private static bool IsValidSemanticIdentifier(string identifier, bool allowLeadingZeroNumbers)
    {
        if (identifier.Length == 0 ||
            identifier.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-'))
        {
            return false;
        }

        return allowLeadingZeroNumbers ||
            !identifier.All(char.IsAsciiDigit) ||
            IsValidSemanticNumericIdentifier(identifier);
    }

    private static bool IsValidSemanticNumericIdentifier(string identifier)
    {
        return identifier.Length > 0 &&
            identifier.All(char.IsAsciiDigit) &&
            (identifier.Length == 1 || identifier[0] != '0');
    }
}
