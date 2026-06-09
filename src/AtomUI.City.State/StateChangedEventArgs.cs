namespace AtomUI.City.State;

public sealed class StateChangedEventArgs<T> : EventArgs
{
    public StateChangedEventArgs(T oldValue, T newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public T OldValue { get; }

    public T NewValue { get; }
}
