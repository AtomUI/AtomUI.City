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
    public async Task ConnectionManagerWritesStartFailureDiagnosticAndPropagatesFailure()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        var owner = new DataConnectionOwner(DataConnectionOwnerKind.Plugin, "sales-plugin");
        var connection = new RecordingConnection("sales-hub", owner)
        {
            StartFailure = new InvalidOperationException("start failed"),
        };
        var manager = new DataConnectionManager(diagnostics);
        manager.Register(connection);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.StartOwnerAsync(owner).AsTask());

        Assert.Equal("start failed", exception.Message);
        var record = Assert.Single(
            diagnostics.Records,
            record => record.Code == DataDiagnosticIds.ConnectionStartFailed);
        Assert.Equal(DataDiagnosticSeverity.Error, record.Severity);
        Assert.Equal(DataErrorKind.ConnectionFailed, record.ErrorKind);
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

    [Fact]
    public async Task ConnectionManagerWritesStopFailureDiagnosticAndPropagatesFailure()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        var owner = new DataConnectionOwner(DataConnectionOwnerKind.Plugin, "sales-plugin");
        var connection = new RecordingConnection("sales-hub", owner)
        {
            StopFailure = new InvalidOperationException("stop failed"),
        };
        var manager = new DataConnectionManager(diagnostics);
        manager.Register(connection);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.StopOwnerAsync(owner).AsTask());

        Assert.Equal("stop failed", exception.Message);
        var record = Assert.Single(
            diagnostics.Records,
            record => record.Code == DataDiagnosticIds.ConnectionStopFailed);
        Assert.Equal(DataDiagnosticSeverity.Error, record.Severity);
        Assert.Equal(DataErrorKind.ConnectionFailed, record.ErrorKind);
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

        public Exception? StartFailure { get; init; }

        public Exception? StopFailure { get; init; }

        public ValueTask StartAsync(CancellationToken cancellationToken = default)
        {
            StartCount++;
            if (StartFailure is not null)
            {
                throw StartFailure;
            }

            State = DataConnectionState.Connected;

            return ValueTask.CompletedTask;
        }

        public ValueTask StopAsync(CancellationToken cancellationToken = default)
        {
            StopCount++;
            if (StopFailure is not null)
            {
                throw StopFailure;
            }

            State = DataConnectionState.Stopped;

            return ValueTask.CompletedTask;
        }
    }
}
