namespace AtomUI.City.Routing;

public sealed class RouteMatchPolicyContext
{
    public RouteMatchPolicyContext(
        Guid navigationId,
        NavigationTarget target,
        RouteDescriptor route,
        NavigationSnapshot currentSnapshot)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(currentSnapshot);

        NavigationId = navigationId;
        Target = target;
        Route = route;
        CurrentSnapshot = currentSnapshot;
    }

    public Guid NavigationId { get; }

    public NavigationTarget Target { get; }

    public RouteDescriptor Route { get; }

    public NavigationSnapshot CurrentSnapshot { get; }
}
