using AtomUI.City.Diagnostics;
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

    [Fact]
    public void SetValueReturnsFalseForEqualValueAndDoesNotNotify()
    {
        var state = new WritableState<string>("ready");
        var changeCount = 0;

        state.Changed += (_, _) => changeCount++;

        var changed = state.SetValue("ready");

        Assert.False(changed);
        Assert.Equal(0, state.Version);
        Assert.Equal(0, changeCount);
    }

    [Fact]
    public void UpdateKeepsCurrentValueWhenUpdaterThrows()
    {
        var state = new WritableState<int>(3);

        Assert.Throws<InvalidOperationException>(
            () => state.Update(_ => throw new InvalidOperationException("bad update")));

        Assert.Equal(3, state.Value);
        Assert.Equal(0, state.Version);
    }

    [Fact]
    public void UpdateRecordsUpdaterFailureDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var state = new WritableState<int>(3, diagnostics: diagnostics);

        Assert.Throws<InvalidOperationException>(
            () => state.Update(_ => throw new InvalidOperationException("bad update")));

        Assert.Equal(3, state.Value);
        Assert.Equal(0, state.Version);
        var record = Assert.Single(diagnostics.Records);
        Assert.Equal(StateDiagnosticIds.WritableStateUpdateFailed, record.Code);
        Assert.Equal(HostDiagnosticSeverity.Error, record.Severity);
        Assert.Contains("bad update", record.Message, StringComparison.Ordinal);
    }
}
