using AtomUI.City.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Core.Tests;

public sealed class ApplicationHostBuilderTests
{
    [Fact]
    public void CreateBuilderCreatesConfigurableApplicationHostBuilder()
    {
        var builder = ApplicationHost.CreateBuilder(["--sample:enabled=true"]);

        Assert.IsAssignableFrom<IApplicationHostBuilder>(builder);
        Assert.NotNull(builder.Services);
        Assert.NotNull(builder.Configuration);
        Assert.Equal("true", builder.Configuration["sample:enabled"]);
    }

    [Fact]
    public async Task BuildCreatesApplicationHostWithServicesAndContext()
    {
        var builder = ApplicationHost.CreateBuilder(["--mode=test"]);

        builder.ConfigureServices(services => services.AddSingleton<TestService>());

        await using var host = builder.Build();

        Assert.IsAssignableFrom<IApplicationHost>(host);
        Assert.IsType<TestService>(host.Services.GetRequiredService<TestService>());
        Assert.Same(host.Services, host.Context.Services);
        Assert.Equal(["--mode=test"], host.Context.StartupArguments);
        Assert.Same(host.Context.Configuration, builder.Configuration);
        Assert.False(string.IsNullOrWhiteSpace(host.Context.ApplicationName));
        Assert.True(Directory.Exists(host.Context.ContentRootPath));
    }

    [Fact]
    public async Task StartupArgumentsRejectExternalListMutation()
    {
        await using var host = ApplicationHost.CreateBuilder(["--mode=test"]).Build();
        var arguments = Assert.IsAssignableFrom<IList<string>>(host.Context.StartupArguments);

        Assert.Throws<NotSupportedException>(() => arguments[0] = "--mode=changed");
        Assert.Equal("--mode=test", host.Context.StartupArguments[0]);
    }

    private sealed class TestService;
}
