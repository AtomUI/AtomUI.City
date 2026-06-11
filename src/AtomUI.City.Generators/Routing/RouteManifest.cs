namespace AtomUI.City.Generators.Routing;

public sealed class RouteManifest
{
    public RouteManifest(IReadOnlyList<RouteManifestRoute> routes)
    {
        Routes = routes ?? throw new ArgumentNullException(nameof(routes));
    }

    public IReadOnlyList<RouteManifestRoute> Routes { get; }
}
