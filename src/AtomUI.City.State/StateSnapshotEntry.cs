namespace AtomUI.City.State;

public sealed record StateSnapshotEntry
{
    public StateSnapshotEntry(
        string stateName,
        Type valueType,
        object? value,
        long version,
        int schemaVersion,
        string? ownerModule,
        string? pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stateName);
        ArgumentNullException.ThrowIfNull(valueType);

        StateName = stateName;
        ValueType = valueType;
        Value = value;
        Version = version;
        SchemaVersion = schemaVersion;
        OwnerModule = ownerModule;
        PluginId = pluginId;
    }

    public string StateName { get; init; }

    public Type ValueType { get; init; }

    public object? Value { get; init; }

    public long Version { get; init; }

    public int SchemaVersion { get; init; }

    public string? OwnerModule { get; init; }

    public string? PluginId { get; init; }

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
