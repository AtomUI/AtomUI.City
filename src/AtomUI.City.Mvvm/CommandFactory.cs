using CommunityToolkit.Mvvm.Input;

namespace AtomUI.City.Mvvm;

public static class CommandFactory
{
    public static IRelayCommand Create(Action execute, Func<bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);

        return canExecute is null
            ? new RelayCommand(execute)
            : new RelayCommand(execute, canExecute);
    }

    public static IAsyncRelayCommand CreateAsync(
        Func<CancellationToken, Task> execute,
        CommandExecutionState? state = null,
        IActivationScope? activationScope = null)
    {
        ArgumentNullException.ThrowIfNull(execute);

        var executionState = state ?? new CommandExecutionState();

        return new AsyncRelayCommand(
            cancellationToken => ExecuteAsync(
                execute,
                executionState,
                activationScope,
                cancellationToken));
    }

    private static async Task ExecuteAsync(
        Func<CancellationToken, Task> execute,
        CommandExecutionState state,
        IActivationScope? activationScope,
        CancellationToken cancellationToken)
    {
        using var linkedCancellationTokenSource = activationScope is null
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, activationScope.CancellationToken);
        var operation = OperationScope.Start(linkedCancellationTokenSource.Token);

        state.Begin(operation.CancellationToken);

        try
        {
            await execute(operation.CancellationToken).ConfigureAwait(false);
            state.Complete(operation.Complete());
        }
        catch (OperationCanceledException)
            when (operation.CancellationToken.IsCancellationRequested)
        {
            state.Complete(operation.Cancel());
        }
        catch (Exception exception)
        {
            state.Complete(operation.Fail(exception));
        }
    }
}
