using AtomUI.City.Mvvm;
using CommunityToolkit.Mvvm.Input;

namespace AtomUI.City.Mvvm.Tests;

public sealed class CommandTests
{
    [Fact]
    public void CreateCommandUsesCommunityToolkitRelayCommand()
    {
        var calls = 0;
        var command = CommandFactory.Create(() => calls++);

        Assert.IsAssignableFrom<IRelayCommand>(command);

        command.Execute(null);

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task AsyncCommandTracksSuccessfulExecution()
    {
        var state = new CommandExecutionState();
        var command = CommandFactory.CreateAsync(
            async cancellationToken =>
            {
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();
            },
            state);

        Assert.IsAssignableFrom<IAsyncRelayCommand>(command);

        await command.ExecuteAsync(null);

        Assert.False(state.IsExecuting);
        Assert.Equal(OperationStatus.Completed, state.LastResult?.Status);
        Assert.Null(state.LastError);
    }

    [Fact]
    public async Task AsyncCommandCapturesFailureWithoutThrowing()
    {
        var state = new CommandExecutionState();
        var command = CommandFactory.CreateAsync(
            _ => throw new InvalidOperationException("boom"),
            state);

        await command.ExecuteAsync(null);

        Assert.False(state.IsExecuting);
        Assert.Equal(OperationStatus.Failed, state.LastResult?.Status);
        Assert.IsType<InvalidOperationException>(state.LastError);
    }

    [Fact]
    public async Task AsyncCommandIsCanceledWhenActivationScopeStops()
    {
        await using var scope = new ActivationScope();
        var state = new CommandExecutionState();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = CommandFactory.CreateAsync(
            async cancellationToken =>
            {
                started.SetResult();
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            },
            state,
            scope);

        var execution = command.ExecuteAsync(null);
        await started.Task;

        await scope.DisposeAsync();
        await execution;

        Assert.Equal(OperationStatus.Canceled, state.LastResult?.Status);
        Assert.Null(state.LastError);
    }

    [Fact]
    public void CommandGroupExecutesOnlyActiveCommands()
    {
        var firstCalls = 0;
        var secondCalls = 0;
        var group = new CommandGroup();

        group.Register(CommandFactory.Create(() => firstCalls++), isActive: () => true);
        group.Register(CommandFactory.Create(() => secondCalls++), isActive: () => false);

        group.Execute(null);

        Assert.Equal(1, firstCalls);
        Assert.Equal(0, secondCalls);
    }

    [Fact]
    public void CommandGroupRegistrationIsRemovedWithActivationScope()
    {
        var calls = 0;
        var scope = new ActivationScope();
        var group = new CommandGroup();

        group.Register(CommandFactory.Create(() => calls++), activationScope: scope);
        scope.Dispose();
        group.Execute(null);

        Assert.Equal(0, calls);
    }
}
