using AtomUI.City.Mvvm;

namespace AtomUI.City.Mvvm.Tests;

public sealed class InteractionTests
{
    [Fact]
    public async Task RequestAsyncReturnsNotHandledWhenNoHandlerExists()
    {
        var interaction = new Interaction<ConfirmRequest, bool>();

        var result = await interaction.RequestAsync(new ConfirmRequest("Delete?"));

        Assert.Equal(InteractionResultStatus.NotHandled, result.Status);
    }

    [Fact]
    public async Task RequestAsyncReturnsCompletedResultFromRegisteredHandler()
    {
        using var scope = new ActivationScope();
        var interaction = new Interaction<ConfirmRequest, bool>();
        interaction.RegisterHandler(
            (context, _) => ValueTask.FromResult(context.Request.Message == "Delete?"),
            scope);

        var result = await interaction.RequestAsync(new ConfirmRequest("Delete?"));

        Assert.Equal(InteractionResultStatus.Completed, result.Status);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task HandlerIsRemovedWhenActivationScopeIsDisposed()
    {
        var scope = new ActivationScope();
        var interaction = new Interaction<ConfirmRequest, bool>();
        interaction.RegisterHandler(
            (_, _) => ValueTask.FromResult(true),
            scope);

        scope.Dispose();

        var result = await interaction.RequestAsync(new ConfirmRequest("Delete?"));

        Assert.Equal(InteractionResultStatus.NotHandled, result.Status);
    }

    [Fact]
    public async Task PendingInteractionIsCanceledWhenActivationScopeIsDisposed()
    {
        var scope = new ActivationScope();
        var interaction = new Interaction<ConfirmRequest, bool>();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        interaction.RegisterHandler(
            async (_, cancellationToken) =>
            {
                started.SetResult();
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                return true;
            },
            scope);

        var request = interaction.RequestAsync(new ConfirmRequest("Delete?")).AsTask();
        await started.Task;

        scope.Dispose();
        var result = await request;

        Assert.Equal(InteractionResultStatus.Canceled, result.Status);
    }

    private readonly record struct ConfirmRequest(string Message);
}
