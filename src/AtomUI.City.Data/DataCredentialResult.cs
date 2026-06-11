namespace AtomUI.City.Data;

public sealed class DataCredentialResult
{
    private DataCredentialResult(
        DataCredentialResultStatus status,
        DataCredential? credential,
        string? message)
    {
        Status = status;
        Credential = credential;
        Message = message;
    }

    public DataCredentialResultStatus Status { get; }

    public DataCredential? Credential { get; }

    public string? Message { get; }

    public bool Succeeded => Status == DataCredentialResultStatus.Success;

    public static DataCredentialResult Success(DataCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);

        return new DataCredentialResult(DataCredentialResultStatus.Success, credential, message: null);
    }

    public static DataCredentialResult None()
    {
        return new DataCredentialResult(DataCredentialResultStatus.None, credential: null, message: null);
    }

    public static DataCredentialResult Required(string? message = null)
    {
        return new DataCredentialResult(DataCredentialResultStatus.Required, credential: null, message);
    }

    public static DataCredentialResult Expired(string? message = null)
    {
        return new DataCredentialResult(DataCredentialResultStatus.Expired, credential: null, message);
    }

    public static DataCredentialResult Unavailable(string? message = null)
    {
        return new DataCredentialResult(DataCredentialResultStatus.Unavailable, credential: null, message);
    }

    public static DataCredentialResult Cancelled(string? message = null)
    {
        return new DataCredentialResult(DataCredentialResultStatus.Cancelled, credential: null, message);
    }
}

public enum DataCredentialResultStatus
{
    None,
    Success,
    Required,
    Expired,
    Unavailable,
    Cancelled,
}
