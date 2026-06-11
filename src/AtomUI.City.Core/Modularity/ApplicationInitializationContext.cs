using AtomUI.City.Hosting;

namespace AtomUI.City.Modularity;

public sealed class ApplicationInitializationContext
{
    public ApplicationInitializationContext(
        ApplicationContext applicationContext,
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(applicationContext);
        ArgumentNullException.ThrowIfNull(services);

        ApplicationContext = applicationContext;
        Services = services;
    }

    public ApplicationContext ApplicationContext { get; }

    public IServiceProvider Services { get; }
}
