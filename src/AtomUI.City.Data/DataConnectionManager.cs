namespace AtomUI.City.Data;

public sealed class DataConnectionManager
{
    private readonly List<IDataConnection> _connections = [];
    private readonly object _syncRoot = new();

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
            await connection.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask StopAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var connection in GetConnections())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await connection.StopAsync(cancellationToken).ConfigureAwait(false);
        }
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
