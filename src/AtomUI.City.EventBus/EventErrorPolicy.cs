namespace AtomUI.City.EventBus;

public enum EventErrorPolicy
{
    ContinueAndReport,
    StopPublication,
    FailPublisher,
}
