using AtomUI.City.Modularity;
using AtomUI.City.Testing;

namespace AtomUI.City.Testing.Tests;

public sealed class ModuleTestHostTests
{
    [Fact]
    public async Task InitializeAsyncRunsModuleConfigurationStagesInRegistrationOrder()
    {
        var calls = new List<string>();
        await using var host = ModuleTestHost
            .CreateBuilder()
            .UseModule("First", new RecordingModule("First", calls))
            .UseModule("Second", new RecordingModule("Second", calls))
            .Build();

        await host.InitializeAsync();

        Assert.Equal(
            [
                "First:PreConfigureServices",
                "Second:PreConfigureServices",
                "First:ConfigureServices",
                "Second:ConfigureServices",
                "First:PostConfigureServices",
                "Second:PostConfigureServices",
                "First:ConfigureContributions",
                "Second:ConfigureContributions",
                "First:OnPreApplicationInitialization",
                "Second:OnPreApplicationInitialization",
                "First:OnApplicationInitialization",
                "Second:OnApplicationInitialization",
                "First:OnPostApplicationInitialization",
                "Second:OnPostApplicationInitialization",
            ],
            calls);
    }

    [Fact]
    public async Task ShutdownAsyncRunsModulesInReverseRegistrationOrder()
    {
        var calls = new List<string>();
        await using var host = ModuleTestHost
            .CreateBuilder()
            .UseModule("First", new RecordingModule("First", calls))
            .UseModule("Second", new RecordingModule("Second", calls))
            .Build();

        await host.InitializeAsync();
        await host.ShutdownAsync();

        Assert.Equal(
            [
                "Second:OnApplicationShutdown",
                "First:OnApplicationShutdown",
            ],
            calls.TakeLast(2));
    }

    [Fact]
    public void ModulesExposeStableTestRecords()
    {
        using var host = ModuleTestHost
            .CreateBuilder()
            .UseModule("Sample", new RecordingModule("Sample", []))
            .Build();

        var record = Assert.Single(host.Modules);

        Assert.Equal("Sample", record.Name);
        Assert.Equal(typeof(RecordingModule), record.Module.GetType());
    }

    [Fact]
    public void ModulesRejectExternalMutation()
    {
        using var host = ModuleTestHost
            .CreateBuilder()
            .UseModule("Sample", new RecordingModule("Sample", []))
            .Build();

        var modules = Assert.IsAssignableFrom<IList<ModuleTestRecord>>(host.Modules);

        Assert.Throws<NotSupportedException>(() => modules[0] = new ModuleTestRecord("Other", new RecordingModule("Other", [])));
        Assert.Equal("Sample", host.Modules[0].Name);
    }

    private sealed class RecordingModule : ModuleBase
    {
        private readonly List<string> _calls;
        private readonly string _name;

        public RecordingModule(string name, List<string> calls)
        {
            _name = name;
            _calls = calls;
        }

        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            _calls.Add($"{_name}:PreConfigureServices");
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            _calls.Add($"{_name}:ConfigureServices");
        }

        public override void PostConfigureServices(ServiceConfigurationContext context)
        {
            _calls.Add($"{_name}:PostConfigureServices");
        }

        public override void ConfigureContributions(ContributionConfigurationContext context)
        {
            _calls.Add($"{_name}:ConfigureContributions");
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            _calls.Add($"{_name}:OnPreApplicationInitialization");
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            _calls.Add($"{_name}:OnApplicationInitialization");
        }

        public override void OnPostApplicationInitialization(ApplicationInitializationContext context)
        {
            _calls.Add($"{_name}:OnPostApplicationInitialization");
        }

        public override void OnApplicationShutdown(ApplicationShutdownContext context)
        {
            _calls.Add($"{_name}:OnApplicationShutdown");
        }
    }
}
