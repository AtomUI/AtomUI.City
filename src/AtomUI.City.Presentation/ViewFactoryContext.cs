namespace AtomUI.City.Presentation;

public sealed class ViewFactoryContext
{
    public ViewFactoryContext(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        Services = services;
    }

    public IServiceProvider Services { get; }
}
