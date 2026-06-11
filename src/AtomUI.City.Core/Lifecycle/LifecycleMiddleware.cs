namespace AtomUI.City.Lifecycle;

public delegate ValueTask LifecycleMiddleware(LifecycleContext context, LifecycleNext next);
