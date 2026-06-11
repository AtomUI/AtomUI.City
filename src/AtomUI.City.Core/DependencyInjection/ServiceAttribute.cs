using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ServiceAttribute : Attribute
{
    public ServiceAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }

    public ServiceLifetime Lifetime { get; }

    public bool Replace { get; set; }

    public bool TryAdd { get; set; }

    public string? Key { get; set; }
}
