using AtomUI.City.Routing;

namespace AtomUI.City.Routing.Tests;

public sealed class RouteDefinitionAttributeTests
{
    [Fact]
    public void RouteAttributeStoresTemplateViewModelAndCommonMetadata()
    {
        var attribute = new RouteAttribute("settings", typeof(SettingsViewModel))
        {
            Id = "app.settings",
            Parent = "Shell",
            Outlet = "side",
            ExtensionPoint = "settings.pages",
        };

        Assert.Equal(RouteDefinitionKind.Route, attribute.Kind);
        Assert.Equal("settings", attribute.Template);
        Assert.Equal(typeof(SettingsViewModel), attribute.ViewModelType);
        Assert.Equal("app.settings", attribute.Id);
        Assert.Equal("Shell", attribute.Parent);
        Assert.Equal("side", attribute.Outlet);
        Assert.Equal("settings.pages", attribute.ExtensionPoint);
    }

    [Fact]
    public void LayoutAndIndexRoutesUsePrimaryOutletByDefault()
    {
        var layout = new LayoutRouteAttribute(typeof(ShellViewModel));
        var index = new IndexRouteAttribute(typeof(HomeViewModel));

        Assert.Equal(RouteDefinitionKind.Layout, layout.Kind);
        Assert.Equal("primary", layout.Outlet);
        Assert.Equal(RouteDefinitionKind.Index, index.Kind);
        Assert.Equal("primary", index.Outlet);
    }

    [Fact]
    public void GroupRedirectAndExtensionPointRoutesStoreSpecificMetadata()
    {
        var group = new RouteGroupAttribute("admin");
        var redirect = new RedirectRouteAttribute("old-settings")
        {
            Target = "Settings",
        };
        var extensionPoint = new RouteExtensionPointAttribute("settings.pages");

        Assert.Equal(RouteDefinitionKind.Group, group.Kind);
        Assert.Equal("admin", group.Template);
        Assert.Null(group.ViewModelType);
        Assert.Equal(RouteDefinitionKind.Redirect, redirect.Kind);
        Assert.Equal("old-settings", redirect.Template);
        Assert.Equal("Settings", redirect.Target);
        Assert.Equal(RouteDefinitionKind.ExtensionPoint, extensionPoint.Kind);
        Assert.Equal("settings.pages", extensionPoint.ExtensionPoint);
    }

    [Fact]
    public void RouteReferenceStoresRouteId()
    {
        var route = new RouteReference("app.settings");
        var parameterizedRoute = new RouteReference<SettingsParameters>("app.settings.details");

        Assert.Equal("app.settings", route.Id);
        Assert.Equal("app.settings.details", parameterizedRoute.Id);
    }

    private sealed class ShellViewModel;

    private sealed class HomeViewModel;

    private sealed class SettingsViewModel;

    private readonly record struct SettingsParameters(string Section);
}
