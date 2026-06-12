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
    }

    public bool Unregister<TClient>()
        where TClient : class, IDataClient
    {
        lock (_syncRoot)
        {
            return _clients.Remove(typeof(TClient));
        }
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
