namespace AtomUI.City.Testing;

public sealed class RoutingTestHostBuilder
{
    private readonly List<RouteTestDefinition> _routes = [];

    public RoutingTestHostBuilder MapRoute(string name, string pattern, Type viewModelType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);
        ArgumentNullException.ThrowIfNull(viewModelType);

        _routes.Add(new RouteTestDefinition(name, pattern, viewModelType));

        return this;
    }

    public RoutingTestHost Build()
    {
        return new RoutingTestHost(_routes.ToArray());
    }
}
