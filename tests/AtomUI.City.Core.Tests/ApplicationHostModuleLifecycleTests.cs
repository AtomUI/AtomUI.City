using AtomUI.City.Hosting;
using AtomUI.City.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Core.Tests;

public sealed class ApplicationHostModuleLifecycleTests
{
    [Fact]
    public async Task BuildAndStartRunModulesInDependencyOrderAndShutdownInReverseOrder()
    {
        ModuleRecorder.Reset();
        var builder = ApplicationHost.CreateBuilder();

        builder
            .UseModule<AppModule>()
            .UseModule<CoreModule>();

        await using var host = builder.Build();

        Assert.IsType<CoreService>(host.Services.GetRequiredService<ICoreService>());

        await host.StartAsync();
        await host.StopAsync();

        Assert.Equal(
            [
                "Core:PreConfigureServices",
                "App:PreConfigureServices",
                "Core:ConfigureServices",
                "App:ConfigureServices",
                "Core:PostConfigureServices",
                "App:PostConfigureServices",
                "Core:ConfigureContributions",
                "App:ConfigureContributions",
                "Core:OnPreApplicationInitialization",
                "App:OnPreApplicationInitialization",
                "Core:OnApplicationInitialization",
                "App:OnApplicationInitialization",
                "Core:OnPostApplicationInitialization",
                "App:OnPostApplicationInitialization",
                "App:OnApplicationShutdown",
                "Core:OnApplicationShutdown",
            ],
            ModuleRecorder.Calls);
    }

    [Fact]
    public async Task AsyncModuleInitializationStagesAreAwaited()
    {
        ModuleRecorder.Reset();
        var builder = ApplicationHost.CreateBuilder();

        builder.UseModule<AsyncModule>();

        await using var host = builder.Build();

        await host.StartAsync();
        await host.StopAsync();

        Assert.Equal(
            [
                "Async:OnApplicationInitialization.Start",
                "Async:OnApplicationInitialization.End",
                "Async:OnApplicationShutdown.Start",
                "Async:OnApplicationShutdown.End",
            ],
            ModuleRecorder.Calls.Where(call => call.StartsWith("Async:", StringComparison.Ordinal)));
    }

    [Fact]
    public void BuildFailsWhenRequiredDependencyIsMissing()
    {
        var builder = ApplicationHost.CreateBuilder();

        builder.UseModule<MissingRequiredDependencyModule>();

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains(nameof(MissingRequiredDependencyModule), exception.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(CoreModule), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OptionalDependencyCanBeMissing()
    {
        ModuleRecorder.Reset();
        var builder = ApplicationHost.CreateBuilder();

        builder.UseModule<MissingOptionalDependencyModule>();

        await using var host = builder.Build();

        await host.StartAsync();

        Assert.Contains("Optional:OnApplicationInitialization", ModuleRecorder.Calls);
    }

    [Fact]
    public async Task ModuleRegistryModulesRejectExternalListMutation()
    {
        var builder = ApplicationHost.CreateBuilder();
        builder.UseModule<CoreModule>();

        await using var host = builder.Build();
        var registry = host.Services.GetRequiredService<IModuleRegistry>();
        var modules = Assert.IsAssignableFrom<IList<ModuleDescriptor>>(registry.Modules);

        Assert.Throws<NotSupportedException>(() => modules[0] = new ModuleDescriptor(
            "Replacement",
            typeof(AsyncModule),
            version: null,
            description: null,
            []));
        Assert.Equal(typeof(CoreModule), registry.Modules[0].ModuleType);
    }

    private interface ICoreService;

    private sealed class CoreService : ICoreService;

    private sealed class CoreModule : RecordingModule
    {
        public CoreModule()
            : base("Core")
        {
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            base.ConfigureServices(context);
            context.Services.AddSingleton<ICoreService, CoreService>();
        }
    }

    [DependsOn(typeof(CoreModule))]
    private sealed class AppModule : RecordingModule
    {
        public AppModule()
            : base("App")
        {
        }
    }

    [DependsOn(typeof(CoreModule))]
    private sealed class MissingRequiredDependencyModule : ModuleBase;

    [DependsOn(typeof(CoreModule), Optional = true)]
    private sealed class MissingOptionalDependencyModule : RecordingModule
    {
        public MissingOptionalDependencyModule()
            : base("Optional")
        {
        }
    }

    private sealed class AsyncModule : ModuleBase
    {
        public override async ValueTask OnApplicationInitializationAsync(
            ApplicationInitializationContext context,
            CancellationToken cancellationToken = default)
        {
            ModuleRecorder.Record("Async:OnApplicationInitialization.Start");
            await Task.Yield();
            ModuleRecorder.Record("Async:OnApplicationInitialization.End");
        }

        public override async ValueTask OnApplicationShutdownAsync(
            ApplicationShutdownContext context,
            CancellationToken cancellationToken = default)
        {
            ModuleRecorder.Record("Async:OnApplicationShutdown.Start");
            await Task.Yield();
            ModuleRecorder.Record("Async:OnApplicationShutdown.End");
        }
    }

    private abstract class RecordingModule(string name) : ModuleBase
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            ModuleRecorder.Record($"{name}:PreConfigureServices");
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            ModuleRecorder.Record($"{name}:ConfigureServices");
        }

        public override void PostConfigureServices(ServiceConfigurationContext context)
        {
            ModuleRecorder.Record($"{name}:PostConfigureServices");
        }

        public override void ConfigureContributions(ContributionConfigurationContext context)
        {
            ModuleRecorder.Record($"{name}:ConfigureContributions");
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            ModuleRecorder.Record($"{name}:OnPreApplicationInitialization");
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            ModuleRecorder.Record($"{name}:OnApplicationInitialization");
        }

        public override void OnPostApplicationInitialization(ApplicationInitializationContext context)
        {
            ModuleRecorder.Record($"{name}:OnPostApplicationInitialization");
        }

        public override void OnApplicationShutdown(ApplicationShutdownContext context)
        {
            ModuleRecorder.Record($"{name}:OnApplicationShutdown");
        }
    }

    private static class ModuleRecorder
    {
        private static readonly List<string> RecordedCalls = [];

        public static IReadOnlyList<string> Calls => RecordedCalls;

        public static void Record(string call)
        {
            RecordedCalls.Add(call);
        }

        public static void Reset()
        {
            RecordedCalls.Clear();
        }
    }
}
