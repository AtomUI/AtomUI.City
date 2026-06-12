using System.Collections.ObjectModel;
using AtomUI.City.Diagnostics;

namespace AtomUI.City.State;

public sealed class StateCollection<TKey, TItem> : IStateCollection<TKey, TItem>
    where TKey : notnull
{
    private readonly IHostDiagnostics? _diagnostics;
    private readonly Dictionary<TKey, CollectionItem> _items;
    private readonly IEqualityComparer<TItem> _itemComparer;
    private readonly List<StateSubscription> _subscriptions = [];
    private readonly object _syncRoot = new();

    public StateCollection(
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TItem>? itemComparer = null,
        IHostDiagnostics? diagnostics = null)
    {
        _items = new Dictionary<TKey, CollectionItem>(keyComparer);
        _itemComparer = itemComparer ?? EqualityComparer<TItem>.Default;
        _diagnostics = diagnostics;
    }

    public event EventHandler<StateCollectionChangedEventArgs<TKey, TItem>>? Changed;

    public long Version { get; private set; }

    public IReadOnlyDictionary<TKey, TItem> Items
    {
        get
        {
            lock (_syncRoot)
            {
                var snapshot = new Dictionary<TKey, TItem>(_items.Count, _items.Comparer);

                foreach (var item in _items)
                {
                    snapshot.Add(item.Key, item.Value.Value);
                }

                return new ReadOnlyDictionary<TKey, TItem>(snapshot);
            }
        }
    }

    public bool TryGetItemVersion(TKey key, out long version)
    {
        ArgumentNullException.ThrowIfNull(key);

        lock (_syncRoot)
        {
            if (_items.TryGetValue(key, out var item))
            {
                version = item.Version;
                return true;
            }

            version = 0;
            return false;
        }
    }

    public StateCollectionSnapshot<TKey, TItem> CreateSnapshot()
    {
        lock (_syncRoot)
        {
            var items = _items
                .Select(item => new StateCollectionSnapshotEntry<TKey, TItem>(
                    item.Key,
                    item.Value.Value,
                    item.Value.Version))
                .ToArray();

            return new StateCollectionSnapshot<TKey, TItem>(Version, items);
        }
    }

    public bool RestoreSnapshot(StateCollectionSnapshot<TKey, TItem> snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        StateCollectionChangedEventArgs<TKey, TItem> args;
        StateSubscription[] subscriptions;

        lock (_syncRoot)
        {
            var nextItems = new Dictionary<TKey, CollectionItem>(_items.Comparer);
            var snapshotKeys = new HashSet<TKey>(_items.Comparer);
            var changes = new List<StateCollectionChange<TKey, TItem>>();

            foreach (var item in snapshot.Items)
            {
                ArgumentNullException.ThrowIfNull(item.Key);

                snapshotKeys.Add(item.Key);
                var hasOldItem = _items.TryGetValue(item.Key, out var oldItem);
                nextItems[item.Key] = new CollectionItem(item.Item, item.ItemVersion);

                if (hasOldItem &&
                    _itemComparer.Equals(oldItem!.Value, item.Item) &&
                    oldItem.Version == item.ItemVersion)
                {
                    continue;
                }

                changes.Add(new StateCollectionChange<TKey, TItem>(
                    StateCollectionChangeKind.Reset,
                    item.Key,
                    hasOldItem,
                    hasOldItem ? oldItem!.Value : default,
                    HasNewItem: true,
                    item.Item,
                    snapshot.CollectionVersion,
                    item.ItemVersion));
            }

            foreach (var item in _items)
            {
                if (snapshotKeys.Contains(item.Key))
                {
                    continue;
                }

                changes.Add(new StateCollectionChange<TKey, TItem>(
                    StateCollectionChangeKind.Reset,
                    item.Key,
                    HasOldItem: true,
                    item.Value.Value,
                    HasNewItem: false,
                    NewItem: default,
                    snapshot.CollectionVersion,
                    item.Value.Version + 1));
            }

            if (changes.Count == 0 && Version == snapshot.CollectionVersion)
            {
                return false;
            }

            _items.Clear();

            foreach (var item in nextItems)
            {
                _items.Add(item.Key, item.Value);
            }

            Version = snapshot.CollectionVersion;

            if (changes.Count == 0)
            {
                return false;
            }

            args = new StateCollectionChangedEventArgs<TKey, TItem>(changes);
            subscriptions = _subscriptions.ToArray();
        }

        Notify(args, subscriptions);

        return true;
    }

    public bool AddOrUpdate(TKey key, TItem item)
    {
        ArgumentNullException.ThrowIfNull(key);

        StateCollectionChangedEventArgs<TKey, TItem> args;
        StateSubscription[] subscriptions;

        lock (_syncRoot)
        {
            StateCollectionChange<TKey, TItem> change;

            if (_items.TryGetValue(key, out var currentItem))
            {
                if (_itemComparer.Equals(currentItem.Value, item))
                {
                    return false;
                }

                var itemVersion = currentItem.Version + 1;
                Version++;
                _items[key] = new CollectionItem(item, itemVersion);
                change = new StateCollectionChange<TKey, TItem>(
                    StateCollectionChangeKind.Updated,
                    key,
                    HasOldItem: true,
                    currentItem.Value,
                    HasNewItem: true,
                    item,
                    Version,
                    itemVersion);
            }
            else
            {
                Version++;
                _items.Add(key, new CollectionItem(item, Version: 1));
                change = new StateCollectionChange<TKey, TItem>(
                    StateCollectionChangeKind.Added,
                    key,
                    HasOldItem: false,
                    OldItem: default,
                    HasNewItem: true,
                    item,
                    Version,
                    ItemVersion: 1);
            }

            args = new StateCollectionChangedEventArgs<TKey, TItem>(change);
            subscriptions = _subscriptions.ToArray();
        }

        Notify(args, subscriptions);

        return true;
    }

    public bool AddOrUpdateRange(IEnumerable<KeyValuePair<TKey, TItem>> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        StateCollectionChangedEventArgs<TKey, TItem> args;
        StateSubscription[] subscriptions;

        lock (_syncRoot)
        {
            var nextItems = new Dictionary<TKey, CollectionItem>(_items, _items.Comparer);
            var nextVersion = Version + 1;
            var changes = new List<StateCollectionChange<TKey, TItem>>();

            foreach (var item in items)
            {
                ArgumentNullException.ThrowIfNull(item.Key);

                if (nextItems.TryGetValue(item.Key, out var currentItem))
                {
                    if (_itemComparer.Equals(currentItem.Value, item.Value))
                    {
                        continue;
                    }

                    var itemVersion = currentItem.Version + 1;
                    nextItems[item.Key] = new CollectionItem(item.Value, itemVersion);
                    changes.Add(new StateCollectionChange<TKey, TItem>(
                        StateCollectionChangeKind.Updated,
                        item.Key,
                        HasOldItem: true,
                        currentItem.Value,
                        HasNewItem: true,
                        item.Value,
                        nextVersion,
                        itemVersion));
                }
                else
                {
                    nextItems.Add(item.Key, new CollectionItem(item.Value, Version: 1));
                    changes.Add(new StateCollectionChange<TKey, TItem>(
                        StateCollectionChangeKind.Added,
                        item.Key,
                        HasOldItem: false,
                        OldItem: default,
                        HasNewItem: true,
                        item.Value,
                        nextVersion,
                        ItemVersion: 1));
                }
            }

            if (changes.Count == 0)
            {
                return false;
            }

            Version = nextVersion;
            _items.Clear();

            foreach (var item in nextItems)
            {
                _items.Add(item.Key, item.Value);
            }

            args = new StateCollectionChangedEventArgs<TKey, TItem>(changes);
            subscriptions = _subscriptions.ToArray();
        }

        Notify(args, subscriptions);

        return true;
    }

    public bool Remove(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        StateCollectionChangedEventArgs<TKey, TItem> args;
        StateSubscription[] subscriptions;

        lock (_syncRoot)
        {
            if (!_items.TryGetValue(key, out var currentItem))
            {
                return false;
            }

            var itemVersion = currentItem.Version + 1;
            Version++;
            _items.Remove(key);
            var change = new StateCollectionChange<TKey, TItem>(
                StateCollectionChangeKind.Removed,
                key,
                HasOldItem: true,
                currentItem.Value,
                HasNewItem: false,
                NewItem: default,
                Version,
                itemVersion);

            args = new StateCollectionChangedEventArgs<TKey, TItem>(change);
            subscriptions = _subscriptions.ToArray();
        }

        Notify(args, subscriptions);

        return true;
    }

    public bool Clear()
    {
        StateCollectionChangedEventArgs<TKey, TItem> args;
        StateSubscription[] subscriptions;

        lock (_syncRoot)
        {
            if (_items.Count == 0)
            {
                return false;
            }

            Version++;
            var changes = _items
                .Select(item => new StateCollectionChange<TKey, TItem>(
                    StateCollectionChangeKind.Cleared,
                    item.Key,
                    HasOldItem: true,
                    item.Value.Value,
                    HasNewItem: false,
                    NewItem: default,
                    Version,
                    item.Value.Version + 1))
                .ToArray();
            _items.Clear();
            args = new StateCollectionChangedEventArgs<TKey, TItem>(changes);
            subscriptions = _subscriptions.ToArray();
        }

        Notify(args, subscriptions);

        return true;
    }

    public IStateSubscription OnChange(Action<StateCollectionChangedEventArgs<TKey, TItem>> handler)
    {
        return OnChange(handler, StateSubscriptionOptions.Immediate);
    }

    public IStateSubscription OnChange(
        Action<StateCollectionChangedEventArgs<TKey, TItem>> handler,
        StateSubscriptionOptions options)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(options);

        var subscription = new StateSubscription(
            args => handler((StateCollectionChangedEventArgs<TKey, TItem>)args),
            options,
            _diagnostics);

        lock (_syncRoot)
        {
            _subscriptions.Add(subscription);
        }

        return new RemovingStateSubscription(this, subscription);
    }

    private void Notify(
        StateCollectionChangedEventArgs<TKey, TItem> args,
        StateSubscription[] subscriptions)
    {
        NotifyChangedEvent(args);

        foreach (var subscription in subscriptions)
        {
            subscription.Notify(args);
        }
    }

    private void NotifyChangedEvent(StateCollectionChangedEventArgs<TKey, TItem> args)
    {
        var changed = Changed;

        if (changed is null)
        {
            return;
        }

        foreach (var handler in changed.GetInvocationList().Cast<EventHandler<StateCollectionChangedEventArgs<TKey, TItem>>>())
        {
            try
            {
                handler(this, args);
            }
            catch (Exception exception)
            {
                _diagnostics?.Write(new HostDiagnosticRecord(
                    StateDiagnosticIds.ChangedEventHandlerFailed,
                    $"State collection changed event handler failed for key type '{typeof(TKey).FullName}' and item type '{typeof(TItem).FullName}' at version {args.Version}: {exception.Message}",
                    HostDiagnosticSeverity.Error));
            }
        }
    }

    private void Remove(StateSubscription subscription)
    {
        lock (_syncRoot)
        {
            _subscriptions.Remove(subscription);
        }
    }

    private sealed record CollectionItem(TItem Value, long Version);

    private sealed class RemovingStateSubscription : IStateSubscription
    {
        private readonly StateCollection<TKey, TItem> _collection;
        private readonly StateSubscription _subscription;
        private bool _disposed;

        public RemovingStateSubscription(
            StateCollection<TKey, TItem> collection,
            StateSubscription subscription)
        {
            _collection = collection;
            _subscription = subscription;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _subscription.Dispose();
            _collection.Remove(_subscription);
        }
    }
}
