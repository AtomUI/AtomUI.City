namespace AtomUI.City.PluginSystem;

public static class PluginPackageLayoutValidator
{
    public static PluginValidationResult Validate(string packageRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageRoot);

        var diagnostics = new List<PluginDiagnostic>();
        var manifestPath = PluginPackagePaths.GetManifestPath(packageRoot);

        if (!File.Exists(manifestPath))
        {
            diagnostics.Add(new PluginDiagnostic(
                PluginDiagnosticIds.ManifestNotFound,
                "Plugin package must contain atomui-city/plugin.json.",
                Path: manifestPath));

            return new PluginValidationResult(diagnostics);
        }

        var manifest = PluginManifestReader.Read(manifestPath);
        diagnostics.AddRange(PluginManifestValidator.Validate(manifest).Diagnostics);

        if (!PluginManifestValidator.IsInvalidMainAssembly(manifest.MainAssembly))
        {
            var mainAssemblyPath = PluginPackagePaths.GetMainAssemblyPath(
                packageRoot,
                manifest.TargetFramework,
                manifest.MainAssembly);

            if (!File.Exists(mainAssemblyPath))
            {
                diagnostics.Add(new PluginDiagnostic(
                    PluginDiagnosticIds.MainAssemblyNotFound,
                    $"Plugin main assembly '{manifest.MainAssembly}' was not found.",
                    manifest.PluginId,
                    "mainAssembly",
                    mainAssemblyPath));
            }
        }

        foreach (var contribution in manifest.Contributions.Where(contribution => contribution.Required))
        {
            if (PluginManifestValidator.IsInvalidPackageRelativePath(contribution.Path))
            {
                continue;
            }

            var contributionPath = PluginPackagePaths.CombinePackagePath(packageRoot, contribution.Path);
            if (File.Exists(contributionPath))
            {
                continue;
            }

            diagnostics.Add(new PluginDiagnostic(
                PluginDiagnosticIds.RequiredContributionManifestNotFound,
                $"Required contribution manifest '{contribution.Path}' was not found.",
                manifest.PluginId,
                contribution.Type,
                contributionPath));
        }

        return new PluginValidationResult(diagnostics);
    }
}
