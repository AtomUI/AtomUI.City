namespace AtomUI.City.Routing;

public sealed class RouteMatcher
{
    private readonly RouteGraphSnapshot _snapshot;
    private readonly IReadOnlyList<RouteMatcherEntry> _entries;

    internal RouteMatcher(RouteGraphSnapshot snapshot)
    {
        _snapshot = snapshot;
        _entries = snapshot
            .Routes
            .Where(route => route.Kind is RouteDefinitionKind.Route or RouteDefinitionKind.Index or RouteDefinitionKind.Layout)
            .Select(route =>
            {
                var template = RouteTemplate.Parse(_snapshot.GetFullTemplate(route));

                return new RouteMatcherEntry(route, template);
            })
            .OrderByDescending(entry => entry.Template.SpecificityScore())
            .ThenByDescending(entry => entry.Template.Segments.Count)
            .ThenBy(entry => entry.Route.RouteId, StringComparer.Ordinal)
            .ToArray();
    }

    public RouteMatch Match(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        foreach (var match in MatchAll(path))
        {
            return match;
        }

        return RouteMatch.NotFound(path);
    }

    public IReadOnlyList<RouteMatch> MatchAll(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var matches = new List<RouteMatch>();

        foreach (var entry in _entries)
        {
            if (entry.Template.TryMatch(path, out var values))
            {
                matches.Add(RouteMatch.Success(entry.Route, values));
            }
        }

        return matches;
    }

    private sealed record RouteMatcherEntry(RouteDescriptor Route, RouteTemplate Template);
}
