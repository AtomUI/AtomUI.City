using System.Collections.ObjectModel;

namespace AtomUI.City.Routing;

internal static class RouteParameters
{
    public static IReadOnlyDictionary<string, string> Empty()
    {
        return new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
    }

    public static IReadOnlyDictionary<string, string> Copy(IReadOnlyDictionary<string, string> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        return new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase));
    }
}
