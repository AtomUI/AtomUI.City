using AtomUI.City.Diagnostics;
using AtomUI.City.Presentation;

namespace AtomUI.City.Presentation.Tests;

public sealed class ViewLocatorTests
{
    [Fact]
    public void RegistryLocatesDefaultViewForViewModel()
    {
        var registry = new ViewRegistry();
        var descriptor = new ViewDescriptor(
            typeof(SettingsViewModel),
            typeof(SettingsView),
            viewKey: null,
            _ => new SettingsView());

        registry.Register(descriptor);

        Assert.True(registry.TryLocate(typeof(SettingsViewModel), out var located));
        Assert.Same(descriptor, located);
        Assert.Same(descriptor, registry.Locate(typeof(SettingsViewModel)));
    }

    [Fact]
    public void RegistrySupportsNamedViewsForSameViewModel()
    {
        var registry = new ViewRegistry();
        var defaultDescriptor = new ViewDescriptor(
            typeof(SettingsViewModel),
            typeof(SettingsView),
            viewKey: null,
            _ => new SettingsView());
        var compactDescriptor = new ViewDescriptor(
            typeof(SettingsViewModel),
            typeof(CompactSettingsView),
            "compact",
            _ => new CompactSettingsView());

        registry.Register(defaultDescriptor);
        registry.Register(compactDescriptor);

        Assert.Same(defaultDescriptor, registry.Locate(typeof(SettingsViewModel)));
        Assert.Same(compactDescriptor, registry.Locate(typeof(SettingsViewModel), "compact"));
    }

    [Fact]
    public void RegistryRejectsDuplicateDefaultViews()
    {
        var registry = new ViewRegistry();
        registry.Register(
            new ViewDescriptor(
                typeof(SettingsViewModel),
                typeof(SettingsView),
                viewKey: null,
                _ => new SettingsView()));

        var exception = Assert.Throws<PresentationException>(
            () => registry.Register(
                new ViewDescriptor(
                    typeof(SettingsViewModel),
                    typeof(AlternativeSettingsView),
                    viewKey: null,
                    _ => new AlternativeSettingsView())));

        Assert.Equal(PresentationError.DuplicateView, exception.Error);
    }

    [Fact]
    public void RegistryReportsMissingView()
    {
        var registry = new ViewRegistry();

        var exception = Assert.Throws<PresentationException>(
            () => registry.Locate(typeof(SettingsViewModel)));

        Assert.Equal(PresentationError.ViewNotFound, exception.Error);
    }

    [Fact]
    public void RegistryRecordsLocateHitDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var registry = new ViewRegistry(diagnostics);
        var descriptor = new ViewDescriptor(
            typeof(SettingsViewModel),
            typeof(SettingsView),
            viewKey: null,
            _ => new SettingsView());
        registry.Register(descriptor);

        var located = registry.Locate(typeof(SettingsViewModel));

        Assert.Same(descriptor, located);
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ViewLocatorMatched &&
                record.Severity == HostDiagnosticSeverity.Info &&
                record.Message.Contains(typeof(SettingsViewModel).FullName!, StringComparison.Ordinal) &&
                record.Message.Contains(typeof(SettingsView).FullName!, StringComparison.Ordinal));
    }

    [Fact]
    public void RegistryRecordsLocateFailureDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var registry = new ViewRegistry(diagnostics);

        Assert.Throws<PresentationException>(() => registry.Locate(typeof(SettingsViewModel)));

        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ViewLocatorFailed &&
                record.Severity == HostDiagnosticSeverity.Warning &&
                record.Message.Contains(typeof(SettingsViewModel).FullName!, StringComparison.Ordinal));
    }

    [Fact]
    public void RegistryRevokesViewsByContributionId()
    {
        var registry = new ViewRegistry();
        registry.Register(
            new ViewDescriptor(
                typeof(SettingsViewModel),
                typeof(SettingsView),
                viewKey: null,
                _ => new SettingsView(),
                contributionId: "plugin.settings"));

        Assert.True(registry.TryLocate(typeof(SettingsViewModel), out _));

        registry.RevokeContribution("plugin.settings");

        Assert.False(registry.TryLocate(typeof(SettingsViewModel), out _));
    }

    [Fact]
    public void RegistryRevokesViewsByPluginId()
    {
        var registry = new ViewRegistry();
        registry.Register(
            new ViewDescriptor(
                typeof(SettingsViewModel),
                typeof(SettingsView),
                viewKey: null,
                _ => new SettingsView(),
                pluginId: "com.company.sales",
                contributionId: "plugin.settings"));
        var hostDescriptor = new ViewDescriptor(
            typeof(ProfileViewModel),
            typeof(ProfileView),
            viewKey: null,
            _ => new ProfileView(),
            pluginId: "com.company.host",
            contributionId: "host.profile");
        registry.Register(hostDescriptor);

        var revoked = registry.RevokePlugin("com.company.sales");

        Assert.Equal(1, revoked);
        Assert.False(registry.TryLocate(typeof(SettingsViewModel), out _));
        Assert.Same(hostDescriptor, registry.Locate(typeof(ProfileViewModel)));
    }

    [Fact]
    public void ViewForAttributeStoresPluginContributionMetadata()
    {
        var attribute = new ViewForAttribute(typeof(SettingsViewModel))
        {
            Key = "compact",
            PluginId = "com.company.sales",
            ContributionId = "plugin.settings",
        };

        Assert.Equal(typeof(SettingsViewModel), attribute.ViewModelType);
        Assert.Equal("compact", attribute.Key);
        Assert.Equal("com.company.sales", attribute.PluginId);
        Assert.Equal("plugin.settings", attribute.ContributionId);
    }

    private sealed class SettingsViewModel;

    private sealed class ProfileViewModel;

    private sealed class SettingsView;

    private sealed class ProfileView;

    private sealed class CompactSettingsView;

    private sealed class AlternativeSettingsView;
}
