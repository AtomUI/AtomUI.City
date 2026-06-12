namespace AtomUI.City.Data;

public sealed class DataCacheOptions
{
    private const string DefaultRevision = "default";

    private DataCacheOptions(
        bool isEnabled,
        string requestFingerprint,
        string principalRevision,
        string permissionRevision,
        string? pluginContributionId,
        string clientVersion,
        string policyVersion)
    {
        IsEnabled = isEnabled;
        RequestFingerprint = requestFingerprint;
        PrincipalRevision = principalRevision;
        PermissionRevision = permissionRevision;
        PluginContributionId = pluginContributionId;
        ClientVersion = clientVersion;
        PolicyVersion = policyVersion;
    }

    public static DataCacheOptions Disabled { get; } = new(
        isEnabled: false,
        requestFingerprint: string.Empty,
        principalRevision: DefaultRevision,
        permissionRevision: DefaultRevision,
        pluginContributionId: null,
        clientVersion: DefaultRevision,
        policyVersion: DefaultRevision);

    public bool IsEnabled { get; }

    public string RequestFingerprint { get; }

    public string PrincipalRevision { get; }

    public string PermissionRevision { get; }

    public string? PluginContributionId { get; }

    public string ClientVersion { get; }

    public string PolicyVersion { get; }

    public static DataCacheOptions Enabled(
        string requestFingerprint,
        string principalRevision = "anonymous",
        string permissionRevision = DefaultRevision,
        string? pluginContributionId = null,
        string clientVersion = DefaultRevision,
        string policyVersion = DefaultRevision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestFingerprint);
        ArgumentException.ThrowIfNullOrWhiteSpace(principalRevision);
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionRevision);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyVersion);

        return new DataCacheOptions(
            isEnabled: true,
            requestFingerprint,
            principalRevision,
            permissionRevision,
            pluginContributionId,
            clientVersion,
            policyVersion);
    }
}
