using System.Globalization;

using AtomUI.City.Routing;

namespace AtomUI.City.Routing.Tests;

public sealed class NavigationScopeTests
{
    [Fact]
    public async Task NavigateByPathUpdatesCurrentSnapshot()
    {
        var graph = RouteGraphSnapshot.Create(
            [
                Layout("shell", typeof(ShellViewModel)),
                Route("settings", "settings", typeof(SettingsViewModel), parentRouteId: "shell"),
            ]);
        var scope = new NavigationScope(graph);

        var result = await scope.Router.NavigateByPathAsync("settings");

        Assert.Equal(NavigationResultStatus.Success, result.Status);
        Assert.Equal("settings", result.Route.RouteId);
        Assert.Equal("settings", scope.CurrentSnapshot.Route.RouteId);
        Assert.Equal(NavigationTargetKind.Path, result.Target.Kind);
    }

    [Fact]
    public async Task NavigateByRouteReferenceFindsRouteById()
    {
        var graph = RouteGraphSnapshot.Create(
            [
                Layout("shell", typeof(ShellViewModel)),
                Route("settings", "settings", typeof(SettingsViewModel), parentRouteId: "shell"),
            ]);
        var scope = new NavigationScope(graph);

        var result = await scope.Router.NavigateAsync(new RouteReference("settings"));

        Assert.Equal(NavigationResultStatus.Success, result.Status);
        Assert.Equal("settings", result.Route.RouteId);
        Assert.Equal(NavigationTargetKind.RouteReference, result.Target.Kind);
        Assert.Equal("settings", scope.CurrentSnapshot.Route.RouteId);
    }

    [Fact]
    public async Task NavigateByRouteReferenceBindsTypedParametersThroughRouteReferenceBinder()
    {
        var graph = RouteGraphSnapshot.Create(
            [
                Layout("shell", typeof(ShellViewModel)),
                Route("profile", "profile/{id:int}", typeof(ProfileViewModel), parentRouteId: "shell"),
            ]);
        var scope = new NavigationScope(graph);
        var route = new RouteReference<ProfileParameters>(
            "profile",
            parameters => new Dictionary<string, string>
            {
                ["id"] = parameters.Id.ToString(CultureInfo.InvariantCulture),
            });

        var result = await scope.Router.NavigateAsync(route, new ProfileParameters(42));

        Assert.Equal(NavigationResultStatus.Success, result.Status);
        Assert.Equal("profile", result.Route.RouteId);
        Assert.Equal("42", result.Parameters["id"]);
        Assert.Equal("42", scope.CurrentSnapshot.Parameters["id"]);
    }

    [Fact]
    public async Task NavigateByPathReturnsNotFoundWithoutChangingCurrentSnapshot()
    {
        var graph = RouteGraphSnapshot.Create(
            [
                Route("settings", "settings", typeof(SettingsViewModel)),
            ]);
        var scope = new NavigationScope(graph);

        var result = await scope.Router.NavigateByPathAsync("missing");

        Assert.Equal(NavigationResultStatus.NotFound, result.Status);
        Assert.Equal("CITY-NAVIGATION-NOT-FOUND", result.Error?.Code);
        Assert.Null(scope.CurrentSnapshot.ActiveRoute);
    }

    [Fact]
    public async Task CancelledNavigationReturnsCancelledResultWithoutChangingCurrentSnapshot()
    {
        var graph = RouteGraphSnapshot.Create(
            [
                Route("settings", "settings", typeof(SettingsViewModel)),
            ]);
        var scope = new NavigationScope(graph);
        using var cancellationTokenSource = new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        var result = await scope.Router.NavigateByPathAsync(
            "settings",
            cancellationToken: cancellationTokenSource.Token);

        Assert.Equal(NavigationResultStatus.Cancelled, result.Status);
        Assert.Null(scope.CurrentSnapshot.ActiveRoute);
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

    private readonly record struct ProfileParameters(int Id);
}
