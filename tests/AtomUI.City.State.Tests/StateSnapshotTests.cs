using AtomUI.City.State;

namespace AtomUI.City.State.Tests;

public sealed class StateSnapshotTests
{
    [Fact]
    public void SnapshotCapturesRegisteredStateValuesAndVersions()
    {
        var theme = new StateKey<string>("AtomUI.City.Tests.Theme");
        var counter = new StateKey<int>("AtomUI.City.Tests.Counter");
        var registry = new ApplicationStateRegistry();
        registry.Add(StateDefinition.Create(theme, "light", snapshotPolicy: StateSnapshotPolicy.Persisted));
        registry.Add(StateDefinition.Create(counter, 1, snapshotPolicy: StateSnapshotPolicy.Persisted));

        registry.Set(theme, "dark");

        var snapshot = registry.CreateSnapshot();

        Assert.Collection(
            snapshot.Entries.OrderBy(entry => entry.StateName),
            entry =>
            {
                Assert.Equal(counter.Name, entry.StateName);
                Assert.Equal(1, entry.Value);
                Assert.Equal(0, entry.Version);
            },
            entry =>
            {
                Assert.Equal(theme.Name, entry.StateName);
                Assert.Equal("dark", entry.Value);
                Assert.Equal(1, entry.Version);
            });
    }

    [Fact]
    public void SnapshotRestoreAppliesCompatibleRegisteredValues()
    {
        var key = new StateKey<string>("AtomUI.City.Tests.Theme");
        var registry = new ApplicationStateRegistry();
        registry.Add(StateDefinition.Create(key, "light", snapshotPolicy: StateSnapshotPolicy.Persisted));
        var snapshot = new StateSnapshot(
            [
                new StateSnapshotEntry(
                    key.Name,
                    typeof(string),
                    "dark",
                    version: 3,
                    schemaVersion: 1,
                    ownerModule: null,
                    pluginId: null),
            ]);

        registry.Restore(snapshot);

        Assert.Equal("dark", registry.Get(key).Value);
    }
}
