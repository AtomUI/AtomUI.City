using AtomUI.City.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AtomUI.City.Presentation;

public static class ViewRegistryServiceCollectionExtensions
{
    public static IServiceCollection AddViewRegistry(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(
            serviceProvider =>
            {
                var diagnostics = serviceProvider.GetService<IHostDiagnostics>();

                return diagnostics is null
                    ? new ViewRegistry()
                    : new ViewRegistry(diagnostics);
            });
        services.TryAddSingleton<IViewRegistry>(
            serviceProvider => serviceProvider.GetRequiredService<ViewRegistry>());
        services.TryAddSingleton<IViewLocator>(
            serviceProvider => serviceProvider.GetRequiredService<ViewRegistry>());

        return services;
    }
}
