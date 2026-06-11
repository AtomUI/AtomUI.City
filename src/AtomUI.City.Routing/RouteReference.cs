namespace AtomUI.City.Routing;

public readonly record struct RouteReference
{
    public RouteReference(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id;
    }

    public string Id { get; }

    public override string ToString() => Id ?? string.Empty;
}
