using AtomUI.City.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Modularity;

public interface IModuleRegistry
{
    IReadOnlyList<ModuleDescriptor> Modules { get; }

    ValueTask ConfigureServicesAsync(
        ApplicationContext applicationContext,
        IServiceCollection services,
        CancellationToken cancellationToken = default);

    ValueTask ConfigureContributionsAsync(
        ApplicationContext applicationContext,
        IServiceProvider services,
        CancellationToken cancellationToken = default);

    ValueTask InitializeAsync(
        ApplicationContext applicationContext,
        IServiceProvider services,
        CancellationToken cancellationToken = default);

    ValueTask ShutdownAsync(
        ApplicationContext applicationContext,
        IServiceProvider services,
        CancellationToken cancellationToken = default);
}
