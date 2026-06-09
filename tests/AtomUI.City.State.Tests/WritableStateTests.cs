using AtomUI.City.State;

namespace AtomUI.City.State.Tests;

public sealed class WritableStateTests
{
    [Fact]
    public void SetUpdatesValueAndRaisesChangedOnce()
    {
        var state = new WritableState<int>(1);
        var changeCount = 0;
        StateChangedEventArgs<int>? lastChange = null;

        state.Changed += (_, args) =>
        {
            changeCount++;
            lastChange = args;
        };

        state.Set(2);
        state.Set(2);

        Assert.Equal(2, state.Value);
        Assert.Equal(1, changeCount);
        Assert.NotNull(lastChange);
        Assert.Equal(1, lastChange.OldValue);
        Assert.Equal(2, lastChange.NewValue);
    }

    [Fact]
    public void UpdateTransformsCurrentValue()
    {
        var state = new WritableState<int>(2);

        state.Update(value => value + 3);

        Assert.Equal(5, state.Value);
    }
}
