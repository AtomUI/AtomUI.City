namespace AtomUI.City.State;

public interface IStateCollection<TKey, TItem>
    where TKey : notnull
{
    long Version { get; }

    IReadOnlyDictionary<TKey, TItem> Items { get; }

    bool AddOrUpdate(TKey key, TItem item);

    IStateSubscription OnChange(Action<StateCollectionChangedEventArgs<TKey, TItem>> handler);

    IStateSubscription OnChange(
        Action<StateCollectionChangedEventArgs<TKey, TItem>> handler,
        StateSubscriptionOptions options);
}
