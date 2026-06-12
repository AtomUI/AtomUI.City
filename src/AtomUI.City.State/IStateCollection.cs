namespace AtomUI.City.State;

public interface IStateCollection<TKey, TItem>
    where TKey : notnull
{
    long Version { get; }

    IReadOnlyDictionary<TKey, TItem> Items { get; }

    bool TryGetItemVersion(TKey key, out long version);

    StateCollectionSnapshot<TKey, TItem> CreateSnapshot();

    bool RestoreSnapshot(StateCollectionSnapshot<TKey, TItem> snapshot);

    bool AddOrUpdate(TKey key, TItem item);

    bool AddOrUpdateRange(IEnumerable<KeyValuePair<TKey, TItem>> items);

    bool Remove(TKey key);

    bool Clear();

    IStateSubscription OnChange(Action<StateCollectionChangedEventArgs<TKey, TItem>> handler);

    IStateSubscription OnChange(
        Action<StateCollectionChangedEventArgs<TKey, TItem>> handler,
        StateSubscriptionOptions options);
}
