using AtomUI.City.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AtomUI.City.Presentation;

public static class ActivePluginViewRegistryServiceCollectionExtensions
{
    public static IServiceCollection AddActivePluginViewRegistry(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(
            serviceProvider => new ActivePluginViewRegistry(
                serviceProvider.GetService<IHostDiagnostics>()));
        services.TryAddSingleton<IActivePluginViewRegistry>(
            serviceProvider => serviceProvider.GetRequiredService<ActivePluginViewRegistry>());

        return services;
    }
}
