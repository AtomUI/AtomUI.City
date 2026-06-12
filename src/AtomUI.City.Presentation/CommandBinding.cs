using System.Windows.Input;
using AtomUI.City.Diagnostics;
using AtomUI.City.Mvvm;
using AtomUI.City.Threading;
using CommunityToolkit.Mvvm.Input;

namespace AtomUI.City.Presentation;

public sealed class CommandBinding
{
    private readonly IUiDispatcher _dispatcher;
    private readonly IHostDiagnostics? _diagnostics;

    public CommandBinding(IUiDispatcher dispatcher)
        : this(dispatcher, diagnostics: null)
    {
    }

    public CommandBinding(
        IUiDispatcher dispatcher,
        IHostDiagnostics? diagnostics)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        _dispatcher = dispatcher;
        _diagnostics = diagnostics;
    }

    public async ValueTask<ICommandBindingHandle> BindAsync(
        ICommand command,
        IUiCommandSource source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(source);

        var handle = new CommandBindingHandle(command, source, _dispatcher, _diagnostics);

        try
        {
            await handle.RefreshAsync(cancellationToken).ConfigureAwait(false);

            return handle;
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    public async ValueTask<ICommandBindingHandle> BindAsync(
        ICommand command,
        IUiCommandSource source,
        IActivationScope activationScope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activationScope);

        var handle = await BindAsync(command, source, cancellationToken).ConfigureAwait(false);
        activationScope.Add(handle);

        return handle;
    }

    private sealed class CommandBindingHandle : ICommandBindingHandle
    {
        private readonly ICommand _command;
        private readonly IUiCommandSource _source;
        private readonly IUiDispatcher _dispatcher;
        private readonly IHostDiagnostics? _diagnostics;
        private bool _disposed;

        public CommandBindingHandle(
            ICommand command,
            IUiCommandSource source,
            IUiDispatcher dispatcher,
            IHostDiagnostics? diagnostics)
        {
            _command = command;
            _source = source;
            _dispatcher = dispatcher;
            _diagnostics = diagnostics;

            _command.CanExecuteChanged += HandleCanExecuteChanged;
            _source.ExecuteRequested += HandleExecuteRequested;
        }

        public async ValueTask RefreshAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                return;
            }

            var state = new UiCommandState(
                _command.CanExecute(_source.CommandParameter),
                IsExecuting(_command));

            try
            {
                await _dispatcher
                    .InvokeAsync(
                        () => _source.ApplyCommandState(state),
                        cancellationToken)
                    .ConfigureAwait(false);
                WriteAppliedDiagnostic(state);
            }
            catch (Exception exception)
            {
                WriteFailedDiagnostic(exception);
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _command.CanExecuteChanged -= HandleCanExecuteChanged;
            _source.ExecuteRequested -= HandleExecuteRequested;
        }

        private void HandleCanExecuteChanged(object? sender, EventArgs args)
        {
            _ = RefreshAsync().AsTask();
        }

        private void HandleExecuteRequested(object? sender, EventArgs args)
        {
            if (_disposed)
            {
                return;
            }

            var parameter = _source.CommandParameter;
            if (_command.CanExecute(parameter))
            {
                _command.Execute(parameter);
                _ = RefreshAsync().AsTask();
            }
        }

        private void WriteAppliedDiagnostic(UiCommandState state)
        {
            _diagnostics?.Write(new HostDiagnosticRecord(
                PresentationDiagnosticIds.CommandStateApplied,
                $"Presentation command state applied canExecute '{state.CanExecute}' isExecuting '{state.IsExecuting}'.",
                HostDiagnosticSeverity.Info));
        }

        private void WriteFailedDiagnostic(Exception exception)
        {
            _diagnostics?.Write(new HostDiagnosticRecord(
                PresentationDiagnosticIds.CommandStateApplyFailed,
                $"Presentation command state failed to apply: {exception.Message}",
                HostDiagnosticSeverity.Error));
        }

        private static bool IsExecuting(ICommand command)
        {
            return command is IAsyncRelayCommand { IsRunning: true };
        }
    }
}
