namespace AtomUI.City.Generators.DependencyInjection;

public sealed class ServiceRegistrationMetadata
{
    public ServiceRegistrationMetadata(
        string implementationTypeName,
        ServiceRegistrationLifetime lifetime,
        IReadOnlyList<string> exposedServiceTypeNames,
        bool replace,
        bool tryAdd,
        string? key)
    {
        if (string.IsNullOrWhiteSpace(implementationTypeName))
        {
            throw new ArgumentException("Implementation type name cannot be empty.", nameof(implementationTypeName));
        }

        ImplementationTypeName = implementationTypeName;
        Lifetime = lifetime;
        ExposedServiceTypeNames = Array.AsReadOnly((exposedServiceTypeNames ?? throw new ArgumentNullException(nameof(exposedServiceTypeNames))).ToArray());
        Replace = replace;
        TryAdd = tryAdd;
        Key = key;
    }

    public string ImplementationTypeName { get; }

    public ServiceRegistrationLifetime Lifetime { get; }

    public IReadOnlyList<string> ExposedServiceTypeNames { get; }

    public bool Replace { get; }

    public bool TryAdd { get; }

    public string? Key { get; }
}
