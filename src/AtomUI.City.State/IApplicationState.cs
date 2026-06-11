namespace AtomUI.City.State;

public interface IApplicationState
{
    IReadOnlyState<T> Get<T>(StateKey<T> key);

    IStateSubscription OnChange<T>(
        StateKey<T> key,
        Action<StateChangedEventArgs<T>> handler);
}
