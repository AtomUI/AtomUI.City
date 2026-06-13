namespace AtomUI.City.State;

public sealed class StateSnapshot
{
    public StateSnapshot(IReadOnlyList<StateSnapshotEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        Entries = Array.AsReadOnly(entries.ToArray());
    }

    public IReadOnlyList<StateSnapshotEntry> Entries { get; }
}
