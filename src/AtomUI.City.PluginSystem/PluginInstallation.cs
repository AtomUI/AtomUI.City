namespace AtomUI.City.PluginSystem;

public sealed record PluginInstallation(
    string PluginId,
    string PackageId,
    string Version,
    string RootPath,
    string ManifestPath);

public sealed class PluginInstallResult
{
    private PluginInstallResult(
        PluginInstallation? installation,
        IReadOnlyList<PluginDiagnostic> diagnostics)
    {
        Installation = installation;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
    }

    public PluginInstallation? Installation { get; }

    public IReadOnlyList<PluginDiagnostic> Diagnostics { get; }

    public bool Succeeded => Diagnostics.Count == 0;

    public static PluginInstallResult Success(PluginInstallation installation)
    {
        ArgumentNullException.ThrowIfNull(installation);

        return new PluginInstallResult(installation, []);
    }

    public static PluginInstallResult Failed(IReadOnlyList<PluginDiagnostic> diagnostics)
    {
        return new PluginInstallResult(null, diagnostics);
    }
}
