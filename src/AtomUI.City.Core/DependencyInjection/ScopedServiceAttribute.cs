using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ScopedServiceAttribute : Attribute
{
    private readonly Type[] _serviceTypes;

    public ScopedServiceAttribute(params Type[] serviceTypes)
    {
        _serviceTypes = (serviceTypes ?? throw new ArgumentNullException(nameof(serviceTypes))).ToArray();
    }

    public ServiceLifetime Lifetime => ServiceLifetime.Scoped;

    public Type[] ServiceTypes => _serviceTypes.ToArray();

    public bool Replace { get; set; }

    public bool TryAdd { get; set; }

    public string? Key { get; set; }
}
