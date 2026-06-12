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
}
