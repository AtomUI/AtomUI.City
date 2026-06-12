namespace AtomUI.City.State;

public sealed class StateCollectionSnapshot<TKey, TItem>
    where TKey : notnull
{
    public StateCollectionSnapshot(
        long collectionVersion,
        IReadOnlyList<StateCollectionSnapshotEntry<TKey, TItem>> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        CollectionVersion = collectionVersion;
        Items = items;
    }

    public long CollectionVersion { get; }

    public int ItemCount => Items.Count;

    public IReadOnlyList<StateCollectionSnapshotEntry<TKey, TItem>> Items { get; }
}
