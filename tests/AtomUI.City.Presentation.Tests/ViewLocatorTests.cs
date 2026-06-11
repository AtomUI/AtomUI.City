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
    public void ViewForAttributeStoresViewModelAndKey()
    {
        var attribute = new ViewForAttribute(typeof(SettingsViewModel))
        {
            Key = "compact",
            ContributionId = "plugin.settings",
        };

        Assert.Equal(typeof(SettingsViewModel), attribute.ViewModelType);
        Assert.Equal("compact", attribute.Key);
        Assert.Equal("plugin.settings", attribute.ContributionId);
    }

    private sealed class SettingsViewModel;

    private sealed class SettingsView;

    private sealed class CompactSettingsView;

    private sealed class AlternativeSettingsView;
}
