using AtomUI.City.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AtomUI.City.Presentation;

public static class PresentationPluginUnloadCoordinatorServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationPluginUnloadCoordinator(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(
            serviceProvider => new PresentationPluginUnloadCoordinator(
                serviceProvider.GetRequiredService<IActivePluginViewRegistry>(),
                serviceProvider.GetRequiredService<IInteractionHandlerRegistry>(),
                serviceProvider.GetRequiredService<IViewRegistry>(),
                serviceProvider.GetRequiredService<IPresentationResourceRegistry>(),
                serviceProvider.GetRequiredService<IPresentationResourceDictionaryRevoker>(),
                serviceProvider.GetService<IHostDiagnostics>()));
        services.TryAddSingleton<IPresentationPluginUnloadCoordinator>(
            serviceProvider => serviceProvider.GetRequiredService<PresentationPluginUnloadCoordinator>());

        return services;
    }
}
