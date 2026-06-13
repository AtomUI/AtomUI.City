using AtomUI.City.Diagnostics;

namespace AtomUI.City.State;

public sealed class ApplicationStateRegistry :
    IApplicationState,
    IApplicationStateWriter,
    IStateRegistry
{
    private readonly IHostDiagnostics? _diagnostics;
    private readonly Dictionary<string, StateRegistration> _registrations = new(StringComparer.Ordinal);

    public ApplicationStateRegistry(IHostDiagnostics? diagnostics = null)
    {
        _diagnostics = diagnostics;
    }

    public void Add<T>(StateDefinition<T> definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (_registrations.ContainsKey(definition.Key.Name))
        {
            WriteAlreadyRegisteredDiagnostic(definition.Key.Name, typeof(T));
            throw new InvalidOperationException($"State '{definition.Key.Name}' is already registered.");
        }

        _registrations.Add(
            definition.Key.Name,
            new StateRegistration<T>(
                definition,
                new WritableState<T>(
                    definition.DefaultValue,
                    definition.Comparer,
                    _diagnostics)));
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

            registration.Restore(entry, _diagnostics);
        }
    }

    private StateRegistration<T> GetRegistration<T>(StateKey<T> key)
    {
        if (!_registrations.TryGetValue(key.Name, out var registration))
        {
            WriteNotRegisteredDiagnostic(key.Name, typeof(T));
            throw new StateNotRegisteredException(key.Name);
        }

        if (registration is StateRegistration<T> typedRegistration)
        {
            return typedRegistration;
        }

        WriteNotRegisteredDiagnostic(key.Name, typeof(T));
        var message = $"State '{key.Name}' is not registered with value type '{typeof(T).FullName}'.";

        throw new InvalidOperationException(message);
    }

    private void ThrowIfWriteDenied<T>(StateRegistration<T> registration)
    {
        if (registration.Definition.Access == StateAccessPolicy.ReadOnly)
        {
            WriteWriteDeniedDiagnostic(
                registration.Definition.Key.Name,
                typeof(T),
                registration.Definition.Access);
            throw new StateAccessDeniedException(registration.Definition.Key.Name);
        }
    }

    private void WriteNotRegisteredDiagnostic(string stateName, Type valueType)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            StateDiagnosticIds.ApplicationStateNotRegistered,
            $"Application state '{stateName}' with value type '{valueType.FullName}' is not registered.",
            HostDiagnosticSeverity.Warning));
    }

    private void WriteAlreadyRegisteredDiagnostic(string stateName, Type valueType)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            StateDiagnosticIds.ApplicationStateAlreadyRegistered,
            $"Application state '{stateName}' with value type '{valueType.FullName}' is already registered.",
            HostDiagnosticSeverity.Warning));
    }

    private void WriteWriteDeniedDiagnostic(
        string stateName,
        Type valueType,
        StateAccessPolicy access)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            StateDiagnosticIds.ApplicationStateWriteDenied,
            $"Application state '{stateName}' with value type '{valueType.FullName}' rejected write because access policy is '{access}'.",
            HostDiagnosticSeverity.Warning));
    }

    private abstract class StateRegistration
    {
        protected StateRegistration(StateDefinition definition)
        {
            Definition = definition;
        }

        public StateDefinition Definition { get; }

        public abstract StateSnapshotEntry CreateSnapshotEntry();

        public abstract void Restore(
            StateSnapshotEntry entry,
            IHostDiagnostics? diagnostics);
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

        public override void Restore(
            StateSnapshotEntry entry,
            IHostDiagnostics? diagnostics)
        {
            if (!string.Equals(entry.PluginId, Definition.PluginId, StringComparison.Ordinal))
            {
                WriteRestoreFailedDiagnostic(
                    diagnostics,
                    entry,
                    $"plugin id '{entry.PluginId ?? "<none>"}' does not match expected plugin id '{Definition.PluginId ?? "<none>"}'");
                return;
            }

            if (!string.Equals(entry.OwnerModule, Definition.OwnerModule, StringComparison.Ordinal))
            {
                WriteRestoreFailedDiagnostic(
                    diagnostics,
                    entry,
                    $"owner module '{entry.OwnerModule ?? "<none>"}' does not match expected owner module '{Definition.OwnerModule ?? "<none>"}'");
                return;
            }

            if (entry.SchemaVersion != Definition.SchemaVersion)
            {
                WriteRestoreFailedDiagnostic(
                    diagnostics,
                    entry,
                    $"schema version '{entry.SchemaVersion}' does not match expected schema version '{Definition.SchemaVersion}'");
                return;
            }

            if (entry.Value is T value)
            {
                State.Restore(value, entry.Version);
            }
            else
            {
                WriteRestoreFailedDiagnostic(
                    diagnostics,
                    entry,
                    $"value type '{entry.ValueType.FullName}' cannot be restored as '{typeof(T).FullName}'");
            }
        }

        private void WriteRestoreFailedDiagnostic(
            IHostDiagnostics? diagnostics,
            StateSnapshotEntry entry,
            string reason)
        {
            diagnostics?.Write(new HostDiagnosticRecord(
                StateDiagnosticIds.SnapshotRestoreFailed,
                $"State snapshot restore failed for state '{entry.StateName}': {reason}.",
                HostDiagnosticSeverity.Warning));
        }
    }
}
