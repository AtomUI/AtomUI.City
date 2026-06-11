namespace AtomUI.City.Modularity;

public interface IModule
{
    ValueTask PreConfigureServicesAsync(
        ServiceConfigurationContext context,
        CancellationToken cancellationToken = default);

    ValueTask ConfigureServicesAsync(
        ServiceConfigurationContext context,
        CancellationToken cancellationToken = default);

    ValueTask PostConfigureServicesAsync(
        ServiceConfigurationContext context,
        CancellationToken cancellationToken = default);

    ValueTask ConfigureContributionsAsync(
        ContributionConfigurationContext context,
        CancellationToken cancellationToken = default);

    ValueTask OnPreApplicationInitializationAsync(
        ApplicationInitializationContext context,
        CancellationToken cancellationToken = default);

    ValueTask OnApplicationInitializationAsync(
        ApplicationInitializationContext context,
        CancellationToken cancellationToken = default);

    ValueTask OnPostApplicationInitializationAsync(
        ApplicationInitializationContext context,
        CancellationToken cancellationToken = default);

    ValueTask OnApplicationShutdownAsync(
        ApplicationShutdownContext context,
        CancellationToken cancellationToken = default);
}
