namespace AtomUI.City.Testing;

public sealed class DeterministicScheduler
{
    private readonly PriorityQueue<ScheduledWorkItem, DateTimeOffset> _scheduledWork = new();

    public DateTimeOffset Now { get; private set; } = DateTimeOffset.UnixEpoch;

    public int ScheduledCount => _scheduledWork.Count;

    public void Schedule(TimeSpan delay, Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (delay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), delay, "Delay cannot be negative.");
        }

        var dueAt = Now.Add(delay);
        _scheduledWork.Enqueue(new ScheduledWorkItem(callback, dueAt), dueAt);
    }

    public void AdvanceBy(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration cannot be negative.");
        }

        Now = Now.Add(duration);
        RunDueWork();
    }

    public void RunDueWork()
    {
        while (_scheduledWork.TryPeek(out var workItem, out var dueAt) && dueAt <= Now)
        {
            _scheduledWork.Dequeue();
            workItem.Callback();
        }
    }

    private sealed record ScheduledWorkItem(Action Callback, DateTimeOffset DueAt);
}
