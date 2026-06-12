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
}
