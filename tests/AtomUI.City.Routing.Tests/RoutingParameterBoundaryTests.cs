using AtomUI.City.Routing;

namespace AtomUI.City.Routing.Tests;

public sealed class RoutingParameterBoundaryTests
{
    [Fact]
    public void NavigationTargetParametersRejectExternalMutation()
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = "42",
        };
        var target = NavigationTarget.FromRouteReference("profile", parameters, NavigationOptions.Default);
        var exposedParameters = Assert.IsAssignableFrom<IDictionary<string, string>>(target.Parameters);

        parameters["id"] = "99";

        Assert.Throws<NotSupportedException>(() => exposedParameters["id"] = "99");
        Assert.Equal("42", target.Parameters["id"]);
    }

    [Fact]
    public void RouteMatchParametersRejectExternalMutation()
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = "42",
        };
        var match = RouteMatch.Success(Route("profile"), parameters);
        var exposedParameters = Assert.IsAssignableFrom<IDictionary<string, string>>(match.Parameters);

        parameters["id"] = "99";

        Assert.Throws<NotSupportedException>(() => exposedParameters["id"] = "99");
        Assert.Equal("42", match.Parameters["id"]);
    }

    [Fact]
    public void NavigationResultParametersRejectExternalMutation()
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = "42",
        };
        var route = Route("profile");
        var target = NavigationTarget.FromRouteReference("profile", parameters, NavigationOptions.Default);
        var result = NavigationResult.Success(Guid.NewGuid(), target, route, parameters);
        var exposedParameters = Assert.IsAssignableFrom<IDictionary<string, string>>(result.Parameters);

        parameters["id"] = "99";

        Assert.Throws<NotSupportedException>(() => exposedParameters["id"] = "99");
        Assert.Equal("42", result.Parameters["id"]);
    }

    [Fact]
    public void NavigationSnapshotParametersRejectExternalMutation()
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = "42",
        };
        var snapshot = NavigationSnapshot.FromRoute(Route("profile"), parameters, routeGraphVersion: 1);
        var exposedParameters = Assert.IsAssignableFrom<IDictionary<string, string>>(snapshot.Parameters);

        parameters["id"] = "99";

        Assert.Throws<NotSupportedException>(() => exposedParameters["id"] = "99");
        Assert.Equal("42", snapshot.Parameters["id"]);
    }

    [Fact]
    public void RouteGuardContextParametersRejectExternalMutation()
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = "42",
        };
        var route = Route("profile");
        var target = NavigationTarget.FromRouteReference("profile", parameters, NavigationOptions.Default);
        var snapshot = NavigationSnapshot.FromRoute(route, parameters, routeGraphVersion: 1);
        var context = new RouteGuardContext(Guid.NewGuid(), target, route, snapshot, parameters);
        var exposedParameters = Assert.IsAssignableFrom<IDictionary<string, string>>(context.Parameters);

        parameters["id"] = "99";

        Assert.Throws<NotSupportedException>(() => exposedParameters["id"] = "99");
        Assert.Equal("42", context.Parameters["id"]);
    }

    private static RouteDescriptor Route(string id)
    {
        return new RouteDescriptor(
            id,
            RouteDefinitionKind.Route,
            $"{id}/{{id:int}}",
            new ViewModelTargetDescriptor(typeof(ProfileViewModel)));
    }

    private sealed class ProfileViewModel;
}
