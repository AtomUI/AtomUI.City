namespace AtomUI.City.Routing;

public sealed class NavigationTarget
{
    private NavigationTarget(
        NavigationTargetKind kind,
        string? routeId,
        string? path,
        IReadOnlyDictionary<string, string> parameters,
        NavigationOptions options)
    {
        Kind = kind;
        RouteId = routeId;
        Path = path;
        Parameters = parameters;
        Options = options;
    }

    public NavigationTargetKind Kind { get; }

    public string? RouteId { get; }

    public string? Path { get; }

    public IReadOnlyDictionary<string, string> Parameters { get; }

    public NavigationOptions Options { get; }

    public static NavigationTarget FromPath(
        string path,
        NavigationOptions options)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(options);

        return new NavigationTarget(
            NavigationTargetKind.Path,
            routeId: null,
            path,
            EmptyParameters(),
            options);
    }

    public static NavigationTarget FromRouteReference(
        string routeId,
        IReadOnlyDictionary<string, string>? parameters,
        NavigationOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeId);
        ArgumentNullException.ThrowIfNull(options);

        return new NavigationTarget(
            NavigationTargetKind.RouteReference,
            routeId,
            path: null,
            parameters is null
                ? EmptyParameters()
                : new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase),
            options);
    }

    public static NavigationTarget FromJournal(NavigationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new NavigationTarget(
            NavigationTargetKind.Journal,
            routeId: null,
            path: null,
            EmptyParameters(),
            options);
    }

    public override string ToString()
    {
        return Kind switch
        {
            NavigationTargetKind.Path => Path ?? string.Empty,
            NavigationTargetKind.RouteReference => RouteId ?? string.Empty,
            NavigationTargetKind.Journal => "journal",
            _ => Kind.ToString(),
        };
    }

    private static Dictionary<string, string> EmptyParameters()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
