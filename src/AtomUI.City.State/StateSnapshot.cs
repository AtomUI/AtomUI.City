namespace AtomUI.City.State;

public sealed class StateSnapshot
{
    public StateSnapshot(IReadOnlyList<StateSnapshotEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        Entries = entries.ToArray();
    }

    public IReadOnlyList<StateSnapshotEntry> Entries { get; }
}
