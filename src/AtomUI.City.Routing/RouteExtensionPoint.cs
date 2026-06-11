namespace AtomUI.City.Routing;

public readonly record struct RouteExtensionPoint
{
    public RouteExtensionPoint(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id;
    }

    public string Id { get; }

    public override string ToString() => Id ?? string.Empty;
}
