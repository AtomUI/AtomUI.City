namespace AtomUI.City.Generators.Routing;

public sealed class RouteMapMetadata
{
    public RouteMapMetadata(string typeName, IReadOnlyList<RouteDefinitionMetadata> routes)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException("Route map type name cannot be empty.", nameof(typeName));
        }

        TypeName = typeName;
        Routes = routes ?? throw new ArgumentNullException(nameof(routes));
    }

    public string TypeName { get; }

    public IReadOnlyList<RouteDefinitionMetadata> Routes { get; }
}
