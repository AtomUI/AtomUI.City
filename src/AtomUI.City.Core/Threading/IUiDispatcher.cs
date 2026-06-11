namespace AtomUI.City.Threading;

public interface IUiDispatcher
{
    bool CheckAccess();

    ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default);

    ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default);

    ValueTask PostAsync(
        Func<CancellationToken, ValueTask> callback,
        CancellationToken cancellationToken = default);
}
