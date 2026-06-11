namespace AtomUI.City.Modularity;

public abstract class ModuleBase : IModule
{
    public virtual ValueTask PreConfigureServicesAsync(
        ServiceConfigurationContext context,
        CancellationToken cancellationToken = default)
    {
        PreConfigureServices(context);

        return ValueTask.CompletedTask;
    }

    public virtual void PreConfigureServices(ServiceConfigurationContext context)
    {
    }

    public virtual ValueTask ConfigureServicesAsync(
        ServiceConfigurationContext context,
        CancellationToken cancellationToken = default)
    {
        ConfigureServices(context);

        return ValueTask.CompletedTask;
    }

    public virtual void ConfigureServices(ServiceConfigurationContext context)
    {
    }

    public virtual ValueTask PostConfigureServicesAsync(
        ServiceConfigurationContext context,
        CancellationToken cancellationToken = default)
    {
        PostConfigureServices(context);

        return ValueTask.CompletedTask;
    }

    public virtual void PostConfigureServices(ServiceConfigurationContext context)
    {
    }

    public virtual ValueTask ConfigureContributionsAsync(
        ContributionConfigurationContext context,
        CancellationToken cancellationToken = default)
    {
        ConfigureContributions(context);

        return ValueTask.CompletedTask;
    }

    public virtual void ConfigureContributions(ContributionConfigurationContext context)
    {
    }

    public virtual ValueTask OnPreApplicationInitializationAsync(
        ApplicationInitializationContext context,
        CancellationToken cancellationToken = default)
    {
        OnPreApplicationInitialization(context);

        return ValueTask.CompletedTask;
    }

    public virtual void OnPreApplicationInitialization(ApplicationInitializationContext context)
    {
    }

    public virtual ValueTask OnApplicationInitializationAsync(
        ApplicationInitializationContext context,
        CancellationToken cancellationToken = default)
    {
        OnApplicationInitialization(context);

        return ValueTask.CompletedTask;
    }

    public virtual void OnApplicationInitialization(ApplicationInitializationContext context)
    {
    }

    public virtual ValueTask OnPostApplicationInitializationAsync(
        ApplicationInitializationContext context,
        CancellationToken cancellationToken = default)
    {
        OnPostApplicationInitialization(context);

        return ValueTask.CompletedTask;
    }

    public virtual void OnPostApplicationInitialization(ApplicationInitializationContext context)
    {
    }

    public virtual ValueTask OnApplicationShutdownAsync(
        ApplicationShutdownContext context,
        CancellationToken cancellationToken = default)
    {
        OnApplicationShutdown(context);

        return ValueTask.CompletedTask;
    }

    public virtual void OnApplicationShutdown(ApplicationShutdownContext context)
    {
    }
}
