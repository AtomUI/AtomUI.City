namespace AtomUI.City.Generators.Routing;

public sealed class RouteManifest
{
    public RouteManifest(IReadOnlyList<RouteManifestRoute> routes)
    {
        Routes = Array.AsReadOnly((routes ?? throw new ArgumentNullException(nameof(routes))).ToArray());
    }

    public IReadOnlyList<RouteManifestRoute> Routes { get; }
}
