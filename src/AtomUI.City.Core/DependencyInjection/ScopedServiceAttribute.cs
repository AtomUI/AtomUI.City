using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ScopedServiceAttribute : Attribute
{
    public ScopedServiceAttribute(params Type[] serviceTypes)
    {
        ServiceTypes = serviceTypes ?? throw new ArgumentNullException(nameof(serviceTypes));
    }

    public ServiceLifetime Lifetime => ServiceLifetime.Scoped;

    public Type[] ServiceTypes { get; }

    public bool Replace { get; set; }

    public bool TryAdd { get; set; }

    public string? Key { get; set; }
}
