using AtomUI.City.Diagnostics;
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

    [Fact]
    public void SnapshotRestoreRecordsDiagnosticsForMissingState()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var key = new StateKey<string>("AtomUI.City.Tests.Missing");
        var registry = new ApplicationStateRegistry(diagnostics);
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

        var record = Assert.Single(diagnostics.Records);
        Assert.Equal(StateDiagnosticIds.SnapshotRestoreFailed, record.Code);
        Assert.Equal(HostDiagnosticSeverity.Warning, record.Severity);
        Assert.Contains(key.Name, record.Message, StringComparison.Ordinal);
        Assert.Contains("not registered", record.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SnapshotRestoreSkipsNotificationForUnchangedValueAndVersion()
    {
        var key = new StateKey<string>("AtomUI.City.Tests.Theme");
        var registry = new ApplicationStateRegistry();
        registry.Add(StateDefinition.Create(key, "light", snapshotPolicy: StateSnapshotPolicy.Persisted));
        var changeCount = 0;
        registry.OnChange(key, _ => changeCount++);
        var snapshot = new StateSnapshot(
            [
                new StateSnapshotEntry(
                    key.Name,
                    typeof(string),
                    "light",
                    version: 0,
                    schemaVersion: 1,
                    ownerModule: null,
                    pluginId: null),
            ]);

        registry.Restore(snapshot);

        Assert.Equal("light", registry.Get(key).Value);
        Assert.Equal(0, registry.Get(key).Version);
        Assert.Equal(0, changeCount);
    }

    [Fact]
    public void SnapshotRestoreRecordsDiagnosticsForSchemaMismatch()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var key = new StateKey<string>("AtomUI.City.Tests.Theme");
        var registry = new ApplicationStateRegistry(diagnostics);
        registry.Add(StateDefinition.Create(key, "light", snapshotPolicy: StateSnapshotPolicy.Persisted));
        var snapshot = new StateSnapshot(
            [
                new StateSnapshotEntry(
                    key.Name,
                    typeof(string),
                    "dark",
                    version: 3,
                    schemaVersion: 2,
                    ownerModule: null,
                    pluginId: null),
            ]);

        registry.Restore(snapshot);

        Assert.Equal("light", registry.Get(key).Value);
        var record = Assert.Single(diagnostics.Records);
        Assert.Equal(StateDiagnosticIds.SnapshotRestoreFailed, record.Code);
        Assert.Equal(HostDiagnosticSeverity.Warning, record.Severity);
        Assert.Contains(key.Name, record.Message, StringComparison.Ordinal);
        Assert.Contains("schema", record.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SnapshotRestoreRecordsDiagnosticsForPluginMismatch()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var key = new StateKey<string>("AtomUI.City.Tests.Theme");
        var registry = new ApplicationStateRegistry(diagnostics);
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
                    pluginId: "plugin.catalog"),
            ]);

        registry.Restore(snapshot);

        Assert.Equal("light", registry.Get(key).Value);
        var record = Assert.Single(diagnostics.Records);
        Assert.Equal(StateDiagnosticIds.SnapshotRestoreFailed, record.Code);
        Assert.Equal(HostDiagnosticSeverity.Warning, record.Severity);
        Assert.Contains(key.Name, record.Message, StringComparison.Ordinal);
        Assert.Contains("plugin", record.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SnapshotRestoreRecordsDiagnosticsForOwnerModuleMismatch()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var key = new StateKey<string>("AtomUI.City.Tests.Theme");
        var registry = new ApplicationStateRegistry(diagnostics);
        registry.Add(StateDefinition.Create(
            key,
            "light",
            snapshotPolicy: StateSnapshotPolicy.Persisted,
            ownerModule: "Host.Theme"));
        var snapshot = new StateSnapshot(
            [
                new StateSnapshotEntry(
                    key.Name,
                    typeof(string),
                    "dark",
                    version: 3,
                    schemaVersion: 1,
                    ownerModule: "Plugin.Theme",
                    pluginId: null),
            ]);

        registry.Restore(snapshot);

        Assert.Equal("light", registry.Get(key).Value);
        var record = Assert.Single(diagnostics.Records);
        Assert.Equal(StateDiagnosticIds.SnapshotRestoreFailed, record.Code);
        Assert.Equal(HostDiagnosticSeverity.Warning, record.Severity);
        Assert.Contains(key.Name, record.Message, StringComparison.Ordinal);
        Assert.Contains("owner module", record.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SnapshotRestoreRecordsDiagnosticsForValueTypeMismatch()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var key = new StateKey<string>("AtomUI.City.Tests.Theme");
        var registry = new ApplicationStateRegistry(diagnostics);
        registry.Add(StateDefinition.Create(key, "light", snapshotPolicy: StateSnapshotPolicy.Persisted));
        var snapshot = new StateSnapshot(
            [
                new StateSnapshotEntry(
                    key.Name,
                    typeof(int),
                    "dark",
                    version: 3,
                    schemaVersion: 1,
                    ownerModule: null,
                    pluginId: null),
            ]);

        registry.Restore(snapshot);

        Assert.Equal("light", registry.Get(key).Value);
        var record = Assert.Single(diagnostics.Records);
        Assert.Equal(StateDiagnosticIds.SnapshotRestoreFailed, record.Code);
        Assert.Equal(HostDiagnosticSeverity.Warning, record.Severity);
        Assert.Contains(key.Name, record.Message, StringComparison.Ordinal);
        Assert.Contains("value type", record.Message, StringComparison.OrdinalIgnoreCase);
    }
}
