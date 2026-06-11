using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Hosting;

public interface IApplicationHostBuilder
{
    IServiceCollection Services { get; }

    IConfigurationManager Configuration { get; }

    IDictionary<string, object?> Properties { get; }

    IApplicationHostBuilder ConfigureServices(Action<IServiceCollection> configureServices);

    IApplicationHostBuilder ConfigureHost(Action<ApplicationHostOptions> configureOptions);

    IApplicationHost Build();
}
