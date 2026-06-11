namespace AtomUI.City.Data;

public readonly record struct DataConnectionOwner
{
    public DataConnectionOwner(DataConnectionOwnerKind kind, string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Kind = kind;
        Id = id;
    }

    public DataConnectionOwnerKind Kind { get; }

    public string? Id { get; }

    public static DataConnectionOwner None { get; } = new();
}
