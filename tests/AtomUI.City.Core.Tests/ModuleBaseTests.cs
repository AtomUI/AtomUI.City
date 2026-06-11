using AtomUI.City.Hosting;
using AtomUI.City.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Core.Tests;

public sealed class ModuleBaseTests
{
    [Fact]
    public async Task AsyncLifecycleMethodsCallSynchronousConvenienceMethodsInOrder()
    {
        var applicationContext = new ApplicationContext();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var calls = new List<string>();
        var module = new RecordingModule(calls);

        await module.PreConfigureServicesAsync(new ServiceConfigurationContext(applicationContext, services));
        await module.ConfigureServicesAsync(new ServiceConfigurationContext(applicationContext, services));
        await module.PostConfigureServicesAsync(new ServiceConfigurationContext(applicationContext, services));
        await module.ConfigureContributionsAsync(new ContributionConfigurationContext(applicationContext, serviceProvider));
        await module.OnPreApplicationInitializationAsync(new ApplicationInitializationContext(applicationContext, serviceProvider));
        await module.OnApplicationInitializationAsync(new ApplicationInitializationContext(applicationContext, serviceProvider));
        await module.OnPostApplicationInitializationAsync(new ApplicationInitializationContext(applicationContext, serviceProvider));
        await module.OnApplicationShutdownAsync(new ApplicationShutdownContext(applicationContext, serviceProvider));

        Assert.Equal(
            [
                "PreConfigureServices",
                "ConfigureServices",
                "PostConfigureServices",
                "ConfigureContributions",
                "OnPreApplicationInitialization",
                "OnApplicationInitialization",
                "OnPostApplicationInitialization",
                "OnApplicationShutdown",
            ],
            calls);
    }

    [Fact]
    public async Task DefaultLifecycleMethodsComplete()
    {
        var applicationContext = new ApplicationContext();
        var services = new ServiceCollection();
        using var serviceProvider = services.BuildServiceProvider();
        var module = new EmptyModule();

        await module.PreConfigureServicesAsync(new ServiceConfigurationContext(applicationContext, services));
        await module.ConfigureServicesAsync(new ServiceConfigurationContext(applicationContext, services));
        await module.PostConfigureServicesAsync(new ServiceConfigurationContext(applicationContext, services));
        await module.ConfigureContributionsAsync(new ContributionConfigurationContext(applicationContext, serviceProvider));
        await module.OnPreApplicationInitializationAsync(new ApplicationInitializationContext(applicationContext, serviceProvider));
        await module.OnApplicationInitializationAsync(new ApplicationInitializationContext(applicationContext, serviceProvider));
        await module.OnPostApplicationInitializationAsync(new ApplicationInitializationContext(applicationContext, serviceProvider));
        await module.OnApplicationShutdownAsync(new ApplicationShutdownContext(applicationContext, serviceProvider));
    }

    private sealed class RecordingModule(List<string> calls) : ModuleBase
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            calls.Add("PreConfigureServices");
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            calls.Add("ConfigureServices");
        }

        public override void PostConfigureServices(ServiceConfigurationContext context)
        {
            calls.Add("PostConfigureServices");
        }

        public override void ConfigureContributions(ContributionConfigurationContext context)
        {
            calls.Add("ConfigureContributions");
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            calls.Add("OnPreApplicationInitialization");
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            calls.Add("OnApplicationInitialization");
        }

        public override void OnPostApplicationInitialization(ApplicationInitializationContext context)
        {
            calls.Add("OnPostApplicationInitialization");
        }

        public override void OnApplicationShutdown(ApplicationShutdownContext context)
        {
            calls.Add("OnApplicationShutdown");
        }
    }

    private sealed class EmptyModule : ModuleBase;
}
