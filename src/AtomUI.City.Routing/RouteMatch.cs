namespace AtomUI.City.Routing;

public sealed class RouteMatch
{
    private RouteMatch(
        RouteMatchStatus status,
        RouteDescriptor? route,
        IReadOnlyDictionary<string, string> parameters,
        string? unmatchedPath)
    {
        Status = status;
        MatchedRoute = route;
        Parameters = parameters;
        UnmatchedPath = unmatchedPath;
    }

    public RouteMatchStatus Status { get; }

    public RouteDescriptor Route => MatchedRoute ?? throw new InvalidOperationException("No route was matched.");

    public RouteDescriptor? MatchedRoute { get; }

    public IReadOnlyDictionary<string, string> Parameters { get; }

    public string? UnmatchedPath { get; }

    public static RouteMatch Success(
        RouteDescriptor route,
        IReadOnlyDictionary<string, string> parameters)
    {
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(parameters);

        return new RouteMatch(RouteMatchStatus.Success, route, parameters, unmatchedPath: null);
    }

    public static RouteMatch NotFound(string path)
    {
        return new RouteMatch(RouteMatchStatus.NotFound, null, new Dictionary<string, string>(), path);
    }
}
