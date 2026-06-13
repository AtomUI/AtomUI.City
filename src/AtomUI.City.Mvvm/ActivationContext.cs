using System.Collections.ObjectModel;

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
        Properties = new ReadOnlyDictionary<string, object?>(
            properties is null
                ? new Dictionary<string, object?>(StringComparer.Ordinal)
                : new Dictionary<string, object?>(properties, StringComparer.Ordinal));
    }

    public IActivationScope Scope { get; }

    public string? Source { get; }

    public IReadOnlyDictionary<string, object?> Properties { get; }
}
