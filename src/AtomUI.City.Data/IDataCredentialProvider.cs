namespace AtomUI.City.Data;

public interface IDataCredentialProvider
{
    ValueTask<DataCredentialResult> GetCredentialAsync(
        DataAuthenticationContext context,
        CancellationToken cancellationToken = default);
}
