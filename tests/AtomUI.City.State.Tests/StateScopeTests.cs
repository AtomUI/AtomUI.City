using AtomUI.City.Diagnostics;
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

    [Fact]
    public void StateScopeRecordsDisposeFailuresAndContinuesDisposal()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var calls = new List<string>();
        var scope = new StateScope("activation", diagnostics);

        scope.Add(new TestSubscription(() => calls.Add("first")));
        scope.Add(new TestSubscription(() => throw new InvalidOperationException("bad dispose")));
        scope.Add(new TestSubscription(() => calls.Add("third")));

        scope.Dispose();

        Assert.Equal(["third", "first"], calls);
        Assert.Equal(StateScopeState.Disposed, scope.State);

        var record = Assert.Single(diagnostics.Records);
        Assert.Equal("AUCSTA009", record.Code);
        Assert.Equal(HostDiagnosticSeverity.Error, record.Severity);
        Assert.Contains(scope.Id, record.Message, StringComparison.Ordinal);
        Assert.Contains("bad dispose", record.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StateScopeRecordsDisposeFailuresForSubscriptionsAddedAfterDisposal()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var scope = new StateScope("activation", diagnostics);
        scope.Dispose();

        scope.Add(new TestSubscription(() => throw new InvalidOperationException("late dispose")));

        Assert.Equal(StateScopeState.Disposed, scope.State);
        var record = Assert.Single(diagnostics.Records);
        Assert.Equal("AUCSTA009", record.Code);
        Assert.Equal(HostDiagnosticSeverity.Error, record.Severity);
        Assert.Contains(scope.Id, record.Message, StringComparison.Ordinal);
        Assert.Contains("late dispose", record.Message, StringComparison.Ordinal);
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
