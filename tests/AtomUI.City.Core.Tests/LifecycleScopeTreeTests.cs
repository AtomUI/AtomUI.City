using AtomUI.City.Lifecycle;

namespace AtomUI.City.Core.Tests;

public sealed class LifecycleScopeTreeTests
{
    [Fact]
    public void ScopeTreeModelsHostApplicationAndNavigationRuntimeBoundaries()
    {
        using var host = LifecycleScope.CreateRoot(LifecycleScopeKind.Host, "host");
        var application = host.CreateChild(LifecycleScopeKind.Application, "application");
        var navigation = application.CreateChild(LifecycleScopeKind.Navigation, "main-navigation");

        Assert.Null(host.Parent);
        Assert.Same(host, application.Parent);
        Assert.Same(application, navigation.Parent);
        Assert.Equal([application], host.Children);
        Assert.Equal([navigation], application.Children);
        Assert.Equal(LifecycleScopeState.Running, host.State);
        Assert.Equal(LifecycleScopeState.Running, navigation.State);
    }

    [Fact]
    public async Task StoppingParentScopeStopsChildrenAndCancelsTokens()
    {
        await using var host = LifecycleScope.CreateRoot(LifecycleScopeKind.Host, "host");
        var application = host.CreateChild(LifecycleScopeKind.Application, "application");
        var navigation = application.CreateChild(LifecycleScopeKind.Navigation, "main-navigation");

        await host.StopAsync();

        Assert.True(host.CancellationToken.IsCancellationRequested);
        Assert.True(application.CancellationToken.IsCancellationRequested);
        Assert.True(navigation.CancellationToken.IsCancellationRequested);
        Assert.Equal(LifecycleScopeState.Stopped, host.State);
        Assert.Equal(LifecycleScopeState.Stopped, application.State);
        Assert.Equal(LifecycleScopeState.Stopped, navigation.State);
    }

    [Fact]
    public async Task StoppedScopeRejectsNewChildren()
    {
        await using var host = LifecycleScope.CreateRoot(LifecycleScopeKind.Host, "host");

        await host.StopAsync();

        Assert.Throws<InvalidOperationException>(() => host.CreateChild(LifecycleScopeKind.Application, "application"));
    }

    [Fact]
    public void ModuleAndPluginAreNotPublicScopeKinds()
    {
        var scopeKindNames = Enum.GetNames<LifecycleScopeKind>();

        Assert.DoesNotContain("Module", scopeKindNames);
        Assert.DoesNotContain("Plugin", scopeKindNames);
    }
}
