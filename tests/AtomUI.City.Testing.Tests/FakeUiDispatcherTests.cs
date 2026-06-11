using AtomUI.City.Testing;

namespace AtomUI.City.Testing.Tests;

public sealed class FakeUiDispatcherTests
{
    [Fact]
    public void PostQueuesWorkUntilDrainIsCalled()
    {
        var dispatcher = new FakeUiDispatcher();
        var calls = new List<string>();

        dispatcher.Post(() => calls.Add("first"));
        dispatcher.Post(() => calls.Add("second"));

        Assert.Empty(calls);
        Assert.Equal(2, dispatcher.PendingCount);

        dispatcher.Drain();

        Assert.Equal(["first", "second"], calls);
        Assert.Equal(0, dispatcher.PendingCount);
    }

    [Fact]
    public void CanceledWorkItemDoesNotRunDuringDrain()
    {
        var dispatcher = new FakeUiDispatcher();
        var wasCalled = false;

        var workItem = dispatcher.Post(() => wasCalled = true);
        workItem.Cancel();
        dispatcher.Drain();

        Assert.False(wasCalled);
        Assert.True(workItem.IsCanceled);
    }
}
