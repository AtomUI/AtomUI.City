namespace AtomUI.City.Routing;

public sealed class RouteGuardContext
{
    public RouteGuardContext(
        Guid navigationId,
        NavigationTarget target,
        RouteDescriptor route,
        NavigationSnapshot currentSnapshot,
        IReadOnlyDictionary<string, string> parameters)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(currentSnapshot);
        ArgumentNullException.ThrowIfNull(parameters);

        NavigationId = navigationId;
        Target = target;
        Route = route;
        CurrentSnapshot = currentSnapshot;
        Parameters = RouteParameters.Copy(parameters);
    }

    public Guid NavigationId { get; }

    public NavigationTarget Target { get; }

    public RouteDescriptor Route { get; }

    public NavigationSnapshot CurrentSnapshot { get; }

    public IReadOnlyDictionary<string, string> Parameters { get; }
}
