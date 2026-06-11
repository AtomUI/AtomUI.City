namespace AtomUI.City.Testing;

public sealed class PluginTestHostBuilder
{
    private readonly List<PluginTestPackage> _packages = [];
    private readonly TestHostBuilder _hostBuilder = TestHost.CreateBuilder();

    public PluginTestHostBuilder UsePlugin(string id, string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        _packages.Add(new PluginTestPackage(id, version));

        return this;
    }

    public PluginTestHost Build()
    {
        return new PluginTestHost(_hostBuilder.UseDirectoryName("plugin-host").Build(), _packages.ToArray());
    }
}
