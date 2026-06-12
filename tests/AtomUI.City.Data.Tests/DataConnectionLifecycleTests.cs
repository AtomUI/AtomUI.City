using AtomUI.City.Data;

namespace AtomUI.City.Data.Tests;

public sealed class DataConnectionLifecycleTests
{
    [Fact]
    public async Task ConnectionManagerStartsAndStopsConnection()
    {
        var owner = new DataConnectionOwner(DataConnectionOwnerKind.Plugin, "sales-plugin");
        var connection = new RecordingConnection("sales-hub", owner);
        var manager = new DataConnectionManager();

        manager.Register(connection);
        await manager.StartOwnerAsync(owner);
        await manager.StopOwnerAsync(owner);

        Assert.Equal(DataConnectionState.Stopped, connection.State);
        Assert.Equal(1, connection.StartCount);
        Assert.Equal(1, connection.StopCount);
    }

    [Fact]
    public async Task ConnectionManagerRejectsOwnerlessLongRunningConnection()
    {
        var connection = new RecordingConnection(
            "manual-hub",
            DataConnectionOwner.None);
        var manager = new DataConnectionManager();

        var result = manager.Register(connection);

        Assert.False(result.Succeeded);
        Assert.Equal(DataErrorKind.PolicyRejected, result.Error?.Kind);
        await manager.StopAllAsync();
        Assert.Equal(0, connection.StopCount);
    }

    [Fact]
    public void ConnectionManagerWritesRegisteredDiagnostic()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        var owner = new DataConnectionOwner(DataConnectionOwnerKind.Plugin, "sales-plugin");
        var connection = new RecordingConnection("sales-hub", owner);
        var manager = new DataConnectionManager(diagnostics);

        manager.Register(connection);

        var record = Assert.Single(
            diagnostics.Records,
            record => record.Code == DataDiagnosticIds.ConnectionRegistered);
        Assert.Contains("sales-hub", record.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ConnectionManagerWritesStoppedDiagnostic()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        var owner = new DataConnectionOwner(DataConnectionOwnerKind.Plugin, "sales-plugin");
        var connection = new RecordingConnection("sales-hub", owner);
        var manager = new DataConnectionManager(diagnostics);
        manager.Register(connection);

        await manager.StopOwnerAsync(owner);

        var record = Assert.Single(
            diagnostics.Records,
            record => record.Code == DataDiagnosticIds.ConnectionStopped);
        Assert.Contains("sales-hub", record.Message, StringComparison.Ordinal);
    }

    private sealed class RecordingConnection : IDataConnection
    {
        public RecordingConnection(string connectionId, DataConnectionOwner owner)
        {
            ConnectionId = connectionId;
            Owner = owner;
        }

        public string ConnectionId { get; }

        public DataConnectionOwner Owner { get; }

        public DataConnectionState State { get; private set; } = DataConnectionState.Created;

        public int StartCount { get; private set; }

        public int StopCount { get; private set; }

        public ValueTask StartAsync(CancellationToken cancellationToken = default)
        {
            StartCount++;
            State = DataConnectionState.Connected;

            return ValueTask.CompletedTask;
        }

        public ValueTask StopAsync(CancellationToken cancellationToken = default)
        {
            StopCount++;
            State = DataConnectionState.Stopped;

            return ValueTask.CompletedTask;
        }
    }
}
