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
