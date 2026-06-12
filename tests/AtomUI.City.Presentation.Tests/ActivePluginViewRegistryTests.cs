using AtomUI.City.Diagnostics;
using AtomUI.City.Presentation;
using AtomUI.City.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Presentation.Tests;

public sealed class ActivePluginViewRegistryTests
{
    [Fact]
    public void ServiceCollectionRegistersActivePluginViewRegistry()
    {
        var services = new ServiceCollection();

        services.AddActivePluginViewRegistry();

        using var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<IActivePluginViewRegistry>();

        Assert.Same(provider.GetRequiredService<ActivePluginViewRegistry>(), registry);
    }

    [Fact]
    public void TrackReturnsLeaseAndRecordsDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var registry = new ActivePluginViewRegistry(diagnostics);
        var outlet = new RouteOutlet("primary", new InlineDispatcher());
        var handle = BoundViewHandle.FromExisting(new SettingsView(), new SettingsViewModel());

        var lease = registry.Track(new ActivePluginView(
            "com.company.sales",
            outlet,
            handle,
            contributionId: "sales.settings"));

        Assert.Equal([lease.View], registry.ActiveViews);
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.PluginViewTracked &&
                record.Severity == HostDiagnosticSeverity.Info &&
                record.Message.Contains("com.company.sales", StringComparison.Ordinal) &&
                record.Message.Contains("sales.settings", StringComparison.Ordinal));

        lease.Dispose();

        Assert.Empty(registry.ActiveViews);
    }

    [Fact]
    public async Task ClosePluginViewsClearsOnlyMatchingPluginOutlets()
    {
        var registry = new ActivePluginViewRegistry();
        var salesOutlet = new RouteOutlet("primary", new InlineDispatcher());
        var supportOutlet = new RouteOutlet("secondary", new InlineDispatcher());
        var salesHandle = BoundViewHandle.FromExisting(new SettingsView(), new SettingsViewModel());
        var supportHandle = BoundViewHandle.FromExisting(new SettingsView(), new SettingsViewModel());
        await salesOutlet.CommitAsync(RouteOutletCommitPlan.Replace("primary", salesHandle));
        await supportOutlet.CommitAsync(RouteOutletCommitPlan.Replace("secondary", supportHandle));
        registry.Track(new ActivePluginView(
            "com.company.sales",
            salesOutlet,
            salesHandle,
            contributionId: "sales.settings"));
        registry.Track(new ActivePluginView(
            "com.company.support",
            supportOutlet,
            supportHandle,
            contributionId: "support.settings"));

        var closed = await registry.ClosePluginViewsAsync("com.company.sales");

        Assert.Equal(1, closed);
        Assert.Null(salesOutlet.CurrentContent);
        Assert.True(salesHandle.IsDisposed);
        Assert.Same(supportHandle.View, supportOutlet.CurrentContent);
        Assert.False(supportHandle.IsDisposed);
        Assert.Equal(["com.company.support"], registry.ActiveViews.Select(view => view.PluginId));
    }

    [Fact]
    public async Task CloseContributionViewsClearsMatchingContribution()
    {
        var registry = new ActivePluginViewRegistry();
        var outlet = new RouteOutlet("primary", new InlineDispatcher());
        var handle = BoundViewHandle.FromExisting(new SettingsView(), new SettingsViewModel());
        await outlet.CommitAsync(RouteOutletCommitPlan.Replace("primary", handle));
        registry.Track(new ActivePluginView(
            "com.company.sales",
            outlet,
            handle,
            contributionId: "sales.settings"));

        var closed = await registry.CloseContributionViewsAsync("sales.settings");

        Assert.Equal(1, closed);
        Assert.Null(outlet.CurrentContent);
        Assert.True(handle.IsDisposed);
        Assert.Empty(registry.ActiveViews);
    }

    [Fact]
    public async Task ClosePluginViewsDoesNotClearOutletWhenTrackedViewWasReplaced()
    {
        var registry = new ActivePluginViewRegistry();
        var outlet = new RouteOutlet("primary", new InlineDispatcher());
        var pluginHandle = BoundViewHandle.FromExisting(new SettingsView(), new SettingsViewModel());
        var hostHandle = BoundViewHandle.FromExisting(new HostView(), new HostViewModel());
        await outlet.CommitAsync(RouteOutletCommitPlan.Replace("primary", pluginHandle));
        registry.Track(new ActivePluginView(
            "com.company.sales",
            outlet,
            pluginHandle,
            contributionId: "sales.settings"));
        await outlet.CommitAsync(RouteOutletCommitPlan.Replace("primary", hostHandle));

        var closed = await registry.ClosePluginViewsAsync("com.company.sales");

        Assert.Equal(0, closed);
        Assert.Same(hostHandle.View, outlet.CurrentContent);
        Assert.False(hostHandle.IsDisposed);
        Assert.Empty(registry.ActiveViews);
    }

    [Fact]
    public async Task ClosePluginViewsRecordsFailureAndContinuesClosingOtherViews()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var registry = new ActivePluginViewRegistry(diagnostics);
        var failingOutlet = new FailingOutlet("failing", RouteOutletCommitResult.Failed(
            PresentationError.OutletCommitFailed,
            "outlet close failed"));
        var healthyOutlet = new RouteOutlet("healthy", new InlineDispatcher());
        var failingHandle = BoundViewHandle.FromExisting(new SettingsView(), new SettingsViewModel());
        var healthyHandle = BoundViewHandle.FromExisting(new SettingsView(), new SettingsViewModel());
        failingOutlet.CurrentContent = failingHandle.View;
        await healthyOutlet.CommitAsync(RouteOutletCommitPlan.Replace("healthy", healthyHandle));
        registry.Track(new ActivePluginView(
            "com.company.sales",
            failingOutlet,
            failingHandle,
            contributionId: "sales.failing"));
        registry.Track(new ActivePluginView(
            "com.company.sales",
            healthyOutlet,
            healthyHandle,
            contributionId: "sales.healthy"));

        var closed = await registry.ClosePluginViewsAsync("com.company.sales");

        Assert.Equal(1, closed);
        Assert.Same(failingHandle.View, failingOutlet.CurrentContent);
        Assert.False(failingHandle.IsDisposed);
        Assert.Null(healthyOutlet.CurrentContent);
        Assert.True(healthyHandle.IsDisposed);
        Assert.Equal(["sales.failing"], registry.ActiveViews.Select(view => view.ContributionId));
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.PluginViewCloseFailed &&
                record.Severity == HostDiagnosticSeverity.Error &&
                record.Message.Contains("outlet close failed", StringComparison.Ordinal));
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.PluginViewClosed &&
                record.Severity == HostDiagnosticSeverity.Info &&
                record.Message.Contains("sales.healthy", StringComparison.Ordinal));
    }

    private sealed class InlineDispatcher : IUiDispatcher
    {
        public bool CheckAccess() => true;

        public ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default)
        {
            callback();

            return ValueTask.CompletedTask;
        }

        public ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(callback());
        }

        public ValueTask PostAsync(
            Func<CancellationToken, ValueTask> callback,
            CancellationToken cancellationToken = default)
        {
            return callback(cancellationToken);
        }
    }

    private sealed class FailingOutlet(string name, RouteOutletCommitResult result) : IRouteOutlet
    {
        public string Name { get; } = name;

        public object? CurrentContent { get; set; }

        public ValueTask<RouteOutletCommitResult> CommitAsync(
            RouteOutletCommitPlan plan,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(result);
        }
    }

    private sealed class SettingsViewModel;

    private sealed class SettingsView;

    private sealed class HostViewModel;

    private sealed class HostView;
}
