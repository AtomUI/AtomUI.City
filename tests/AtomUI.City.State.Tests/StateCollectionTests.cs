using AtomUI.City.State;

namespace AtomUI.City.State.Tests;

public sealed class StateCollectionTests
{
    [Fact]
    public void AddOrUpdateAddsAndUpdatesItemsWithStableChangeRecords()
    {
        var collection = new StateCollection<string, int>();
        var changes = new List<StateCollectionChangedEventArgs<string, int>>();

        collection.OnChange(changes.Add);

        var added = collection.AddOrUpdate("settings", 1);
        var updated = collection.AddOrUpdate("settings", 2);

        Assert.True(added);
        Assert.True(updated);
        Assert.Equal(2, collection.Version);
        Assert.Equal(2, collection.Items["settings"]);
        Assert.Collection(
            changes,
            args =>
            {
                Assert.Equal(StateCollectionChangeKind.Added, args.Change.Kind);
                Assert.Equal("settings", args.Change.Key);
                Assert.False(args.Change.HasOldItem);
                Assert.Equal(1, args.Change.NewItem);
                Assert.True(args.Change.HasNewItem);
                Assert.Equal(1, args.Change.CollectionVersion);
                Assert.Equal(1, args.Change.ItemVersion);
            },
            args =>
            {
                Assert.Equal(StateCollectionChangeKind.Updated, args.Change.Kind);
                Assert.Equal("settings", args.Change.Key);
                Assert.True(args.Change.HasOldItem);
                Assert.Equal(1, args.Change.OldItem);
                Assert.Equal(2, args.Change.NewItem);
                Assert.True(args.Change.HasNewItem);
                Assert.Equal(2, args.Change.CollectionVersion);
                Assert.Equal(2, args.Change.ItemVersion);
            });
    }

    [Fact]
    public void AddOrUpdateSkipsUnchangedItem()
    {
        var collection = new StateCollection<string, int>();
        collection.AddOrUpdate("settings", 1);
        var notifications = new List<StateCollectionChangedEventArgs<string, int>>();

        collection.OnChange(notifications.Add);

        var changed = collection.AddOrUpdate("settings", 1);

        Assert.False(changed);
        Assert.Equal(1, collection.Version);
        Assert.Equal(1, collection.Items["settings"]);
        Assert.Empty(notifications);
    }

    [Fact]
    public void RemoveDeletesExistingItemAndRaisesChangeRecord()
    {
        var collection = new StateCollection<string, int>();
        collection.AddOrUpdate("settings", 1);
        var changes = new List<StateCollectionChangedEventArgs<string, int>>();

        collection.OnChange(changes.Add);

        var removed = collection.Remove("settings");

        Assert.True(removed);
        Assert.Equal(2, collection.Version);
        Assert.False(collection.Items.ContainsKey("settings"));
        var args = Assert.Single(changes);
        Assert.Equal(StateCollectionChangeKind.Removed, args.Change.Kind);
        Assert.Equal("settings", args.Change.Key);
        Assert.True(args.Change.HasOldItem);
        Assert.Equal(1, args.Change.OldItem);
        Assert.False(args.Change.HasNewItem);
        Assert.Equal(2, args.Change.CollectionVersion);
        Assert.Equal(2, args.Change.ItemVersion);
    }

    [Fact]
    public void RemoveSkipsMissingItem()
    {
        var collection = new StateCollection<string, int>();
        var notifications = new List<StateCollectionChangedEventArgs<string, int>>();

        collection.OnChange(notifications.Add);

        var removed = collection.Remove("settings");

        Assert.False(removed);
        Assert.Equal(0, collection.Version);
        Assert.Empty(collection.Items);
        Assert.Empty(notifications);
    }

    [Fact]
    public void ClearDeletesItemsAndRaisesClearedChangeRecord()
    {
        var collection = new StateCollection<string, int>();
        collection.AddOrUpdate("settings", 1);
        var changes = new List<StateCollectionChangedEventArgs<string, int>>();

        collection.OnChange(changes.Add);

        var cleared = collection.Clear();

        Assert.True(cleared);
        Assert.Equal(2, collection.Version);
        Assert.Empty(collection.Items);
        var args = Assert.Single(changes);
        Assert.Equal(StateCollectionChangeKind.Cleared, args.Change.Kind);
        Assert.Equal("settings", args.Change.Key);
        Assert.True(args.Change.HasOldItem);
        Assert.Equal(1, args.Change.OldItem);
        Assert.False(args.Change.HasNewItem);
        Assert.Equal(2, args.Change.CollectionVersion);
        Assert.Equal(2, args.Change.ItemVersion);
    }

