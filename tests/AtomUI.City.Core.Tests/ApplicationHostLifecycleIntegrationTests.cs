using AtomUI.City.Hosting;
using AtomUI.City.Lifecycle;

namespace AtomUI.City.Core.Tests;

public sealed class ApplicationHostLifecycleIntegrationTests
{
    [Fact]
    public async Task HostStartAndStopManageHostAndApplicationScopes()
    {
        await using var host = ApplicationHost.CreateBuilder().Build();

        Assert.Equal(LifecycleScopeKind.Host, host.HostScope.Kind);
        Assert.Equal(LifecycleScopeState.Running, host.HostScope.State);
        Assert.Null(host.ApplicationScope);

        await host.StartAsync();

        Assert.NotNull(host.ApplicationScope);
        Assert.Equal(LifecycleScopeKind.Application, host.ApplicationScope.Kind);
        Assert.Same(host.HostScope, host.ApplicationScope.Parent);
        Assert.Equal([host.ApplicationScope], host.HostScope.Children);

        await host.StopAsync();

        Assert.Equal(LifecycleScopeState.Stopped, host.ApplicationScope.State);
        Assert.Equal(LifecycleScopeState.Stopped, host.HostScope.State);
    }
}
