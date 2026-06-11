namespace AtomUI.City.Testing;

public sealed class FakeUiWorkItem
{
    private readonly Action _callback;

    internal FakeUiWorkItem(Action callback)
    {
        _callback = callback;
    }

    public bool IsCanceled { get; private set; }

    public bool IsCompleted { get; private set; }

    public void Cancel()
    {
        if (IsCompleted)
        {
            return;
        }

        IsCanceled = true;
    }

    internal void Execute()
    {
        if (IsCanceled || IsCompleted)
        {
            return;
        }

        _callback();
        IsCompleted = true;
    }
}
