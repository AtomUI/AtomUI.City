namespace AtomUI.City.State;

public abstract class StateDefinition
{
    protected StateDefinition(
        string name,
        Type valueType,
        StateLifetime lifetime,
        StateAccessPolicy access,
        StateSnapshotPolicy snapshotPolicy,
        int schemaVersion,
        string? ownerModule,
        string? pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(valueType);

        Name = name;
        ValueType = valueType;
        Lifetime = lifetime;
        Access = access;
        SnapshotPolicy = snapshotPolicy;
        SchemaVersion = schemaVersion;
        OwnerModule = ownerModule;
        PluginId = pluginId;
    }

    public string Name { get; }

    public Type ValueType { get; }

    public StateLifetime Lifetime { get; }

    public StateAccessPolicy Access { get; }

    public StateSnapshotPolicy SnapshotPolicy { get; }

    public int SchemaVersion { get; }

    public string? OwnerModule { get; }

    public string? PluginId { get; }

    public static StateDefinition<T> Create<T>(
        StateKey<T> key,
        T defaultValue,
        StateLifetime lifetime = StateLifetime.Application,
        StateAccessPolicy access = StateAccessPolicy.HostWrite,
        StateSnapshotPolicy snapshotPolicy = StateSnapshotPolicy.Transient,
        int schemaVersion = 1,
        string? ownerModule = null,
        string? pluginId = null,
        IEqualityComparer<T>? comparer = null)
    {
        return StateDefinition<T>.Create(
            key,
            defaultValue,
            lifetime,
            access,
            snapshotPolicy,
            schemaVersion,
            ownerModule,
            pluginId,
            comparer);
    }
}

public sealed class StateDefinition<T> : StateDefinition
{
    private StateDefinition(
        StateKey<T> key,
        T defaultValue,
        StateLifetime lifetime,
        StateAccessPolicy access,
        StateSnapshotPolicy snapshotPolicy,
        int schemaVersion,
        string? ownerModule,
        string? pluginId,
        IEqualityComparer<T>? comparer)
        : base(
            key.Name,
            typeof(T),
            lifetime,
            access,
            snapshotPolicy,
            schemaVersion,
            ownerModule,
            pluginId)
    {
        Key = key;
        DefaultValue = defaultValue;
        Comparer = comparer;
    }

    public StateKey<T> Key { get; }

    public T DefaultValue { get; }

    public IEqualityComparer<T>? Comparer { get; }

    public static StateDefinition<T> Create(
        StateKey<T> key,
        T defaultValue,
        StateLifetime lifetime = StateLifetime.Application,
        StateAccessPolicy access = StateAccessPolicy.HostWrite,
        StateSnapshotPolicy snapshotPolicy = StateSnapshotPolicy.Transient,
        int schemaVersion = 1,
        string? ownerModule = null,
        string? pluginId = null,
        IEqualityComparer<T>? comparer = null)
    {
        return new StateDefinition<T>(
            key,
            defaultValue,
            lifetime,
            access,
            snapshotPolicy,
            schemaVersion,
            ownerModule,
            pluginId,
            comparer);
    }
}
