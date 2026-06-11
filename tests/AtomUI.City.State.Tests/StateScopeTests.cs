using AtomUI.City.State;

namespace AtomUI.City.State.Tests;

public sealed class StateScopeTests
{
    [Fact]
    public void StateScopeDisposesSubscriptionsInReverseOrder()
    {
        var calls = new List<string>();
        using var scope = new StateScope("activation");

        scope.Add(new TestSubscription(() => calls.Add("first")));
        scope.Add(new TestSubscription(() => calls.Add("second")));

        scope.Dispose();

        Assert.Equal(["second", "first"], calls);
        Assert.Equal(StateScopeState.Disposed, scope.State);
    }

    [Fact]
    public void ScopedStateSubscriptionStopsNotificationsAfterScopeDisposes()
    {
        var state = new WritableState<int>(0);
        using var scope = new StateScope("activation");
        var calls = 0;

        scope.Add(state.OnChange(_ => calls++));
        scope.Dispose();
        state.SetValue(1);

        Assert.Equal(0, calls);
    }

    private sealed class TestSubscription : IStateSubscription
    {
        private readonly Action _dispose;

        public TestSubscription(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            _dispose();
        }
    }
}
