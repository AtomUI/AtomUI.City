using AtomUI.City.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AtomUI.City.Presentation;

public static class PresentationLocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationLocalizationBridge(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPresentationCultureApplier, CurrentThreadCultureApplier>());
        services.TryAddSingleton<PresentationLocalizationBridge>();
        services.TryAddSingleton<IPresentationLocalizationBridge>(
            serviceProvider => serviceProvider.GetRequiredService<PresentationLocalizationBridge>());

        return services;
    }

    public static IServiceCollection AddPresentationCultureApplier<TApplier>(this IServiceCollection services)
        where TApplier : class, IPresentationCultureApplier
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPresentationCultureApplier, TApplier>());

        return services;
    }
}
