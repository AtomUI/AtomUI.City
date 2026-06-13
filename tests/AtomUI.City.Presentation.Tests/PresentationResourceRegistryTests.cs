using AtomUI.City.Diagnostics;
using AtomUI.City.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Presentation.Tests;

public sealed class PresentationResourceRegistryTests
{
    [Fact]
    public void ServiceCollectionRegistersPresentationResourceRegistry()
    {
        var services = new ServiceCollection();

        services.AddPresentationResourceRegistry();

        using var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<IPresentationResourceRegistry>();

        Assert.Same(provider.GetRequiredService<PresentationResourceRegistry>(), registry);
    }

    [Fact]
    public void RegisterReturnsLeaseAndRecordsDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var registry = new PresentationResourceRegistry(diagnostics);
        var resource = new DisposableResource();

        var lease = registry.Register(new PresentationResourceContribution(
            "style",
            resource,
            pluginId: "com.company.sales",
            contributionId: "sales.style"));

        Assert.Contains(lease.Contribution, registry.Contributions);
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ResourceContributionRegistered &&
                record.Severity == HostDiagnosticSeverity.Info &&
                record.Message.Contains("com.company.sales", StringComparison.Ordinal) &&
                record.Message.Contains("sales.style", StringComparison.Ordinal));

        lease.Dispose();

        Assert.Empty(registry.Contributions);
        Assert.True(resource.IsDisposed);
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ResourceContributionRevoked &&
                record.Severity == HostDiagnosticSeverity.Info &&
                record.Message.Contains("sales.style", StringComparison.Ordinal));
    }

    [Fact]
    public void RevokePluginRemovesOnlyMatchingPluginContributions()
    {
        var registry = new PresentationResourceRegistry();
        registry.Register(new PresentationResourceContribution(
            "style",
            new object(),
            pluginId: "com.company.sales",
            contributionId: "sales.style"));
        var hostLease = registry.Register(new PresentationResourceContribution(
            "style",
            new object(),
            pluginId: "com.company.host",
            contributionId: "host.style"));

        var revoked = registry.RevokePlugin("com.company.sales");

        Assert.Equal(1, revoked);
        Assert.Equal([hostLease.Contribution], registry.Contributions);
    }

    [Fact]
    public void RevokeContributionRemovesMatchingContribution()
    {
        var registry = new PresentationResourceRegistry();
        registry.Register(new PresentationResourceContribution(
            "style",
            new object(),
            pluginId: "com.company.sales",
            contributionId: "sales.style"));

        var revoked = registry.RevokeContribution("sales.style");

        Assert.Equal(1, revoked);
        Assert.Empty(registry.Contributions);
    }

    [Fact]
    public void ContributionsRejectExternalListMutation()
    {
        var registry = new PresentationResourceRegistry();
        var lease = registry.Register(new PresentationResourceContribution(
            "style",
            new object(),
            pluginId: "com.company.sales",
            contributionId: "sales.style"));

        var contributions = Assert.IsAssignableFrom<IList<PresentationResourceContribution>>(registry.Contributions);

        Assert.Throws<NotSupportedException>(
            () => contributions[0] = new PresentationResourceContribution("style", new object()));
        Assert.Same(lease.Contribution, registry.Contributions[0]);
    }

    [Fact]
    public void RevokePluginRecordsFailureAndContinuesRevokingOtherResources()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var registry = new PresentationResourceRegistry(diagnostics);
        var failing = new FailingResource();
        var disposable = new DisposableResource();
        registry.Register(new PresentationResourceContribution(
            "style",
            failing,
            pluginId: "com.company.sales",
            contributionId: "sales.failing"));
        registry.Register(new PresentationResourceContribution(
            "icon",
            disposable,
            pluginId: "com.company.sales",
            contributionId: "sales.icon"));

        var revoked = registry.RevokePlugin("com.company.sales");

        Assert.Equal(2, revoked);
        Assert.Empty(registry.Contributions);
        Assert.True(disposable.IsDisposed);
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ResourceContributionRevokeFailed &&
                record.Severity == HostDiagnosticSeverity.Error &&
                record.Message.Contains("resource dispose failed", StringComparison.Ordinal));
    }

    private sealed class DisposableResource : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    private sealed class FailingResource : IDisposable
    {
        public void Dispose()
        {
            throw new InvalidOperationException("resource dispose failed");
        }
    }
}
