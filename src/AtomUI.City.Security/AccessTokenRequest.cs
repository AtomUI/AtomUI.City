namespace AtomUI.City.Security;

public sealed class AccessTokenRequest
{
    public AccessTokenRequest(string resourceName, string? scheme = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);

        ResourceName = resourceName;
        Scheme = scheme;
    }

    public string ResourceName { get; }

    public string? Scheme { get; }
}
