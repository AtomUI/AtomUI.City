namespace AtomUI.City.State;

public sealed class WritableState<T> : IWritableState<T>
{
    private T _value;

    public WritableState(T initialValue)
    {
        _value = initialValue;
    }

    public event EventHandler<StateChangedEventArgs<T>>? Changed;

    public T Value => _value;

    public void Set(T value)
    {
        if (EqualityComparer<T>.Default.Equals(_value, value))
        {
            return;
        }

        var oldValue = _value;
        _value = value;
        Changed?.Invoke(this, new StateChangedEventArgs<T>(oldValue, value));
    }

    public void Update(Func<T, T> updater)
    {
        ArgumentNullException.ThrowIfNull(updater);

        Set(updater(_value));
    }
}
