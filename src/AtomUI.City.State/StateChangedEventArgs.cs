namespace AtomUI.City.State;

public class StateChangedEventArgs : EventArgs
{
    public StateChangedEventArgs(
        object? oldValue,
        object? newValue,
        long version)
    {
        OldValue = oldValue;
        NewValue = newValue;
        Version = version;
    }

    public object? OldValue { get; }

    public object? NewValue { get; }

    public long Version { get; }
}

public sealed class StateChangedEventArgs<T> : StateChangedEventArgs
{
    public StateChangedEventArgs(T oldValue, T newValue)
        : this(oldValue, newValue, version: 0)
    {
    }

    public StateChangedEventArgs(
        T oldValue,
        T newValue,
        long version)
        : base(oldValue, newValue, version)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public new T OldValue { get; }

    public new T NewValue { get; }
}
