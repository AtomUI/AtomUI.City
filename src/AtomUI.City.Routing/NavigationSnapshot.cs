namespace AtomUI.City.Routing;

public sealed class NavigationSnapshot
{
    private NavigationSnapshot(
        RouteDescriptor? activeRoute,
        IReadOnlyDictionary<string, string> parameters,
        long routeGraphVersion)
    {
        ActiveRoute = activeRoute;
        Parameters = RouteParameters.Copy(parameters);
        RouteGraphVersion = routeGraphVersion;
    }

    public RouteDescriptor Route => ActiveRoute ?? throw new InvalidOperationException("Navigation snapshot does not have an active route.");

    public RouteDescriptor? ActiveRoute { get; }

    public IReadOnlyDictionary<string, string> Parameters { get; }

    public long RouteGraphVersion { get; }

    public static NavigationSnapshot Empty(long routeGraphVersion)
    {
        return new NavigationSnapshot(
            activeRoute: null,
            RouteParameters.Empty(),
            routeGraphVersion);
    }

    public static NavigationSnapshot FromRoute(
        RouteDescriptor activeRoute,
        IReadOnlyDictionary<string, string> parameters,
        long routeGraphVersion)
    {
        ArgumentNullException.ThrowIfNull(activeRoute);
        ArgumentNullException.ThrowIfNull(parameters);

        return new NavigationSnapshot(
            activeRoute,
            parameters,
            routeGraphVersion);
    }
}
