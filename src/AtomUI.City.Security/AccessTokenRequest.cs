namespace AtomUI.City.Security;

public sealed class AccessTokenRequest
{
    public AccessTokenRequest(
        string resourceName,
        string? scheme = null,
        string? operationName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);

        ResourceName = resourceName;
        Scheme = scheme;
        OperationName = operationName;
    }

    public string ResourceName { get; }

    public string? Scheme { get; }

    public string? OperationName { get; }
}
