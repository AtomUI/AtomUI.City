namespace AtomUI.City.Lifecycle;

public enum LifecycleScopeState
{
    Created,
    Starting,
    Running,
    CancelRequested,
    Stopping,
    Stopped,
    Faulted,
    UnloadPending,
    Disposing,
    Disposed,
}
