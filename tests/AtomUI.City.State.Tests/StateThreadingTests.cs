using AtomUI.City.State;
using AtomUI.City.Threading;

namespace AtomUI.City.State.Tests;

public sealed class StateThreadingTests
{
    [Fact]
    public async Task ConcurrentUpdatesAreAtomicAndVersioned()
    {
        var state = new WritableState<int>(0);
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() => state.Update(value => value + 1)))
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.Equal(100, state.Value);
        Assert.Equal(100, state.Version);
    }

    [Fact]
    public void ChangeNotificationsRunOutsideMutationLock()
    {
        var state = new WritableState<int>(0);

        state.OnChange(_ =>
        {
            if (state.Value == 1)
            {
                state.SetValue(2);
            }
        });

        state.SetValue(1);

        Assert.Equal(2, state.Value);
        Assert.Equal(2, state.Version);
    }

    [Fact]
    public void DispatcherSubscriptionUsesUiDispatcher()
    {
        var dispatcher = new RecordingDispatcher();
        var state = new WritableState<int>(0);
        var observed = 0;

        state.OnChange(
            args => observed = args.NewValue,
            StateSubscriptionOptions.Dispatcher(dispatcher));

        state.SetValue(5);

        Assert.Equal(5, observed);
        Assert.Equal(1, dispatcher.InvokeCount);
    }

    [Fact]
    public void ReadOnlyStateSubscriptionCanDeclareDispatcherPolicy()
    {
        var dispatcher = new RecordingDispatcher();
        IReadOnlyState<int> state = new WritableState<int>(0);
        var observed = 0;

        state.OnChange(
            args => observed = args.NewValue,
            StateSubscriptionOptions.Dispatcher(dispatcher));

        ((IWritableState<int>)state).SetValue(7);

        Assert.Equal(7, observed);
        Assert.Equal(1, dispatcher.InvokeCount);
    }

    [Fact]
    public void BackgroundSubscriptionUsesBackgroundDispatchPolicy()
    {
        var state = new WritableState<int>(0);
        var observed = 0;
        var options = StateSubscriptionOptions.Background();

        state.OnChange(
            args => observed = args.NewValue,
            options);

        state.SetValue(9);

        Assert.Equal(9, observed);
        Assert.Equal(StateDispatchPolicy.Background, options.DispatchPolicy);
    }

    [Fact]
    public async Task QueuedSubscriptionDispatchesNotificationsInOrderWithoutBlockingSetValue()
    {
        var state = new WritableState<int>(0);
        var observed = new List<int>();
        using var releaseFirstHandler = new ManualResetEventSlim(false);
        var firstHandlerEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var syncRoot = new object();
        var options = StateSubscriptionOptions.Queued();

        state.OnChange(
            args =>
            {
                if (args.NewValue == 1)
                {
                    firstHandlerEntered.SetResult();
                    Assert.True(releaseFirstHandler.Wait(TimeSpan.FromSeconds(5)));
                }

                lock (syncRoot)
                {
                    observed.Add(args.NewValue);

                    if (observed.Count == 2)
                    {
                        completion.SetResult();
                    }
                }
            },
            options);

        var setValues = Task.Run(() =>
        {
            state.SetValue(1);
            state.SetValue(2);
        });

        await firstHandlerEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var completedBeforeRelease = await Task.WhenAny(
            setValues,
            Task.Delay(TimeSpan.FromSeconds(1))) == setValues;

        releaseFirstHandler.Set();

        Assert.True(completedBeforeRelease);
        await setValues.WaitAsync(TimeSpan.FromSeconds(5));
        await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal([1, 2], observed);
        Assert.Equal(StateDispatchPolicy.Queued, options.DispatchPolicy);
    }

    private sealed class RecordingDispatcher : IUiDispatcher
    {
        public int InvokeCount { get; private set; }

        public bool CheckAccess() => true;

        public ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default)
        {
            InvokeCount++;
            callback();

            return ValueTask.CompletedTask;
        }

        public ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)
        {
            InvokeCount++;

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
