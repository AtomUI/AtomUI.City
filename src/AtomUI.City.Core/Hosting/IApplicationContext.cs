using Microsoft.Extensions.Configuration;

namespace AtomUI.City.Hosting;

public interface IApplicationContext
{
    string ApplicationName { get; }

    string EnvironmentName { get; }

    string ContentRootPath { get; }

    string AppDataPath { get; }

    IReadOnlyList<string> StartupArguments { get; }

    IConfiguration Configuration { get; }

    IServiceProvider Services { get; }

    IDictionary<string, object?> Properties { get; }
}
