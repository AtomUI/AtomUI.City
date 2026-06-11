namespace AtomUI.City.Data;

public sealed record DataCredential(string Scheme, string Parameter)
{
    public static DataCredential Bearer(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return new DataCredential("Bearer", token);
    }
}
