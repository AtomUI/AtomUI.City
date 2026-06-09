using AtomUI.City.Hosting;
using AtomUI.City.Modularity;

namespace AtomUI.City.Core.Tests;

public sealed class ModuleBaseTests
{
    [Fact]
    public async Task DefaultLifecycleMethodsComplete()
    {
        var applicationContext = new ApplicationContext();
        var moduleContext = new ModuleContext("TestModule", applicationContext);
        var module = new TestModule();

        await module.PreConfigureAsync(moduleContext);
        await module.ConfigureAsync(moduleContext);
        await module.InitializeAsync(moduleContext);
        await module.ShutdownAsync(moduleContext);

        Assert.Equal("TestModule", moduleContext.Name);
        Assert.Same(applicationContext, moduleContext.ApplicationContext);
    }

    private sealed class TestModule : ModuleBase;
}
