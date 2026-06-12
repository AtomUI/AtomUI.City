using AtomUI.City.Lifecycle;
using AtomUI.City.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Presentation.Tests;

public sealed class PresentationRuntimeTests
{
    [Fact]
    public async Task StartAsyncCreatesPresentationScopeUnderApplicationScope()
    {
        await using var application = LifecycleScope.CreateRoot(LifecycleScopeKind.Application, "application");
        var runtime = new PresentationRuntime();

        await runtime.StartAsync(application);

        Assert.True(runtime.IsReady);
        Assert.Equal(PresentationRuntimeState.Ready, runtime.State);
        Assert.NotNull(runtime.PresentationScope);
        Assert.Equal(LifecycleScopeKind.Presentation, runtime.PresentationScope.Kind);
        Assert.Same(application, runtime.PresentationScope.Parent);
        Assert.Equal("presentation", runtime.PresentationScope.Id);
    }

    [Fact]
    public void CreateWindowScopeRequiresReadyRuntime()
    {
        var runtime = new PresentationRuntime();

        var exception = Assert.Throws<PresentationException>(() => runtime.CreateWindowScope("main"));

        Assert.Equal(PresentationError.RuntimeNotReady, exception.Error);
    }

    [Fact]
    public async Task CreateWindowScopeCreatesChildUnderPresentationScope()
    {
        await using var application = LifecycleScope.CreateRoot(LifecycleScopeKind.Application, "application");
        var runtime = new PresentationRuntime();

        await runtime.StartAsync(application);
        var window = runtime.CreateWindowScope("main");
        var presentationScope = runtime.PresentationScope!;

        Assert.Equal(LifecycleScopeKind.Window, window.Kind);
        Assert.Equal("main", window.Id);
        Assert.Same(presentationScope, window.Parent);
        Assert.Contains(window, presentationScope.Children);
    }

    [Fact]
    public async Task StopAsyncStopsPresentationScopeAndRejectsNewWindowScopes()
    {
        await using var application = LifecycleScope.CreateRoot(LifecycleScopeKind.Application, "application");
        var runtime = new PresentationRuntime();

        await runtime.StartAsync(application);
        var window = runtime.CreateWindowScope("main");
        await runtime.StopAsync();

        var exception = Assert.Throws<PresentationException>(() => runtime.CreateWindowScope("secondary"));
        var presentationScope = runtime.PresentationScope!;

        Assert.False(runtime.IsReady);
        Assert.Equal(PresentationRuntimeState.Stopped, runtime.State);
        Assert.Equal(LifecycleScopeState.Stopped, presentationScope.State);
        Assert.Equal(LifecycleScopeState.Stopped, window.State);
        Assert.Equal(PresentationError.RuntimeStopping, exception.Error);
    }

    [Fact]
    public async Task ServiceCollectionRegistersPresentationRuntime()
    {
        var services = new ServiceCollection();

        services.AddPresentationRuntime();

        await using var provider = services.BuildServiceProvider();
        var runtime = provider.GetRequiredService<IPresentationRuntime>();

        Assert.IsType<PresentationRuntime>(runtime);
    }
}
