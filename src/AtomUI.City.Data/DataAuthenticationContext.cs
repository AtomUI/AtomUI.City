namespace AtomUI.City.Data;

public sealed class DataAuthenticationContext
{
    public DataAuthenticationContext(
        string clientId,
        string operationName,
        DataAuthenticationOptions authentication)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(authentication);

        ClientId = clientId;
        OperationName = operationName;
        Authentication = authentication;
    }

    public string ClientId { get; }

    public string OperationName { get; }

    public DataAuthenticationOptions Authentication { get; }
}
