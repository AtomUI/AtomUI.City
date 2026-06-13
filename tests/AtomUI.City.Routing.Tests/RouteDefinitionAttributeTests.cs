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
            TitleKey = "Routes.Settings.Title",
            DescriptionKey = "Routes.Settings.Description",
            BreadcrumbKey = "Routes.Settings.Breadcrumb",
            GroupKey = "Routes.Settings.Group",
            ErrorTitleKey = "Routes.Settings.ErrorTitle",
        };

        Assert.Equal(RouteDefinitionKind.Route, attribute.Kind);
        Assert.Equal("settings", attribute.Template);
        Assert.Equal(typeof(SettingsViewModel), attribute.ViewModelType);
        Assert.Equal("app.settings", attribute.Id);
        Assert.Equal("Shell", attribute.Parent);
        Assert.Equal("side", attribute.Outlet);
        Assert.Equal("settings.pages", attribute.ExtensionPoint);
        Assert.Equal("Routes.Settings.Title", attribute.TitleKey);
        Assert.Equal("Routes.Settings.Description", attribute.DescriptionKey);
        Assert.Equal("Routes.Settings.Breadcrumb", attribute.BreadcrumbKey);
        Assert.Equal("Routes.Settings.Group", attribute.GroupKey);
        Assert.Equal("Routes.Settings.ErrorTitle", attribute.ErrorTitleKey);
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

    [Fact]
    public void RouteReferenceBindParametersReturnsReadonlyCopy()
    {
        var boundParameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["section"] = "profile",
        };
        var route = new RouteReference<SettingsParameters>(
            "app.settings.details",
            _ => boundParameters);
        var parameters = route.BindParameters(new SettingsParameters("profile"));
        var exposedParameters = Assert.IsAssignableFrom<IDictionary<string, string>>(parameters);

        boundParameters["section"] = "changed";

        Assert.Throws<NotSupportedException>(() => exposedParameters["section"] = "changed");
        Assert.Equal("profile", parameters["section"]);
    }

    private sealed class ShellViewModel;

    private sealed class HomeViewModel;

    private sealed class SettingsViewModel;

    private readonly record struct SettingsParameters(string Section);
}
