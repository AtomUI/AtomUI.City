namespace AtomUI.City.Routing;

public readonly record struct RouteReference<TParameters>
{
    public RouteReference(string id)
        : this(id, parameterBinder: null)
    {
    }

    public RouteReference(
        string id,
        Func<TParameters, IReadOnlyDictionary<string, string>>? parameterBinder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id;
        ParameterBinder = parameterBinder;
    }

    public string Id { get; }

    public Func<TParameters, IReadOnlyDictionary<string, string>>? ParameterBinder { get; }

    public IReadOnlyDictionary<string, string> BindParameters(TParameters parameters)
    {
        return ParameterBinder?.Invoke(parameters) ?? new Dictionary<string, string>();
    }

    public override string ToString() => Id ?? string.Empty;
}
