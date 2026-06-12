namespace AtomUI.City.Data;

public sealed class DataConnectionManager
{
    private readonly List<IDataConnection> _connections = [];
    private readonly IDataDiagnostics? _diagnostics;
    private readonly object _syncRoot = new();

    public DataConnectionManager(IDataDiagnostics? diagnostics = null)
    {
        _diagnostics = diagnostics;
    }

    public DataResult<DataConnectionRegistration> Register(IDataConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (connection.Owner == DataConnectionOwner.None)
        {
            return DataResult<DataConnectionRegistration>.Failed(
                new DataError(
                    DataErrorKind.PolicyRejected,
                    "Long-running data connections must declare an owner."));
        }

        lock (_syncRoot)
        {
            _connections.Add(connection);
        }

        _diagnostics?.Write(new DataDiagnosticRecord(
            DataDiagnosticIds.ConnectionRegistered,
            $"Data connection '{connection.ConnectionId}' registered.",
            DataDiagnosticSeverity.Info));

        return DataResult<DataConnectionRegistration>.Success(new DataConnectionRegistration(connection));
    }

    public async ValueTask StartOwnerAsync(
        DataConnectionOwner owner,
        CancellationToken cancellationToken = default)
    {
        foreach (var connection in GetOwnerConnections(owner))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await connection.StartAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask StopOwnerAsync(
        DataConnectionOwner owner,
        CancellationToken cancellationToken = default)
    {
        foreach (var connection in GetOwnerConnections(owner))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await StopConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask StopAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var connection in GetConnections())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await StopConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
        }
    }

    private async ValueTask StopConnectionAsync(
        IDataConnection connection,
        CancellationToken cancellationToken)
    {
        await connection.StopAsync(cancellationToken).ConfigureAwait(false);

        _diagnostics?.Write(new DataDiagnosticRecord(
            DataDiagnosticIds.ConnectionStopped,
            $"Data connection '{connection.ConnectionId}' stopped.",
            DataDiagnosticSeverity.Info));
    }

    private IDataConnection[] GetOwnerConnections(DataConnectionOwner owner)
    {
        lock (_syncRoot)
        {
            return _connections
                .Where(connection => connection.Owner == owner)
                .ToArray();
        }
    }

    private IDataConnection[] GetConnections()
    {
        lock (_syncRoot)
        {
            return _connections.ToArray();
        }
    }
}

public sealed record DataConnectionRegistration(IDataConnection Connection);
