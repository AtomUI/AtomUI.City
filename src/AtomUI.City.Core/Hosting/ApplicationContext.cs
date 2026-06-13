using Microsoft.Extensions.Configuration;

namespace AtomUI.City.Hosting;

public sealed class ApplicationContext : IApplicationContext
{
    private IReadOnlyList<string> _startupArguments = Array.AsReadOnly(Array.Empty<string>());

    private sealed class NullServiceProvider : IServiceProvider
    {
        public static readonly NullServiceProvider Instance = new();

        private NullServiceProvider()
        {
        }

        public object? GetService(Type serviceType)
        {
            return null;
        }
    }

    public string ApplicationName { get; internal set; } = "AtomUI.City.Application";

    public string EnvironmentName { get; internal set; } = "Production";

    public string ContentRootPath { get; internal set; } = Directory.GetCurrentDirectory();

    public string AppDataPath { get; internal set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AtomUI.City.Application");

    public IReadOnlyList<string> StartupArguments
    {
        get => _startupArguments;
        internal set
        {
            ArgumentNullException.ThrowIfNull(value);

            _startupArguments = Array.AsReadOnly(value.ToArray());
        }
    }

    public IConfiguration Configuration { get; internal set; } = new ConfigurationBuilder().Build();

    public IServiceProvider Services { get; internal set; } = NullServiceProvider.Instance;

    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();
}
