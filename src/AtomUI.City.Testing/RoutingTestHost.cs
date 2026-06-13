namespace AtomUI.City.Testing;

public sealed class RoutingTestHost
{
    internal RoutingTestHost(IReadOnlyList<RouteTestDefinition> routes)
    {
        Routes = Array.AsReadOnly(routes.ToArray());
    }

    public IReadOnlyList<RouteTestDefinition> Routes { get; }

    public static RoutingTestHostBuilder CreateBuilder()
    {
        return new RoutingTestHostBuilder();
    }

    public RouteTestMatch Match(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var pathSegments = SplitPath(path);

        foreach (var route in Routes)
        {
            var routeSegments = SplitPath(route.Pattern);

            if (routeSegments.Length != pathSegments.Length)
            {
                continue;
            }

            var parameters = new Dictionary<string, string>(StringComparer.Ordinal);
            var matched = true;

            for (var index = 0; index < routeSegments.Length; index++)
            {
                var routeSegment = routeSegments[index];
                var pathSegment = pathSegments[index];

                if (routeSegment.StartsWith('{') && routeSegment.EndsWith('}'))
                {
                    parameters[routeSegment[1..^1]] = pathSegment;
                }
                else if (!string.Equals(routeSegment, pathSegment, StringComparison.OrdinalIgnoreCase))
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return RouteTestMatch.Success(route, parameters);
            }
        }

        return RouteTestMatch.NotFound();
    }

    private static string[] SplitPath(string path)
    {
        return path
            .Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
