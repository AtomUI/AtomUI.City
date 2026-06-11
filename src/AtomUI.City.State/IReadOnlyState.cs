namespace AtomUI.City.State;

public interface IReadOnlyState
{
    object? Value { get; }

    long Version { get; }

    Type ValueType { get; }

    IStateSubscription OnChange(Action<StateChangedEventArgs> handler);

    IStateSubscription OnChange(
        Action<StateChangedEventArgs> handler,
        StateSubscriptionOptions options);
}

public interface IReadOnlyState<T> : IReadOnlyState, IStateValue<T>
{
    new T Value { get; }

    IStateSubscription OnChange(Action<StateChangedEventArgs<T>> handler);

    IStateSubscription OnChange(
        Action<StateChangedEventArgs<T>> handler,
        StateSubscriptionOptions options);
}
