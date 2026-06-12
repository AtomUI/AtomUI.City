namespace AtomUI.City.State;

public sealed record StateCollectionChange<TKey, TItem>(
    StateCollectionChangeKind Kind,
    TKey Key,
    bool HasOldItem,
    TItem? OldItem,
    bool HasNewItem,
    TItem? NewItem,
    long CollectionVersion,
    long ItemVersion)
    where TKey : notnull;
