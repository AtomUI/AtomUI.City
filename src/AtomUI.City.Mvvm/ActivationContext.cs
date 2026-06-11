namespace AtomUI.City.Mvvm;

public sealed class ActivationContext
{
    public ActivationContext(
        IActivationScope scope,
        string? source = null,
        IReadOnlyDictionary<string, object?>? properties = null)
    {
        ArgumentNullException.ThrowIfNull(scope);

        Scope = scope;
        Source = source;
        Properties = properties ?? new Dictionary<string, object?>();
    }

    public IActivationScope Scope { get; }

    public string? Source { get; }

    public IReadOnlyDictionary<string, object?> Properties { get; }
}
