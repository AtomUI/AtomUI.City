namespace AtomUI.City.State;

public sealed class StateCollectionChangedEventArgs<TKey, TItem> : StateChangedEventArgs
    where TKey : notnull
{
    public StateCollectionChangedEventArgs(StateCollectionChange<TKey, TItem> change)
        : this([change])
    {
    }

    public StateCollectionChangedEventArgs(IReadOnlyList<StateCollectionChange<TKey, TItem>> changes)
        : base(oldValue: null, newValue: null, GetCollectionVersion(changes))
    {
        ArgumentNullException.ThrowIfNull(changes);

        if (changes.Count == 0)
        {
            throw new ArgumentException("State collection change list cannot be empty.", nameof(changes));
        }

        Changes = changes;
    }

    public StateCollectionChange<TKey, TItem> Change => Changes[0];

    public IReadOnlyList<StateCollectionChange<TKey, TItem>> Changes { get; }

    private static long GetCollectionVersion(IReadOnlyList<StateCollectionChange<TKey, TItem>> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);

        if (changes.Count == 0)
        {
            return 0;
        }

        return changes[^1].CollectionVersion;
    }
}
