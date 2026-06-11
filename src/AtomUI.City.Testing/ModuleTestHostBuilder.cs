using AtomUI.City.Modularity;

namespace AtomUI.City.Testing;

public sealed class ModuleTestHostBuilder
{
    private readonly List<ModuleTestRecord> _modules = [];
    private readonly TestHostBuilder _hostBuilder = TestHost.CreateBuilder();

    public ModuleTestHostBuilder UseModule(string name, IModule module)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(module);

        _modules.Add(new ModuleTestRecord(name, module));

        return this;
    }

    public ModuleTestHostBuilder UseHostProperty(string key, object? value)
    {
        _hostBuilder.UseProperty(key, value);

        return this;
    }

    public ModuleTestHost Build()
    {
        return new ModuleTestHost(_hostBuilder.Build(), _modules.ToArray());
    }
}
