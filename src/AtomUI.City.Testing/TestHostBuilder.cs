using AtomUI.City.Hosting;

namespace AtomUI.City.Testing;

public sealed class TestHostBuilder
{
    private readonly Dictionary<string, object?> _properties = new(StringComparer.Ordinal);
    private string? _directoryName;
    private bool _keepDirectoryOnDispose;

    public TestHostBuilder UseProperty(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _properties[key] = value;

        return this;
    }

    public TestHostBuilder UseDirectoryName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _directoryName = name;

        return this;
    }

    public TestHostBuilder KeepDirectoryOnDispose()
    {
        _keepDirectoryOnDispose = true;

        return this;
    }

    public TestHost Build()
    {
        var applicationContext = new ApplicationContext();

        foreach (var property in _properties)
        {
            applicationContext.Properties[property.Key] = property.Value;
        }

        return new TestHost(
            applicationContext,
            TestDirectory.Create(_directoryName ?? "host", _keepDirectoryOnDispose),
            new FakeUiDispatcher(),
            new DeterministicScheduler(),
            new TestDiagnostics());
    }
}
