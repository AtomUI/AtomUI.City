using AtomUI.City.Presentation;
using AtomUI.City.Threading;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Presentation.Tests;

public sealed class AvaloniaUiDispatcherTests
{
    [Fact]
    public void ServiceCollectionRegistersAvaloniaUiDispatcherAsUiDispatcher()
    {
        var services = new ServiceCollection();

        services.AddAvaloniaUiDispatcher();

        using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IUiDispatcher>();

        Assert.IsType<AvaloniaUiDispatcher>(dispatcher);
    }

    [Fact]
    public void ServiceCollectionDoesNotReplaceExistingUiDispatcher()
    {
        var existingDispatcher = new InlineUiDispatcher();
        var services = new ServiceCollection();

        services.AddSingleton<IUiDispatcher>(existingDispatcher);
        services.AddAvaloniaUiDispatcher();

        using var provider = services.BuildServiceProvider();

        Assert.Same(existingDispatcher, provider.GetRequiredService<IUiDispatcher>());
    }

    [Fact]
    public void CheckAccessDelegatesToAvaloniaDispatcher()
    {
        var avaloniaDispatcher = Dispatcher.CurrentDispatcher;
        var dispatcher = new AvaloniaUiDispatcher(avaloniaDispatcher);

        Assert.Equal(avaloniaDispatcher.CheckAccess(), dispatcher.CheckAccess());
    }

    [Fact]
    public async Task InvokeAsyncRunsCallbacksWhenDispatcherOwnsCurrentThread()
    {
        var dispatcher = new AvaloniaUiDispatcher(Dispatcher.CurrentDispatcher);
        var wasCalled = false;

        await dispatcher.InvokeAsync(() => wasCalled = true);
        var value = await dispatcher.InvokeAsync(() => 42);

        Assert.True(wasCalled);
        Assert.Equal(42, value);
    }

    [Fact]
    public async Task PostAsyncRunsCallbackWhenDispatcherOwnsCurrentThread()
    {
        var dispatcher = new AvaloniaUiDispatcher(Dispatcher.CurrentDispatcher);
        var callCount = 0;

        await dispatcher.PostAsync(_ =>
        {
            callCount++;
            return ValueTask.CompletedTask;
        });

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task OperationsHonorPreCanceledToken()
    {
        var dispatcher = new AvaloniaUiDispatcher(Dispatcher.CurrentDispatcher);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var wasCalled = false;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => dispatcher.InvokeAsync(() => wasCalled = true, cancellation.Token).AsTask());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => dispatcher.InvokeAsync(() => 42, cancellation.Token).AsTask());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => dispatcher.PostAsync(
                _ =>
                {
                    wasCalled = true;
                    return ValueTask.CompletedTask;
                },
                cancellation.Token).AsTask());

        Assert.False(wasCalled);
    }

    private sealed class InlineUiDispatcher : IUiDispatcher
    {
        public bool CheckAccess()
        {
            return true;
        }

        public ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default)
        {
            callback();

            return ValueTask.CompletedTask;
        }

        public ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(callback());
        }

        public ValueTask PostAsync(
            Func<CancellationToken, ValueTask> callback,
            CancellationToken cancellationToken = default)
        {
            return callback(cancellationToken);
        }
    }
}
