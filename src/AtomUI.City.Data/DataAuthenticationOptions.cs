namespace AtomUI.City.Data;

public sealed class DataAuthenticationOptions
{
    private DataAuthenticationOptions(
        DataAuthenticationMode mode,
        string? scheme)
    {
        Mode = mode;
        Scheme = scheme;
    }

    public DataAuthenticationMode Mode { get; }

    public string? Scheme { get; }

    public static DataAuthenticationOptions Anonymous { get; } =
        new(DataAuthenticationMode.Anonymous, scheme: null);

    public static DataAuthenticationOptions Bearer(string scheme = "Bearer")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scheme);

        return new DataAuthenticationOptions(DataAuthenticationMode.Bearer, scheme);
    }
}
