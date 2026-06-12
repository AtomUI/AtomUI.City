using AtomUI.City.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AtomUI.City.Presentation;

public static class PresentationResourceRegistryServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationResourceRegistry(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(
            serviceProvider => new PresentationResourceRegistry(
                serviceProvider.GetService<IHostDiagnostics>()));
        services.TryAddSingleton<IPresentationResourceRegistry>(
            serviceProvider => serviceProvider.GetRequiredService<PresentationResourceRegistry>());

        return services;
    }
}
