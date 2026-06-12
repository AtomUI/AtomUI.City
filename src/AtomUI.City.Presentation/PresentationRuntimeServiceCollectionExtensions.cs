using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AtomUI.City.Presentation;

public static class PresentationRuntimeServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationRuntime(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<PresentationRuntime>();
        services.TryAddSingleton<IPresentationRuntime>(
            serviceProvider => serviceProvider.GetRequiredService<PresentationRuntime>());

        return services;
    }
}
