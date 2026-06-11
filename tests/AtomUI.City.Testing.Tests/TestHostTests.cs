using AtomUI.City.Hosting;
using AtomUI.City.Testing;

namespace AtomUI.City.Testing.Tests;

public sealed class TestHostTests
{
    [Fact]
    public void CreateBuilderBuildsHostWithDefaultRuntimeFakes()
    {
        using var host = TestHost
            .CreateBuilder()
            .UseProperty("environment", "test")
            .Build();

        Assert.IsType<ApplicationContext>(host.ApplicationContext);
        Assert.Equal("test", host.ApplicationContext.Properties["environment"]);
        Assert.True(Directory.Exists(host.Directory.RootPath));
        Assert.NotNull(host.Dispatcher);
        Assert.NotNull(host.Scheduler);
        Assert.NotNull(host.Diagnostics);
    }

    [Fact]
    public async Task StopAsyncIsIdempotent()
    {
        await using var host = TestHost.CreateBuilder().Build();

        await host.StopAsync();
        await host.StopAsync();

        Assert.True(host.IsStopped);
    }

    [Fact]
    public void DisposeRemovesTestDirectory()
    {
        string rootPath;

        using (var host = TestHost.CreateBuilder().Build())
        {
            rootPath = host.Directory.RootPath;

            Assert.True(Directory.Exists(rootPath));
        }

        Assert.False(Directory.Exists(rootPath));
    }
}
