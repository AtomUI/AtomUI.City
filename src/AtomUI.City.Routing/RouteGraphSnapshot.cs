namespace AtomUI.City.Routing;

public sealed class RouteGraphSnapshot
{
    private readonly IReadOnlyDictionary<string, RouteDescriptor[]> _childrenByParentId;
    private readonly IReadOnlyDictionary<string, RouteDescriptor> _routesById;

    private RouteGraphSnapshot(
        long version,
        IReadOnlyList<RouteDescriptor> routes,
        IReadOnlyDictionary<string, RouteDescriptor> routesById,
        IReadOnlyDictionary<string, RouteDescriptor[]> childrenByParentId)
    {
        Version = version;
        Routes = routes;
        _routesById = routesById;
        _childrenByParentId = childrenByParentId;
        Matcher = new RouteMatcher(this);
    }

    public long Version { get; }

    public IReadOnlyList<RouteDescriptor> Routes { get; }

    public RouteMatcher Matcher { get; }

    public static RouteGraphSnapshot Create(IReadOnlyList<RouteDescriptor> routes)
    {
        return Create(routes, version: 1);
    }

    public static RouteGraphSnapshot Create(IReadOnlyList<RouteDescriptor> routes, long version)
    {
        ArgumentNullException.ThrowIfNull(routes);

        var routesById = new Dictionary<string, RouteDescriptor>(StringComparer.Ordinal);

        foreach (var route in routes)
        {
            if (!routesById.TryAdd(route.RouteId, route))
            {
                throw new RouteGraphException(
                    RouteGraphError.DuplicateRouteId,
                    $"Route id '{route.RouteId}' is declared more than once.");
            }
        }

        foreach (var route in routes)
        {
            if (route.ParentRouteId is null || routesById.ContainsKey(route.ParentRouteId))
            {
                continue;
            }

            throw new RouteGraphException(
                RouteGraphError.MissingParentRoute,
                $"Route '{route.RouteId}' references missing parent route '{route.ParentRouteId}'.");
        }

        var childrenByParentId = routes
            .Where(route => route.ParentRouteId is not null)
            .GroupBy(route => route.ParentRouteId!, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.ToArray(),
                StringComparer.Ordinal);

        return new RouteGraphSnapshot(version, routes.ToArray(), routesById, childrenByParentId);
    }

    public RouteDescriptor GetRequiredRoute(string routeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeId);

        return _routesById.TryGetValue(routeId, out var route)
            ? route
            : throw new KeyNotFoundException($"Route '{routeId}' was not found.");
    }

    public bool TryGetRoute(string routeId, out RouteDescriptor? route)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeId);

        return _routesById.TryGetValue(routeId, out route);
    }

    public IReadOnlyList<RouteDescriptor> GetChildren(string parentRouteId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parentRouteId);

        return _childrenByParentId.TryGetValue(parentRouteId, out var children)
            ? children
            : [];
    }

    internal string GetFullTemplate(RouteDescriptor route)
    {
        var segments = new Stack<string>();

        for (var current = route; current is not null; current = current.ParentRouteId is null ? null : GetRequiredRoute(current.ParentRouteId))
        {
            if (current.Template is not null && current.Template.Pattern.Length > 0)
            {
                segments.Push(current.Template.Pattern);
            }
        }

        return string.Join('/', segments);
    }
}
