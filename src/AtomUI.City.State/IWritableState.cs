namespace AtomUI.City.State;

public interface IWritableState<T> : IReadOnlyState<T>
{
    event EventHandler<StateChangedEventArgs<T>>? Changed;

    bool SetValue(T value);

    bool Update(Func<T, T> updater);

    void Set(T value);
}
