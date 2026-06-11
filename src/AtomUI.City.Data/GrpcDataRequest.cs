namespace AtomUI.City.Data;

public sealed class GrpcDataRequest<TResponse> : DataRequest<TResponse>
{
    public GrpcDataRequest(
        string clientId,
        string operationName,
        Func<GrpcRequestContext, CancellationToken, ValueTask<GrpcCallResult<TResponse>>> invoker,
        DataAccessMode accessMode = DataAccessMode.Query)
        : base(clientId, operationName, DataTransportKind.Grpc, accessMode)
    {
        Invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
    }

    public Func<GrpcRequestContext, CancellationToken, ValueTask<GrpcCallResult<TResponse>>> Invoker { get; }
}

public sealed class GrpcRequestContext
{
    public GrpcRequestContext(DataRequestContext request)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
    }

    public DataRequestContext Request { get; }

    public DataCredential? Credential => Request.Credential;
}
