namespace AtomUI.City.State;

public sealed class ApplicationStateRegistry :
    IApplicationState,
    IApplicationStateWriter,
    IStateRegistry
{
    private readonly Dictionary<string, StateRegistration> _registrations = new(StringComparer.Ordinal);

    public void Add<T>(StateDefinition<T> definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (_registrations.ContainsKey(definition.Key.Name))
        {
            throw new InvalidOperationException($"State '{definition.Key.Name}' is already registered.");
        }

        _registrations.Add(
            definition.Key.Name,
            new StateRegistration<T>(
                definition,
                new WritableState<T>(definition.DefaultValue, definition.Comparer)));
    }

    public IReadOnlyState<T> Get<T>(StateKey<T> key)
    {
        return GetRegistration<T>(key).State;
    }

    public IWritableState<T> GetWritable<T>(StateKey<T> key)
    {
        var registration = GetRegistration<T>(key);
        ThrowIfWriteDenied(registration);

        return registration.State;
    }

    public IStateSubscription OnChange<T>(
        StateKey<T> key,
        Action<StateChangedEventArgs<T>> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return Get(key).OnChange(handler);
    }

    public bool Set<T>(StateKey<T> key, T value)
    {
        return GetWritable(key).SetValue(value);
    }

    public bool Update<T>(StateKey<T> key, Func<T, T> updater)
    {
        return GetWritable(key).Update(updater);
    }

    public StateSnapshot CreateSnapshot()
    {
        var entries = _registrations
            .Values
            .Where(registration => registration.Definition.SnapshotPolicy == StateSnapshotPolicy.Persisted)
            .Select(registration => registration.CreateSnapshotEntry())
            .ToArray();

        return new StateSnapshot(entries);
    }

    public void Restore(StateSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        foreach (var entry in snapshot.Entries)
        {
            if (!_registrations.TryGetValue(entry.StateName, out var registration))
            {
                continue;
            }

            registration.Restore(entry);
        }
    }

    private StateRegistration<T> GetRegistration<T>(StateKey<T> key)
    {
        if (!_registrations.TryGetValue(key.Name, out var registration))
        {
            throw new StateNotRegisteredException(key.Name);
        }

        if (registration is StateRegistration<T> typedRegistration)
        {
            return typedRegistration;
        }

        throw new InvalidOperationException($"State '{key.Name}' is not registered with value type '{typeof(T).FullName}'.");
    }

    private static void ThrowIfWriteDenied<T>(StateRegistration<T> registration)
    {
        if (registration.Definition.Access == StateAccessPolicy.ReadOnly)
        {
            throw new StateAccessDeniedException(registration.Definition.Key.Name);
        }
    }

    private abstract class StateRegistration
    {
        protected StateRegistration(StateDefinition definition)
        {
            Definition = definition;
        }

        public StateDefinition Definition { get; }

        public abstract StateSnapshotEntry CreateSnapshotEntry();

        public abstract void Restore(StateSnapshotEntry entry);
    }

    private sealed class StateRegistration<T> : StateRegistration
    {
        public StateRegistration(
            StateDefinition<T> definition,
            WritableState<T> state)
            : base(definition)
        {
            Definition = definition;
            State = state;
        }

        public new StateDefinition<T> Definition { get; }

        public WritableState<T> State { get; }

        public override StateSnapshotEntry CreateSnapshotEntry()
        {
            return new StateSnapshotEntry(
                Definition.Key.Name,
                typeof(T),
                State.Value,
                State.Version,
                Definition.SchemaVersion,
                Definition.OwnerModule,
                Definition.PluginId);
        }

        public override void Restore(StateSnapshotEntry entry)
        {
            if (entry.SchemaVersion != Definition.SchemaVersion)
            {
                return;
            }

            if (entry.Value is T value)
            {
                State.Restore(value, entry.Version);
            }
        }
    }
}
