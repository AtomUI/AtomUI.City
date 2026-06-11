using AtomUI.City.State;

namespace AtomUI.City.State.Tests;

public sealed class ComputedStateTests
{
    [Fact]
    public void ComputedStateCachesValueUntilDependencyChanges()
    {
        var source = new WritableState<int>(1);
        var computeCount = 0;
        var computed = new ComputedState<int>(
            () =>
            {
                computeCount++;
                return source.Value * 2;
            },
            source);

        Assert.Equal(2, computed.Value);
        Assert.Equal(2, computed.Value);
        Assert.Equal(1, computeCount);

        source.SetValue(2);

        Assert.Equal(4, computed.Value);
        Assert.Equal(2, computeCount);
    }

    [Fact]
    public void ComputedStateNotifiesWhenComputedValueChanges()
    {
        var source = new WritableState<int>(1);
        var computed = new ComputedState<int>(() => source.Value * 2, source);
        var changes = new List<StateChangedEventArgs<int>>();

        computed.OnChange(changes.Add);

        source.SetValue(2);

        Assert.Single(changes);
        Assert.Equal(2, changes[0].OldValue);
        Assert.Equal(4, changes[0].NewValue);
        Assert.Equal(1, computed.Version);
    }

    [Fact]
    public void ComputedStateKeepsLastValueWhenComputeFails()
    {
        var source = new WritableState<int>(1);
        var computed = new ComputedState<int>(
            () => source.Value == 2 ? throw new InvalidOperationException("bad value") : source.Value,
            source);

        Assert.Equal(1, computed.Value);

        source.SetValue(2);

        Assert.Equal(1, computed.Value);
        Assert.NotNull(computed.LastError);
    }
}
