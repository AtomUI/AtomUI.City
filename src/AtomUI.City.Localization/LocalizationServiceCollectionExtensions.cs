using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AtomUI.City.Localization;

public static class LocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddLocalization(
        this IServiceCollection services,
        Action<LocalizationOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(_ =>
        {
            var options = new LocalizationOptions();
            configure?.Invoke(options);

            return options;
        });
        services.TryAddSingleton<ILocalizationDiagnostics, InMemoryLocalizationDiagnostics>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILanguagePackageProvider, FileLanguagePackageProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILanguagePackageProvider, AssemblyLanguagePackageProvider>());
        services.TryAddSingleton<ILocalizationService>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<LocalizationOptions>();

            return new LocalizationService(
                options.LanguagePackages.ToArray(),
                serviceProvider.GetServices<ILanguagePackageProvider>(),
                serviceProvider.GetService<IPresentationLocalizationBridge>(),
                serviceProvider.GetService<ILocalizationDiagnostics>());
        });

        return services;
    }
}
