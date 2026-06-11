using AtomUI.City.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Modularity;

public sealed class ModuleRegistry : IModuleRegistry
{
    private readonly IReadOnlyList<ModuleEntry> _orderedEntries;
    private bool _contributionsConfigured;
    private bool _initialized;
    private bool _servicesConfigured;
    private bool _shutdown;

    private ModuleRegistry(IReadOnlyList<ModuleEntry> orderedEntries)
    {
        _orderedEntries = orderedEntries;
        Modules = orderedEntries.Select(entry => entry.Descriptor).ToArray();
    }

    public IReadOnlyList<ModuleDescriptor> Modules { get; }

    internal static ModuleRegistry Create(IReadOnlyList<ModuleRegistration> registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);

        var entries = registrations
            .Select(registration => new ModuleEntry(
                CreateDescriptor(registration.ModuleType),
                registration.Factory()))
            .ToArray();

        return new ModuleRegistry(OrderByDependencies(entries));
    }

    public async ValueTask ConfigureServicesAsync(
        ApplicationContext applicationContext,
        IServiceCollection services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(applicationContext);
        ArgumentNullException.ThrowIfNull(services);

        if (_servicesConfigured)
        {
            return;
        }

        var context = new ServiceConfigurationContext(applicationContext, services);

        foreach (var entry in _orderedEntries)
        {
            await entry.Module.PreConfigureServicesAsync(context, cancellationToken).ConfigureAwait(false);
        }

        foreach (var entry in _orderedEntries)
        {
            await entry.Module.ConfigureServicesAsync(context, cancellationToken).ConfigureAwait(false);
        }

        foreach (var entry in _orderedEntries)
        {
            await entry.Module.PostConfigureServicesAsync(context, cancellationToken).ConfigureAwait(false);
        }

        _servicesConfigured = true;
    }

    public async ValueTask ConfigureContributionsAsync(
        ApplicationContext applicationContext,
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(applicationContext);
        ArgumentNullException.ThrowIfNull(services);

        if (_contributionsConfigured)
        {
            return;
        }

        var context = new ContributionConfigurationContext(applicationContext, services);

        foreach (var entry in _orderedEntries)
        {
            await entry.Module.ConfigureContributionsAsync(context, cancellationToken).ConfigureAwait(false);
        }

        _contributionsConfigured = true;
    }

    public async ValueTask InitializeAsync(
        ApplicationContext applicationContext,
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(applicationContext);
        ArgumentNullException.ThrowIfNull(services);

        if (_initialized)
        {
            return;
        }

        var context = new ApplicationInitializationContext(applicationContext, services);

        foreach (var entry in _orderedEntries)
        {
            await entry.Module.OnPreApplicationInitializationAsync(context, cancellationToken).ConfigureAwait(false);
        }

        foreach (var entry in _orderedEntries)
        {
            await entry.Module.OnApplicationInitializationAsync(context, cancellationToken).ConfigureAwait(false);
        }

        foreach (var entry in _orderedEntries)
        {
            await entry.Module.OnPostApplicationInitializationAsync(context, cancellationToken).ConfigureAwait(false);
        }

        _initialized = true;
    }

    public async ValueTask ShutdownAsync(
        ApplicationContext applicationContext,
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(applicationContext);
        ArgumentNullException.ThrowIfNull(services);

        if (_shutdown || !_initialized)
        {
            return;
        }

        _shutdown = true;
        var context = new ApplicationShutdownContext(applicationContext, services);

        for (var index = _orderedEntries.Count - 1; index >= 0; index--)
        {
            await _orderedEntries[index].Module.OnApplicationShutdownAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }

    private static ModuleDescriptor CreateDescriptor(Type moduleType)
    {
        var attribute = moduleType
            .GetCustomAttributes(typeof(ModuleAttribute), inherit: false)
            .OfType<ModuleAttribute>()
            .SingleOrDefault();
        var dependencies = moduleType
            .GetCustomAttributes(typeof(DependsOnAttribute), inherit: false)
            .OfType<DependsOnAttribute>()
            .Select(attribute => new ModuleDependencyDescriptor(attribute.ModuleType, attribute.Optional))
            .ToArray();

        return new ModuleDescriptor(
            attribute?.Name ?? moduleType.FullName ?? moduleType.Name,
            moduleType,
            attribute?.Version,
            attribute?.Description,
            dependencies);
    }

    private static IReadOnlyList<ModuleEntry> OrderByDependencies(IReadOnlyList<ModuleEntry> entries)
    {
        var entriesByType = entries.ToDictionary(entry => entry.Descriptor.ModuleType);
        var ordered = new List<ModuleEntry>();
        var visitStates = new Dictionary<Type, ModuleVisitState>();

        foreach (var entry in entries)
        {
            Visit(entry, entriesByType, visitStates, ordered);
        }

        return ordered;
    }

    private static void Visit(
        ModuleEntry entry,
        IReadOnlyDictionary<Type, ModuleEntry> entriesByType,
        IDictionary<Type, ModuleVisitState> visitStates,
        ICollection<ModuleEntry> ordered)
    {
        if (visitStates.TryGetValue(entry.Descriptor.ModuleType, out var state))
        {
            if (state == ModuleVisitState.Visited)
            {
                return;
            }

            throw new InvalidOperationException(
                $"Module dependency graph contains a cycle at '{entry.Descriptor.ModuleType.FullName}'.");
        }

        visitStates.Add(entry.Descriptor.ModuleType, ModuleVisitState.Visiting);

        foreach (var dependency in entry.Descriptor.Dependencies)
        {
            if (entriesByType.TryGetValue(dependency.ModuleType, out var dependencyEntry))
            {
                Visit(dependencyEntry, entriesByType, visitStates, ordered);
                continue;
            }

            if (!dependency.Optional)
            {
                throw new InvalidOperationException(
                    $"Module '{entry.Descriptor.ModuleType.FullName}' depends on missing module '{dependency.ModuleType.FullName}'.");
            }
        }

        visitStates[entry.Descriptor.ModuleType] = ModuleVisitState.Visited;
        ordered.Add(entry);
    }

    private sealed record ModuleEntry(ModuleDescriptor Descriptor, IModule Module);

    private enum ModuleVisitState
    {
        Visiting,
        Visited,
    }
}
