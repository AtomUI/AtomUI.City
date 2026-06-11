namespace AtomUI.City.Testing;

public sealed class FakeUiDispatcher
{
    private readonly Queue<FakeUiWorkItem> _workItems = new();

    public int PendingCount => _workItems.Count;

    public FakeUiWorkItem Post(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var workItem = new FakeUiWorkItem(callback);

        _workItems.Enqueue(workItem);

        return workItem;
    }

    public void Drain()
    {
        while (_workItems.TryDequeue(out var workItem))
        {
            workItem.Execute();
        }
    }
}
