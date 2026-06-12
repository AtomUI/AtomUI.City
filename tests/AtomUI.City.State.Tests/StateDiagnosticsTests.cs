using AtomUI.City.Diagnostics;
using AtomUI.City.State;

namespace AtomUI.City.State.Tests;

public sealed class StateDiagnosticsTests
{
    [Fact]
    public void WritableStateRecordsChangedEventHandlerFailuresAndContinuesNotification()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var state = new WritableState<int>(0, diagnostics: diagnostics);
        var secondHandlerCalled = false;

        state.Changed += (_, _) => throw new InvalidOperationException("bad changed event");
        state.Changed += (_, _) => secondHandlerCalled = true;

        state.SetValue(1);

        Assert.True(secondHandlerCalled);
        var record = Assert.Single(diagnostics.Records);
        Assert.Equal(StateDiagnosticIds.ChangedEventHandlerFailed, record.Code);
        Assert.Equal(HostDiagnosticSeverity.Error, record.Severity);
        Assert.Contains("bad changed event", record.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WritableStateRecordsSubscriptionHandlerFailuresAndContinuesNotification()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var state = new WritableState<int>(0, diagnostics: diagnostics);
        var observed = 0;

        state.OnChange(_ => throw new InvalidOperationException("bad subscription"));
        state.OnChange(args => observed = args.NewValue);

        state.SetValue(5);

        Assert.Equal(5, observed);
        var record = Assert.Single(diagnostics.Records);
        Assert.Equal(StateDiagnosticIds.SubscriptionHandlerFailed, record.Code);
        Assert.Equal(HostDiagnosticSeverity.Error, record.Severity);
        Assert.Contains("bad subscription", record.Message, StringComparison.Ordinal);
    }
}