    [Fact]
    public void ClearSkipsEmptyCollection()
    {
        var collection = new StateCollection<string, int>();
        var notifications = new List<StateCollectionChangedEventArgs<string, int>>();

        collection.OnChange(notifications.Add);

        var cleared = collection.Clear();

        Assert.False(cleared);
        Assert.Equal(0, collection.Version);
        Assert.Empty(collection.Items);
        Assert.Empty(notifications);
    }

    [Fact]
    public void AddOrUpdateRangeMergesChangesIntoSingleNotificationInInputOrder()
    {
        var collection = new StateCollection<string, int>();
        collection.AddOrUpdate("settings", 1);
        var notifications = new List<StateCollectionChangedEventArgs<string, int>>();

        collection.OnChange(notifications.Add);

        var changed = collection.AddOrUpdateRange(
            [
                new KeyValuePair<string, int>("settings", 2),
                new KeyValuePair<string, int>("layout", 3),
            ]);

        Assert.True(changed);
        Assert.Equal(2, collection.Version);
        Assert.Equal(2, collection.Items["settings"]);
        Assert.Equal(3, collection.Items["layout"]);
        var args = Assert.Single(notifications);
        Assert.Equal(2, args.Changes.Count);
        Assert.Collection(
            args.Changes,
            change =>
            {
                Assert.Equal(StateCollectionChangeKind.Updated, change.Kind);
                Assert.Equal("settings", change.Key);
                Assert.True(change.HasOldItem);
                Assert.Equal(1, change.OldItem);
                Assert.True(change.HasNewItem);
                Assert.Equal(2, change.NewItem);
                Assert.Equal(2, change.CollectionVersion);
                Assert.Equal(2, change.ItemVersion);
            },
            change =>
            {
                Assert.Equal(StateCollectionChangeKind.Added, change.Kind);
                Assert.Equal("layout", change.Key);
                Assert.False(change.HasOldItem);
                Assert.True(change.HasNewItem);
                Assert.Equal(3, change.NewItem);
                Assert.Equal(2, change.CollectionVersion);
                Assert.Equal(1, change.ItemVersion);
            });
    }

    [Fact]
    public void AddOrUpdateRangeSkipsUnchangedItems()
    {
        var collection = new StateCollection<string, int>();
        collection.AddOrUpdate("settings", 1);
        var notifications = new List<StateCollectionChangedEventArgs<string, int>>();

        collection.OnChange(notifications.Add);

        var changed = collection.AddOrUpdateRange(
            [
                new KeyValuePair<string, int>("settings", 1),
            ]);

        Assert.False(changed);
        Assert.Equal(1, collection.Version);
        Assert.Equal(1, collection.Items["settings"]);
        Assert.Empty(notifications);
    }

    [Fact]
    public void AddOrUpdateRangeKeepsOldItemsWhenEnumerationFails()
    {
        var collection = new StateCollection<string, int>();
        collection.AddOrUpdate("settings", 1);
        var notifications = new List<StateCollectionChangedEventArgs<string, int>>();

        collection.OnChange(notifications.Add);

        var exception = Assert.Throws<InvalidOperationException>(
            () => collection.AddOrUpdateRange(FailingBatch()));

        Assert.Equal("batch failed", exception.Message);
        Assert.Equal(1, collection.Version);
        Assert.Equal(1, collection.Items["settings"]);
        Assert.False(collection.Items.ContainsKey("layout"));
        Assert.Empty(notifications);
    }

    private static IEnumerable<KeyValuePair<string, int>> FailingBatch()
    {
        yield return new KeyValuePair<string, int>("layout", 2);
        throw new InvalidOperationException("batch failed");
    }

