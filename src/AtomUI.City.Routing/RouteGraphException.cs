namespace AtomUI.City.Routing;

public sealed class RouteGraphException : InvalidOperationException
{
    public RouteGraphException(RouteGraphError error, string message)
        : base(message)
    {
        Error = error;
    }

    public RouteGraphError Error { get; }
}
