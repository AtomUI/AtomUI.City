namespace AtomUI.City.Data;

public sealed class DataClientRegistry : IDataClientFactory
{
    private readonly Dictionary<Type, IDataClient> _clients = [];
    private readonly IDataDiagnostics? _diagnostics;
    private readonly object _syncRoot = new();

    public DataClientRegistry(IDataDiagnostics? diagnostics = null)
    {
        _diagnostics = diagnostics;
    }

    public void Register<TClient>(TClient client)
        where TClient : class, IDataClient
    {
        ArgumentNullException.ThrowIfNull(client);

        lock (_syncRoot)
        {
            _clients[typeof(TClient)] = client;
        }

        _diagnostics?.Write(new DataDiagnosticRecord(
            DataDiagnosticIds.ClientRegistered,
            $"Data client '{typeof(TClient).FullName}' registered.",
            DataDiagnosticSeverity.Info,
            ClientId: client.ClientId));
    }

    public bool Unregister<TClient>()
        where TClient : class, IDataClient
    {
        IDataClient? removedClient = null;
        lock (_syncRoot)
        {
            if (_clients.Remove(typeof(TClient), out var client))
            {
                removedClient = client;
            }
        }

        if (removedClient is null)
        {
            _diagnostics?.Write(new DataDiagnosticRecord(
                DataDiagnosticIds.ClientUnregistrationMissing,
                $"Data client '{typeof(TClient).FullName}' could not be unregistered because it is not registered.",
                DataDiagnosticSeverity.Warning));

            return false;
        }

        _diagnostics?.Write(new DataDiagnosticRecord(
            DataDiagnosticIds.ClientUnregistered,
            $"Data client '{typeof(TClient).FullName}' unregistered.",
            DataDiagnosticSeverity.Info,
            ClientId: removedClient.ClientId));

        return true;
    }

    public TClient GetRequiredClient<TClient>()
        where TClient : class, IDataClient
    {
        lock (_syncRoot)
        {
            if (_clients.TryGetValue(typeof(TClient), out var client))
            {
                return (TClient)client;
            }
        }

        var clientTypeName = typeof(TClient).FullName;
        _diagnostics?.Write(new DataDiagnosticRecord(
            DataDiagnosticIds.ClientMissing,
            $"Data client '{clientTypeName}' is not registered.",
            DataDiagnosticSeverity.Warning));

        throw new KeyNotFoundException($"Data client '{clientTypeName}' is not registered.");
    }
}
