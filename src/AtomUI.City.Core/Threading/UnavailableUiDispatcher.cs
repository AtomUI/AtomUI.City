namespace AtomUI.City.Threading;

public sealed class UnavailableUiDispatcher : IUiDispatcher
{
    private const string ErrorMessage = "UI dispatcher is not available. Register an IUiDispatcher implementation from Presentation or Testing before building the application host.";

    public bool CheckAccess()
    {
        return false;
    }

    public ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled(cancellationToken);
        }

        return ValueTask.FromException(CreateException());
    }

    public ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<T>(cancellationToken);
        }

        return ValueTask.FromException<T>(CreateException());
    }

    public ValueTask PostAsync(
        Func<CancellationToken, ValueTask> callback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled(cancellationToken);
        }

        return ValueTask.FromException(CreateException());
    }

    private static InvalidOperationException CreateException()
    {
        return new InvalidOperationException(ErrorMessage);
    }
}
