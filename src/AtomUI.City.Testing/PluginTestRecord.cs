namespace AtomUI.City.Testing;

public sealed class PluginTestRecord
{
    public PluginTestRecord(string id, string version, string installPath, PluginTestState state)
    {
        Id = id;
        Version = version;
        InstallPath = installPath;
        State = state;
    }

    public string Id { get; }

    public string Version { get; }

    public string InstallPath { get; }

    public PluginTestState State { get; internal set; }
}
