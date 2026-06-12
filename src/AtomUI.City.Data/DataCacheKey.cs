namespace AtomUI.City.Data;

public sealed record DataCacheKey(
    string ClientId,
    string OperationName,
    DataTransportKind TransportKind,
    DataAccessMode AccessMode,
    string RequestFingerprint,
    string AuthenticationScheme,
    string PrincipalRevision,
    string PermissionRevision,
    string? PluginContributionId,
    string ClientVersion,
    string PolicyVersion)
{
    public static DataCacheKey Create<TResponse>(
        DataRequest<TResponse> request,
        string authenticationScheme)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(authenticationScheme);

        return new DataCacheKey(
            request.ClientId,
            request.OperationName,
            request.TransportKind,
            request.AccessMode,
            request.Cache.RequestFingerprint,
            authenticationScheme,
            request.Cache.PrincipalRevision,
            request.Cache.PermissionRevision,
            request.Cache.PluginContributionId,
            request.Cache.ClientVersion,
            request.Cache.PolicyVersion);
    }
}
