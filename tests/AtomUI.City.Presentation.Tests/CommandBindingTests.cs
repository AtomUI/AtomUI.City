using System.Windows.Input;
using AtomUI.City.Diagnostics;
using AtomUI.City.Mvvm;
using AtomUI.City.Presentation;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation.Tests;

public sealed class CommandBindingTests
{
    [Fact]
    public async Task BindAsyncAppliesInitialCanExecuteOnUiDispatcher()
    {
        var dispatcher = new RecordingDispatcher();
        var binding = new CommandBinding(dispatcher);
        var command = new ToggleCommand(canExecute: true);
        var source = new RecordingCommandSource(dispatcher);

        using var handle = await binding.BindAsync(command, source);

        Assert.True(source.WasOnDispatcher);
        Assert.Equal([true], source.CanExecuteStates);
        Assert.Equal(1, dispatcher.InvokeCount);
    }

    [Fact]
    public async Task BindingRefreshesCommandSourceWhenCanExecuteChanges()
    {
        var dispatcher = new RecordingDispatcher();
        var binding = new CommandBinding(dispatcher);
        var command = new ToggleCommand(canExecute: false);
        var source = new RecordingCommandSource(dispatcher);

        using var handle = await binding.BindAsync(command, source);
        command.SetCanExecute(true);

        Assert.Equal([false, true], source.CanExecuteStates);
        Assert.Equal(2, dispatcher.InvokeCount);
    }

    [Fact]
    public async Task BindingExecutesCommandWhenSourceRequestsExecution()
    {
        var binding = new CommandBinding(new RecordingDispatcher());
        var command = new ToggleCommand(canExecute: true);
        var source = new RecordingCommandSource();

        using var handle = await binding.BindAsync(command, source);
        source.RequestExecute("save");

        Assert.Equal(["save"], command.Executions);
    }

    [Fact]
    public async Task ActivationScopeDisposesCommandBinding()
    {
        var binding = new CommandBinding(new RecordingDispatcher());
        var command = new ToggleCommand(canExecute: false);
        var source = new RecordingCommandSource();
        using var activationScope = new ActivationScope();

        await binding.BindAsync(command, source, activationScope);
        activationScope.Dispose();
        command.SetCanExecute(true);
        source.RequestExecute("save");

        Assert.Equal([false], source.CanExecuteStates);
        Assert.Empty(command.Executions);
    }

    [Fact]
    public async Task BindingMapsAsyncCommandExecutingState()
    {
        var dispatcher = new RecordingDispatcher();
        var binding = new CommandBinding(dispatcher);
        var source = new RecordingCommandSource(dispatcher);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = CommandFactory.CreateAsync(
            async cancellationToken =>
            {
                started.SetResult();
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            });

        using var handle = await binding.BindAsync(command, source);
        source.RequestExecute(null);
        await started.Task;
        await handle.RefreshAsync();

        Assert.Contains(true, source.IsExecutingStates);
        command.Cancel();
        await handle.RefreshAsync();

        Assert.Contains(false, source.IsExecutingStates);
    }

    [Fact]
    public async Task BindingRecordsAppliedAndFailureDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var binding = new CommandBinding(new RecordingDispatcher(), diagnostics);
        var command = new ToggleCommand(canExecute: true);
        var source = new RecordingCommandSource
        {
            Failure = new InvalidOperationException("command state failed"),
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => binding.BindAsync(command, source).AsTask());

        Assert.Equal("command state failed", exception.Message);
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.CommandStateApplyFailed &&
                record.Severity == HostDiagnosticSeverity.Error);
    }

    private sealed class RecordingCommandSource : IUiCommandSource
    {
        private readonly RecordingDispatcher? _dispatcher;

        public RecordingCommandSource()
        {
        }

        public RecordingCommandSource(RecordingDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public event EventHandler? ExecuteRequested;

        public List<bool> CanExecuteStates { get; } = [];

        public List<bool> IsExecutingStates { get; } = [];

        public bool WasOnDispatcher { get; private set; }

        public object? CommandParameter { get; private set; }

        public Exception? Failure { get; init; }

        public void ApplyCommandState(UiCommandState state)
        {
            if (Failure is not null)
            {
                throw Failure;
            }

            WasOnDispatcher = _dispatcher?.IsOnDispatcher ?? true;
            CanExecuteStates.Add(state.CanExecute);
            IsExecutingStates.Add(state.IsExecuting);
        }

        public void RequestExecute(object? parameter)
        {
            CommandParameter = parameter;
            ExecuteRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class ToggleCommand : ICommand
    {
        private bool _canExecute;

        public ToggleCommand(bool canExecute)
        {
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public List<object?> Executions { get; } = [];

        public bool CanExecute(object? parameter)
        {
            return _canExecute;
        }

        public void Execute(object? parameter)
        {
            Executions.Add(parameter);
        }

        public void SetCanExecute(bool value)
        {
            _canExecute = value;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class RecordingDispatcher : IUiDispatcher
    {
        public bool IsOnDispatcher { get; private set; }

        public int InvokeCount { get; private set; }

        public bool CheckAccess()
        {
            return IsOnDispatcher;
        }

        public ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default)
        {
            InvokeCount++;
            cancellationToken.ThrowIfCancellationRequested();
            IsOnDispatcher = true;

            try
            {
                callback();
            }
            finally
            {
                IsOnDispatcher = false;
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)
        {
            InvokeCount++;
            cancellationToken.ThrowIfCancellationRequested();
            IsOnDispatcher = true;

            try
            {
                return ValueTask.FromResult(callback());
            }
            finally
            {
                IsOnDispatcher = false;
            }
        }

        public ValueTask PostAsync(
            Func<CancellationToken, ValueTask> callback,
            CancellationToken cancellationToken = default)
        {
            return callback(cancellationToken);
        }
    }
}
