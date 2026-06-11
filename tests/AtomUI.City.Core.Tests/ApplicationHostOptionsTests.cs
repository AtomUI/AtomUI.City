using AtomUI.City.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AtomUI.City.Core.Tests;

public sealed class ApplicationHostOptionsTests
{
    [Fact]
    public async Task ConfigureHostRegistersApplicationHostOptions()
    {
        var builder = ApplicationHost.CreateBuilder();

        builder.ConfigureHost(options =>
        {
            options.ApplicationName = "Sample.Desktop";
            options.ShutdownTimeout = TimeSpan.FromSeconds(5);
            options.AllowDynamicDiscovery = true;
        });

        await using var host = builder.Build();

        var options = host.Services.GetRequiredService<IOptions<ApplicationHostOptions>>().Value;

        Assert.Equal("Sample.Desktop", options.ApplicationName);
        Assert.Equal(TimeSpan.FromSeconds(5), options.ShutdownTimeout);
        Assert.True(options.AllowDynamicDiscovery);
        Assert.Equal("Sample.Desktop", host.Context.ApplicationName);
    }

    [Fact]
    public void ApplicationHostOptionsExposeConservativeDefaults()
    {
        var options = new ApplicationHostOptions();

        Assert.Null(options.ApplicationName);
        Assert.Equal(TimeSpan.FromSeconds(30), options.ShutdownTimeout);
        Assert.False(options.AllowDynamicDiscovery);
    }
}
