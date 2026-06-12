using AtomUI.City.Diagnostics;
using AtomUI.City.Lifecycle;

namespace AtomUI.City.Presentation;

public sealed class PresentationRuntime : IPresentationRuntime
{
    private readonly object _syncRoot = new();
    private readonly IHostDiagnostics? _diagnostics;
    private LifecycleScope? _presentationScope;
    private PresentationRuntimeState _state = PresentationRuntimeState.NotReady;

    public PresentationRuntime(IHostDiagnostics? diagnostics = null)
    {
        _diagnostics = diagnostics;
    }

    public PresentationRuntimeState State
    {
        get
        {
            lock (_syncRoot)
            {
                return _state;
            }
        }
    }

    public bool IsReady
    {
        get
        {
            lock (_syncRoot)
            {
                return _state == PresentationRuntimeState.Ready;
            }
        }
    }

    public LifecycleScope? PresentationScope
    {
        get
        {
            lock (_syncRoot)
            {
                return _presentationScope;
            }
        }
    }

    public ValueTask StartAsync(
        LifecycleScope applicationScope,
        string presentationScopeId = "presentation",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(applicationScope);
        ArgumentException.ThrowIfNullOrWhiteSpace(presentationScopeId);
        cancellationToken.ThrowIfCancellationRequested();

        LifecycleScope presentationScope;
        lock (_syncRoot)
        {
            if (_state == PresentationRuntimeState.Ready)
            {
                return ValueTask.CompletedTask;
            }

            if (_state is PresentationRuntimeState.Stopping or PresentationRuntimeState.Stopped)
            {
                throw CreateRuntimeStoppingException();
            }

            presentationScope = applicationScope.CreateChild(
                LifecycleScopeKind.Presentation,
                presentationScopeId);
            _presentationScope = presentationScope;
            _state = PresentationRuntimeState.Ready;
        }

        WriteDiagnostic(
            PresentationDiagnosticIds.RuntimeReady,
            "Presentation runtime is ready.",
            presentationScope.Id);

        return ValueTask.CompletedTask;
    }

    public LifecycleScope CreateWindowScope(string windowScopeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(windowScopeId);

        lock (_syncRoot)
        {
            if (_state == PresentationRuntimeState.NotReady || _presentationScope is null)
            {
                throw CreateRuntimeNotReadyException();
            }

            if (_state is not PresentationRuntimeState.Ready)
            {
                throw CreateRuntimeStoppingException();
            }

            return _presentationScope.CreateChild(
                LifecycleScopeKind.Window,
                windowScopeId);
        }
    }

    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LifecycleScope? presentationScope;
        lock (_syncRoot)
        {
            if (_state is PresentationRuntimeState.Stopping or PresentationRuntimeState.Stopped)
            {
                return;
            }

            presentationScope = _presentationScope;
            _state = PresentationRuntimeState.Stopping;

            if (presentationScope is null)
            {
                _state = PresentationRuntimeState.Stopped;
                return;
            }
        }

        WriteDiagnostic(
            PresentationDiagnosticIds.RuntimeStopping,
            "Presentation runtime is stopping.",
            presentationScope.Id);

        try
        {
            await presentationScope.StopAsync().ConfigureAwait(false);

            lock (_syncRoot)
            {
                _state = PresentationRuntimeState.Stopped;
            }
        }
        catch
        {
            lock (_syncRoot)
            {
                _state = PresentationRuntimeState.Faulted;
            }

            throw;
        }
    }

    private void WriteDiagnostic(
        string code,
        string message,
        string scopeId)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            code,
            message,
            HostDiagnosticSeverity.Info,
            ScopeId: scopeId));
    }

    private static PresentationException CreateRuntimeNotReadyException()
    {
        return new PresentationException(
            PresentationError.RuntimeNotReady,
            "Presentation runtime is not ready.");
    }

    private static PresentationException CreateRuntimeStoppingException()
    {
        return new PresentationException(
            PresentationError.RuntimeStopping,
            "Presentation runtime is stopping or stopped.");
    }
}