    [Fact]
    public void TryGetItemVersionReturnsCurrentItemVersion()
    {
        var collection = new StateCollection<string, int>();

        var missing = collection.TryGetItemVersion("settings", out var missingVersion);
        collection.AddOrUpdate("settings", 1);
        var added = collection.TryGetItemVersion("settings", out var addedVersion);
        collection.AddOrUpdate("settings", 2);
        var updated = collection.TryGetItemVersion("settings", out var updatedVersion);

        Assert.False(missing);
        Assert.Equal(0, missingVersion);
        Assert.True(added);
        Assert.Equal(1, addedVersion);
        Assert.True(updated);
        Assert.Equal(2, updatedVersion);
    }

    [Fact]
    public void CreateSnapshotCapturesCollectionVersionItemsAndItemVersions()
    {
        var collection = new StateCollection<string, int>();
        collection.AddOrUpdate("settings", 1);
        collection.AddOrUpdate("layout", 2);
        collection.AddOrUpdate("settings", 3);

        var snapshot = collection.CreateSnapshot();

        Assert.Equal(3, snapshot.CollectionVersion);
        Assert.Equal(2, snapshot.ItemCount);
        Assert.Collection(
            snapshot.Items,
            item =>
            {
                Assert.Equal("settings", item.Key);
                Assert.Equal(3, item.Item);
                Assert.Equal(2, item.ItemVersion);
            },
            item =>
            {
                Assert.Equal("layout", item.Key);
                Assert.Equal(2, item.Item);
                Assert.Equal(1, item.ItemVersion);
            });
    }

    [Fact]
    public void RestoreSnapshotRebuildsItemsVersionsAndRaisesResetNotification()
    {
        var collection = new StateCollection<string, int>();
        var notifications = new List<StateCollectionChangedEventArgs<string, int>>();
        var snapshot = new StateCollectionSnapshot<string, int>(
            collectionVersion: 5,
            [
                new StateCollectionSnapshotEntry<string, int>("settings", 3, ItemVersion: 2),
                new StateCollectionSnapshotEntry<string, int>("layout", 2, ItemVersion: 1),
            ]);

        collection.OnChange(notifications.Add);

        var restored = collection.RestoreSnapshot(snapshot);

        Assert.True(restored);
        Assert.Equal(5, collection.Version);
        Assert.Equal(3, collection.Items["settings"]);
        Assert.Equal(2, collection.Items["layout"]);
        Assert.True(collection.TryGetItemVersion("settings", out var settingsVersion));
        Assert.Equal(2, settingsVersion);
        Assert.True(collection.TryGetItemVersion("layout", out var layoutVersion));
        Assert.Equal(1, layoutVersion);
        var args = Assert.Single(notifications);
        Assert.Equal(2, args.Changes.Count);
        Assert.Collection(
            args.Changes,
            change =>
            {
                Assert.Equal(StateCollectionChangeKind.Reset, change.Kind);
                Assert.Equal("settings", change.Key);
                Assert.False(change.HasOldItem);
                Assert.True(change.HasNewItem);
                Assert.Equal(3, change.NewItem);
                Assert.Equal(5, change.CollectionVersion);
                Assert.Equal(2, change.ItemVersion);
            },
            change =>
            {
                Assert.Equal(StateCollectionChangeKind.Reset, change.Kind);
                Assert.Equal("layout", change.Key);
                Assert.False(change.HasOldItem);
                Assert.True(change.HasNewItem);
                Assert.Equal(2, change.NewItem);
                Assert.Equal(5, change.CollectionVersion);
                Assert.Equal(1, change.ItemVersion);
            });
    }

    [Fact]
    public void RestoreSnapshotClearsItemsMissingFromSnapshotAndRaisesResetNotification()
    {
        var collection = new StateCollection<string, int>();
        collection.AddOrUpdate("settings", 1);
        collection.AddOrUpdate("layout", 2);
        var notifications = new List<StateCollectionChangedEventArgs<string, int>>();
        var snapshot = new StateCollectionSnapshot<string, int>(
            collectionVersion: 5,
            []);

        collection.OnChange(notifications.Add);

        var restored = collection.RestoreSnapshot(snapshot);

        Assert.True(restored);
        Assert.Equal(5, collection.Version);
        Assert.Empty(collection.Items);
        var args = Assert.Single(notifications);
        Assert.Equal(2, args.Changes.Count);
        Assert.Collection(
            args.Changes,
            change =>
            {
                Assert.Equal(StateCollectionChangeKind.Reset, change.Kind);
                Assert.Equal("settings", change.Key);
                Assert.True(change.HasOldItem);
                Assert.Equal(1, change.OldItem);
                Assert.False(change.HasNewItem);
                Assert.Equal(5, change.CollectionVersion);
                Assert.Equal(2, change.ItemVersion);
            },
            change =>
            {
                Assert.Equal(StateCollectionChangeKind.Reset, change.Kind);
                Assert.Equal("layout", change.Key);
                Assert.True(change.HasOldItem);
                Assert.Equal(2, change.OldItem);
                Assert.False(change.HasNewItem);
                Assert.Equal(5, change.CollectionVersion);
                Assert.Equal(2, change.ItemVersion);
            });
    }

