using AtomUI.City.Lifecycle;

namespace AtomUI.City.Data;

public class DataRequest<TResponse>
{
    public DataRequest(
        string clientId,
        string operationName,
        DataTransportKind transportKind,
        DataAccessMode accessMode = DataAccessMode.Query)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        ClientId = clientId;
        OperationName = operationName;
        TransportKind = transportKind;
        AccessMode = accessMode;
    }

    public string ClientId { get; }

    public string OperationName { get; }

    public DataTransportKind TransportKind { get; }

    public DataAccessMode AccessMode { get; }

    public DataAuthenticationOptions Authentication { get; init; } =
        DataAuthenticationOptions.Anonymous;

    public DataCacheOptions Cache { get; init; } = DataCacheOptions.Disabled;

    public DataResilienceOptions Resilience { get; init; } = DataResilienceOptions.None;

    public LifecycleScope? ParentScope { get; init; }

    public string? IdempotencyKey { get; init; }

    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>(StringComparer.Ordinal);
}
