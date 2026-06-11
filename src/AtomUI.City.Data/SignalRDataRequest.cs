namespace AtomUI.City.Data;

public sealed class SignalRDataRequest<TResponse> : DataRequest<TResponse>
{
    public SignalRDataRequest(
        string clientId,
        string operationName,
        string hubName,
        string methodName,
        Func<SignalRInvocationContext, CancellationToken, ValueTask<TResponse>> invoker)
        : base(clientId, operationName, DataTransportKind.SignalR)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hubName);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        HubName = hubName;
        MethodName = methodName;
        Invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
    }

    public string HubName { get; }

    public string MethodName { get; }

    public Func<SignalRInvocationContext, CancellationToken, ValueTask<TResponse>> Invoker { get; }
}

public sealed class SignalRInvocationContext
{
    public SignalRInvocationContext(
        string hubName,
        string methodName,
        DataRequestContext request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hubName);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        HubName = hubName;
        MethodName = methodName;
        Request = request ?? throw new ArgumentNullException(nameof(request));
    }

    public string HubName { get; }

    public string MethodName { get; }

    public DataRequestContext Request { get; }

    public DataCredential? Credential => Request.Credential;
}