    [Fact]
    public void RestoreSnapshotSkipsNotificationForUnchangedSnapshot()
    {
        var collection = new StateCollection<string, int>();
        collection.AddOrUpdate("settings", 1);
        var snapshot = collection.CreateSnapshot();
        var notifications = new List<StateCollectionChangedEventArgs<string, int>>();

        collection.OnChange(notifications.Add);

        var restored = collection.RestoreSnapshot(snapshot);

        Assert.False(restored);
        Assert.Equal(1, collection.Version);
        Assert.Equal(1, collection.Items["settings"]);
        Assert.Empty(notifications);
    }

    [Fact]
    public void SnapshotCopiesItemsFromConstructorInput()
    {
        var items = new List<StateCollectionSnapshotEntry<string, int>>
        {
            new("settings", 1, ItemVersion: 1),
        };
        var snapshot = new StateCollectionSnapshot<string, int>(collectionVersion: 1, items);

        items.Add(new StateCollectionSnapshotEntry<string, int>("layout", 2, ItemVersion: 1));

        Assert.Equal(1, snapshot.ItemCount);
        var item = Assert.Single(snapshot.Items);
        Assert.Equal("settings", item.Key);
    }

    [Fact]
    public void SnapshotItemsRejectExternalListMutation()
    {
        var item = new StateCollectionSnapshotEntry<string, int>("settings", 1, ItemVersion: 1);
        var replacement = new StateCollectionSnapshotEntry<string, int>("layout", 2, ItemVersion: 1);
        var snapshot = new StateCollectionSnapshot<string, int>(collectionVersion: 1, [item]);
        var list = Assert.IsAssignableFrom<IList<StateCollectionSnapshotEntry<string, int>>>(snapshot.Items);

        Assert.Throws<NotSupportedException>(() => list[0] = replacement);
        Assert.Equal(item.Key, snapshot.Items[0].Key);
    }

    [Fact]
    public void ChangedEventArgsCopiesChangesFromConstructorInput()
    {
        var changes = new List<StateCollectionChange<string, int>>
        {
            new(
                StateCollectionChangeKind.Added,
                "settings",
                HasOldItem: false,
                OldItem: default,
                HasNewItem: true,
                NewItem: 1,
                CollectionVersion: 1,
                ItemVersion: 1),
        };
        var args = new StateCollectionChangedEventArgs<string, int>(changes);

        changes.Clear();

        var change = Assert.Single(args.Changes);
        Assert.Equal("settings", change.Key);
        Assert.Equal(1, args.Version);
    }

    [Fact]
    public void ChangedEventArgsRejectExternalListMutation()
    {
        var change = new StateCollectionChange<string, int>(
            StateCollectionChangeKind.Added,
            "settings",
            HasOldItem: false,
            OldItem: default,
            HasNewItem: true,
            NewItem: 1,
            CollectionVersion: 1,
            ItemVersion: 1);
        var replacement = new StateCollectionChange<string, int>(
            StateCollectionChangeKind.Added,
            "layout",
            HasOldItem: false,
            OldItem: default,
            HasNewItem: true,
            NewItem: 2,
            CollectionVersion: 2,
            ItemVersion: 1);
        var args = new StateCollectionChangedEventArgs<string, int>(change);
        var list = Assert.IsAssignableFrom<IList<StateCollectionChange<string, int>>>(args.Changes);

        Assert.Throws<NotSupportedException>(() => list[0] = replacement);
        Assert.Equal(change.Key, args.Changes[0].Key);
    }
}
