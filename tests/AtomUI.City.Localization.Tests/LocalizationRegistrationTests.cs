using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Localization.Tests;

public sealed class LocalizationRegistrationTests
{
    [Fact]
    public async Task AddLocalizationRegistersCoreServices()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Settings.Title", "设置"));
        var services = new ServiceCollection();

        services.AddSingleton<ILanguagePackageProvider>(new RecordingLanguagePackageProvider(zh));
        services.AddLocalization(options => options.LanguagePackages.Add(zh.Descriptor));

        using var serviceProvider = services.BuildServiceProvider();
        var localization = serviceProvider.GetRequiredService<ILocalizationService>();
        var diagnostics = serviceProvider.GetRequiredService<ILocalizationDiagnostics>();
        var providers = serviceProvider.GetServices<ILanguagePackageProvider>().ToArray();

        await localization.SetCultureAsync("zh-CN");
        var text = await localization.GetStringAsync("Settings.Title");

        Assert.IsType<LocalizationService>(localization);
        Assert.IsType<InMemoryLocalizationDiagnostics>(diagnostics);
        Assert.Contains(providers, provider => provider.Kind == LanguagePackageProviderKind.InMemory);
        Assert.Contains(providers, provider => provider.Kind == LanguagePackageProviderKind.File);
        Assert.Contains(providers, provider => provider.Kind == LanguagePackageProviderKind.Assembly);
        Assert.Equal("设置", text.Value);
    }

    private static LanguagePackage Package(
        string packageId,
        string cultureName,
        params (string Key, string Value)[] resources)
    {
        return LanguagePackage.Create(
            new LanguagePackageDescriptor(
                packageId,
                CultureInfo.GetCultureInfo(cultureName),
                ResourceScope.Host),
            resources.ToDictionary(resource => resource.Key, resource => resource.Value));
    }
}
