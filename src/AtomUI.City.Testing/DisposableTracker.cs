namespace AtomUI.City.Testing;

public sealed class DisposableTracker : IDisposable, IAsyncDisposable
{
    private readonly List<object> _resources = [];
    private bool _disposed;

    public T Track<T>(T resource)
        where T : notnull
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (resource is not IDisposable && resource is not IAsyncDisposable)
        {
            throw new ArgumentException("Tracked resource must implement IDisposable or IAsyncDisposable.", nameof(resource));
        }

        _resources.Add(resource);

        return resource;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        var exceptions = new List<Exception>();

        for (var index = _resources.Count - 1; index >= 0; index--)
        {
            try
            {
                switch (_resources[index])
                {
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                    case IAsyncDisposable asyncDisposable:
                        asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        break;
                }
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException(exceptions);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        var exceptions = new List<Exception>();

        for (var index = _resources.Count - 1; index >= 0; index--)
        {
            try
            {
                switch (_resources[index])
                {
                    case IAsyncDisposable asyncDisposable:
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                }
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException(exceptions);
        }
    }
}
