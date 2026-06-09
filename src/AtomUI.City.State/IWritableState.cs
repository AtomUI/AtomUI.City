namespace AtomUI.City.State;

public interface IWritableState<T> : IStateValue<T>
{
    event EventHandler<StateChangedEventArgs<T>>? Changed;

    void Set(T value);

    void Update(Func<T, T> updater);
}
