namespace AtomUI.City.Data;

public sealed class DataRequestContext
{
    private DataRequestContext(
        Guid operationId,
        string clientId,
        string operationName,
        DataTransportKind transportKind,
        DataAccessMode accessMode,
        CancellationToken cancellationToken)
    {
        OperationId = operationId;
        CorrelationId = operationId.ToString("D");
        ClientId = clientId;
        OperationName = operationName;
        TransportKind = transportKind;
        AccessMode = accessMode;
        CancellationToken = cancellationToken;
    }

    public Guid OperationId { get; }

    public string CorrelationId { get; }

    public string ClientId { get; }

    public string OperationName { get; }

    public DataTransportKind TransportKind { get; }

    public DataAccessMode AccessMode { get; }

    public int Attempt { get; internal set; }

    public CancellationToken CancellationToken { get; }

    public DataCredential? Credential { get; private set; }

    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>(StringComparer.Ordinal);

    public void SetCredential(DataCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);

        Credential = credential;
    }

    public static DataRequestContext Create<TResponse>(
        DataRequest<TResponse> request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new DataRequestContext(
            Guid.NewGuid(),
            request.ClientId,
            request.OperationName,
            request.TransportKind,
            request.AccessMode,
            cancellationToken);
    }
}
