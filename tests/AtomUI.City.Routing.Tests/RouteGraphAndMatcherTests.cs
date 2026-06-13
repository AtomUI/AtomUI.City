using AtomUI.City.Routing;

namespace AtomUI.City.Routing.Tests;

public sealed class RouteGraphAndMatcherTests
{
    [Fact]
    public void GraphBuildsParentChildIndexesAndFindsRoutesById()
    {
        var snapshot = RouteGraphSnapshot.Create(
            [
                Layout("shell", typeof(ShellViewModel)),
                Route("settings", "settings", typeof(SettingsViewModel), parentRouteId: "shell"),
                Route("profile", "profile/{id:int}", typeof(ProfileViewModel), parentRouteId: "shell"),
            ]);

        Assert.Equal(1, snapshot.Version);
        Assert.Equal("shell", snapshot.GetRequiredRoute("shell").RouteId);
        Assert.Equal(["settings", "profile"], snapshot.GetChildren("shell").Select(route => route.RouteId));
    }

    [Fact]
    public void MatcherPrefersLiteralRoutesOverParameterRoutes()
    {
        var snapshot = RouteGraphSnapshot.Create(
            [
                Layout("shell", typeof(ShellViewModel)),
                Route("settings", "settings", typeof(SettingsViewModel), parentRouteId: "shell"),
                Route("dynamic", "{section}", typeof(DynamicViewModel), parentRouteId: "shell"),
            ]);

        var match = snapshot.Matcher.Match("settings");

        Assert.Equal(RouteMatchStatus.Success, match.Status);
        Assert.Equal("settings", match.Route.RouteId);
    }

    [Fact]
    public void MatcherReturnsParametersForMatchedRoute()
    {
        var snapshot = RouteGraphSnapshot.Create(
            [
                Layout("shell", typeof(ShellViewModel)),
                Route("profile", "profile/{id:int}", typeof(ProfileViewModel), parentRouteId: "shell"),
            ]);

        var match = snapshot.Matcher.Match("profile/42");

        Assert.Equal(RouteMatchStatus.Success, match.Status);
        Assert.Equal("profile", match.Route.RouteId);
        Assert.Equal("42", match.Parameters["id"]);
    }

    [Fact]
    public void GraphRejectsDuplicateRouteIds()
    {
        var exception = Assert.Throws<RouteGraphException>(
            () => RouteGraphSnapshot.Create(
                [
                    Route("settings", "settings", typeof(SettingsViewModel)),
                    Route("settings", "settings/profile", typeof(ProfileViewModel)),
                ]));

        Assert.Equal(RouteGraphError.DuplicateRouteId, exception.Error);
    }

    [Fact]
    public void RouteDescriptorStoresLocalizationMetadata()
    {
        var descriptor = new RouteDescriptor(
            "settings",
            RouteDefinitionKind.Route,
            "settings",
            new ViewModelTargetDescriptor(typeof(SettingsViewModel)),
            metadata: new RouteMetadataDescriptor(
                titleKey: "Routes.Settings.Title",
                descriptionKey: "Routes.Settings.Description",
                breadcrumbKey: "Routes.Settings.Breadcrumb",
                groupKey: "Routes.Settings.Group",
                errorTitleKey: "Routes.Settings.ErrorTitle"));

        Assert.Equal("Routes.Settings.Title", descriptor.Metadata.TitleKey);
        Assert.Equal("Routes.Settings.Description", descriptor.Metadata.DescriptionKey);
        Assert.Equal("Routes.Settings.Breadcrumb", descriptor.Metadata.BreadcrumbKey);
        Assert.Equal("Routes.Settings.Group", descriptor.Metadata.GroupKey);
        Assert.Equal("Routes.Settings.ErrorTitle", descriptor.Metadata.ErrorTitleKey);
    }

    [Fact]
    public void RouteDescriptorGuardCollectionsRejectExternalListMutation()
    {
        Type[] enterGuards = [typeof(SettingsViewModel)];
        Type[] leaveGuards = [typeof(ProfileViewModel)];
        Type[] matchPolicies = [typeof(DynamicViewModel)];
        var descriptor = new RouteDescriptor(
            "settings",
            RouteDefinitionKind.Route,
            "settings",
            new ViewModelTargetDescriptor(typeof(SettingsViewModel)),
            enterGuardTypes: enterGuards,
            leaveGuardTypes: leaveGuards,
            matchPolicyTypes: matchPolicies);
        var enterGuardList = Assert.IsAssignableFrom<IList<Type>>(descriptor.EnterGuardTypes);
        var leaveGuardList = Assert.IsAssignableFrom<IList<Type>>(descriptor.LeaveGuardTypes);
        var matchPolicyList = Assert.IsAssignableFrom<IList<Type>>(descriptor.MatchPolicyTypes);

        Assert.Throws<NotSupportedException>(() => enterGuardList[0] = typeof(ProfileViewModel));
        Assert.Throws<NotSupportedException>(() => leaveGuardList[0] = typeof(SettingsViewModel));
        Assert.Throws<NotSupportedException>(() => matchPolicyList[0] = typeof(SettingsViewModel));
        Assert.Equal(typeof(SettingsViewModel), descriptor.EnterGuardTypes[0]);
        Assert.Equal(typeof(ProfileViewModel), descriptor.LeaveGuardTypes[0]);
        Assert.Equal(typeof(DynamicViewModel), descriptor.MatchPolicyTypes[0]);
    }

    [Fact]
    public void RouteGraphCollectionsRejectExternalListMutation()
    {
        var shell = Layout("shell", typeof(ShellViewModel));
        var settings = Route("settings", "settings", typeof(SettingsViewModel), parentRouteId: "shell");
        var replacement = Route("replacement", "replacement", typeof(DynamicViewModel), parentRouteId: "shell");
        var snapshot = RouteGraphSnapshot.Create([shell, settings]);
        var routes = Assert.IsAssignableFrom<IList<RouteDescriptor>>(snapshot.Routes);
        var children = Assert.IsAssignableFrom<IList<RouteDescriptor>>(snapshot.GetChildren("shell"));

        Assert.Throws<NotSupportedException>(() => routes[0] = replacement);
        Assert.Throws<NotSupportedException>(() => children[0] = replacement);
        Assert.Equal(shell.RouteId, snapshot.Routes[0].RouteId);
        Assert.Equal(settings.RouteId, snapshot.GetChildren("shell")[0].RouteId);
    }

    private static RouteDescriptor Layout(string id, Type viewModelType)
    {
        return new RouteDescriptor(
            id,
            RouteDefinitionKind.Layout,
            template: null,
            new ViewModelTargetDescriptor(viewModelType),
            parentRouteId: null);
    }

    private static RouteDescriptor Route(
        string id,
        string template,
        Type viewModelType,
        string? parentRouteId = null)
    {
        return new RouteDescriptor(
            id,
            RouteDefinitionKind.Route,
            template,
            new ViewModelTargetDescriptor(viewModelType),
            parentRouteId);
    }

    private sealed class ShellViewModel;

    private sealed class SettingsViewModel;

    private sealed class ProfileViewModel;

    private sealed class DynamicViewModel;
}
