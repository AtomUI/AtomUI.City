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
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPresentationCultureApplier, CultureFlowDirectionApplier>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPresentationCultureApplier, CultureResourceDictionaryApplier>());
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

    public static IServiceCollection AddPresentationFlowDirectionTarget<TTarget>(this IServiceCollection services)
        where TTarget : class, IPresentationFlowDirectionTarget
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPresentationFlowDirectionTarget, TTarget>());

        return services;
    }

    public static IServiceCollection AddPresentationResourceDictionaryTarget<TTarget>(this IServiceCollection services)
        where TTarget : class, IPresentationResourceDictionaryTarget
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPresentationResourceDictionaryTarget, TTarget>());

        return services;
    }
}
