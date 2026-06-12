using AtomUI.City.Diagnostics;
using AtomUI.City.Threading;
using Avalonia.Threading;

namespace AtomUI.City.Presentation;

public sealed class AvaloniaUiDispatcher : IUiDispatcher
{
    private readonly Dispatcher _dispatcher;
    private readonly IPresentationRuntime? _runtime;
    private readonly IHostDiagnostics? _diagnostics;

    public AvaloniaUiDispatcher()
        : this(Dispatcher.UIThread, runtime: null, diagnostics: null)
    {
    }

    public AvaloniaUiDispatcher(IPresentationRuntime runtime)
        : this(Dispatcher.UIThread, runtime, diagnostics: null)
    {
    }

    public AvaloniaUiDispatcher(Dispatcher dispatcher)
        : this(dispatcher, runtime: null, diagnostics: null)
    {
    }

    public AvaloniaUiDispatcher(Dispatcher dispatcher, IPresentationRuntime? runtime)
        : this(dispatcher, runtime, diagnostics: null)
    {
    }

    public AvaloniaUiDispatcher(
        Dispatcher dispatcher,
        IPresentationRuntime? runtime,
        IHostDiagnostics? diagnostics)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        _dispatcher = dispatcher;
        _runtime = runtime;
        _diagnostics = diagnostics;
    }

    public bool CheckAccess()
    {
        return _dispatcher.CheckAccess();
    }

    public ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled(cancellationToken);
        }

        var runtimeException = CreateRuntimeException();
        if (runtimeException is not null)
        {
            WriteOperationRejectedDiagnostic(runtimeException);

            return ValueTask.FromException(runtimeException);
        }

        if (_dispatcher.CheckAccess())
        {
            try
            {
                callback();

                return ValueTask.CompletedTask;
            }
            catch (Exception exception)
            {
                WriteCallbackFailedDiagnostic(exception);

                return ValueTask.FromException(exception);
            }
        }

        var operation = _dispatcher.InvokeAsync(callback, DispatcherPriority.Default, cancellationToken);

        return new ValueTask(operation.GetTask());
    }

    public ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<T>(cancellationToken);
        }

        var runtimeException = CreateRuntimeException();
        if (runtimeException is not null)
        {
            WriteOperationRejectedDiagnostic(runtimeException);

            return ValueTask.FromException<T>(runtimeException);
        }

        if (_dispatcher.CheckAccess())
        {
            try
            {
                return ValueTask.FromResult(callback());
            }
            catch (Exception exception)
            {
                WriteCallbackFailedDiagnostic(exception);

                return ValueTask.FromException<T>(exception);
            }
        }

        var operation = _dispatcher.InvokeAsync(callback, DispatcherPriority.Default, cancellationToken);

        return new ValueTask<T>(operation.GetTask());
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

        var runtimeException = CreateRuntimeException();
        if (runtimeException is not null)
        {
            WriteOperationRejectedDiagnostic(runtimeException);

            return ValueTask.FromException(runtimeException);
        }

        if (_dispatcher.CheckAccess())
        {
            try
            {
                return callback(cancellationToken);
            }
            catch (Exception exception)
            {
                WriteCallbackFailedDiagnostic(exception);

                return ValueTask.FromException(exception);
            }
        }

        var taskCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancellationRegistration = cancellationToken.CanBeCanceled
            ? cancellationToken.Register(
                static state => ((TaskCompletionSource)state!).TrySetCanceled(),
                taskCompletion)
            : default;

        try
        {
            _dispatcher.Post(
                async () =>
                {
                    if (taskCompletion.Task.IsCompleted)
                    {
                        cancellationRegistration.Dispose();
                        return;
                    }

                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await callback(cancellationToken).ConfigureAwait(false);
                        taskCompletion.TrySetResult();
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        taskCompletion.TrySetCanceled(cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        WriteCallbackFailedDiagnostic(exception);
                        taskCompletion.TrySetException(exception);
                    }
                    finally
                    {
                        cancellationRegistration.Dispose();
                    }
                },
                DispatcherPriority.Default);
        }
        catch (Exception exception)
        {
            cancellationRegistration.Dispose();
            taskCompletion.TrySetException(exception);
        }

        return new ValueTask(taskCompletion.Task);
    }

    private PresentationException? CreateRuntimeException()
    {
        return _runtime?.State switch
        {
            PresentationRuntimeState.NotReady => new PresentationException(
                PresentationError.RuntimeNotReady,
                "Presentation runtime is not ready."),
            PresentationRuntimeState.Stopping or
                PresentationRuntimeState.Stopped or
                PresentationRuntimeState.Faulted => new PresentationException(
                    PresentationError.RuntimeStopping,
                    "Presentation runtime is not accepting UI dispatcher operations."),
            _ => null,
        };
    }

    private void WriteOperationRejectedDiagnostic(PresentationException exception)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.DispatcherOperationRejected,
            exception.Message,
            HostDiagnosticSeverity.Warning,
            ScopeId: _runtime?.PresentationScope?.Id));
    }

    private void WriteCallbackFailedDiagnostic(Exception exception)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.DispatcherCallbackFailed,
            exception.Message,
            HostDiagnosticSeverity.Error,
            ScopeId: _runtime?.PresentationScope?.Id));
    }
}
