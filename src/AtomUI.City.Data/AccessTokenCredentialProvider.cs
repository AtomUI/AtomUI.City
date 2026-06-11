using AtomUI.City.Security;

namespace AtomUI.City.Data;

public sealed class AccessTokenCredentialProvider : IDataCredentialProvider
{
    private readonly IAccessTokenProvider _accessTokenProvider;

    public AccessTokenCredentialProvider(IAccessTokenProvider accessTokenProvider)
    {
        ArgumentNullException.ThrowIfNull(accessTokenProvider);

        _accessTokenProvider = accessTokenProvider;
    }

    public async ValueTask<DataCredentialResult> GetCredentialAsync(
        DataAuthenticationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Authentication.Mode == DataAuthenticationMode.Anonymous)
        {
            return DataCredentialResult.None();
        }

        var token = await _accessTokenProvider
            .GetTokenAsync(
                new AccessTokenRequest(
                    context.ClientId,
                    context.Authentication.Scheme,
                    context.OperationName),
                cancellationToken)
            .ConfigureAwait(false);

        return token.Status switch
        {
            AccessTokenResultStatus.None => DataCredentialResult.None(),
            AccessTokenResultStatus.Success => DataCredentialResult.Success(
                new DataCredential(token.Scheme!, token.Token!)),
            AccessTokenResultStatus.Required => DataCredentialResult.Required(token.Message),
            AccessTokenResultStatus.Expired => DataCredentialResult.Expired(token.Message),
            AccessTokenResultStatus.Unavailable => DataCredentialResult.Unavailable(token.Message),
            AccessTokenResultStatus.Cancelled => DataCredentialResult.Cancelled(token.Message),
            _ => DataCredentialResult.Unavailable(token.Message),
        };
    }
}
