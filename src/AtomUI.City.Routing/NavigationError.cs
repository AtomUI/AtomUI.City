namespace AtomUI.City.Routing;

public sealed record NavigationError(
    string Code,
    string Message,
    Exception? Exception = null);
