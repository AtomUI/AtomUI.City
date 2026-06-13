using System.Collections.ObjectModel;

namespace AtomUI.City.Testing;

public sealed class RouteTestMatch
{
    private RouteTestMatch(
        bool isMatch,
        string? routeName,
        Type? viewModelType,
        IReadOnlyDictionary<string, string> parameters,
        string? errorCode)
    {
        IsMatch = isMatch;
        RouteName = routeName;
        ViewModelType = viewModelType;
        Parameters = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase));
        ErrorCode = errorCode;
    }

    public bool IsMatch { get; }

    public string? RouteName { get; }

    public Type? ViewModelType { get; }

    public IReadOnlyDictionary<string, string> Parameters { get; }

    public string? ErrorCode { get; }

    public static RouteTestMatch Success(RouteTestDefinition route, IReadOnlyDictionary<string, string> parameters)
    {
        return new RouteTestMatch(true, route.Name, route.ViewModelType, parameters, null);
    }

    public static RouteTestMatch NotFound()
    {
        return new RouteTestMatch(false, null, null, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), "CITY-ROUTE-NOT-FOUND");
    }
}
