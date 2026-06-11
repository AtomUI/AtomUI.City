namespace AtomUI.City.Presentation;

public sealed record VisualLifecycleEvent(
    object View,
    VisualLifecycleEventKind Kind);
