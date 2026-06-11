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
                "First:PreConfigure",
                "Second:PreConfigure",
                "First:Configure",
                "Second:Configure",
                "First:Initialize",
                "Second:Initialize",
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
                "Second:Shutdown",
                "First:Shutdown",
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

    private sealed class RecordingModule : ModuleBase
    {
        private readonly List<string> _calls;
        private readonly string _name;

        public RecordingModule(string name, List<string> calls)
        {
            _name = name;
            _calls = calls;
        }

        public override ValueTask PreConfigureAsync(ModuleContext context)
        {
            _calls.Add($"{_name}:PreConfigure");

            return ValueTask.CompletedTask;
        }

        public override ValueTask ConfigureAsync(ModuleContext context)
        {
            _calls.Add($"{_name}:Configure");

            return ValueTask.CompletedTask;
        }

        public override ValueTask InitializeAsync(ModuleContext context)
        {
            _calls.Add($"{_name}:Initialize");

            return ValueTask.CompletedTask;
        }

        public override ValueTask ShutdownAsync(ModuleContext context)
        {
            _calls.Add($"{_name}:Shutdown");

            return ValueTask.CompletedTask;
        }
    }
}
