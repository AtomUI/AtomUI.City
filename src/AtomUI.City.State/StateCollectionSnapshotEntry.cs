namespace AtomUI.City.State;

public sealed record StateCollectionSnapshotEntry<TKey, TItem>(
    TKey Key,
    TItem Item,
    long ItemVersion)
    where TKey : notnull;
