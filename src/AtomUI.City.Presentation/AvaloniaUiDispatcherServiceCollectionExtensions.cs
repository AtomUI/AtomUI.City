using AtomUI.City.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AtomUI.City.Presentation;

public static class AvaloniaUiDispatcherServiceCollectionExtensions
{
    public static IServiceCollection AddAvaloniaUiDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IUiDispatcher, AvaloniaUiDispatcher>();

        return services;
    }
}
