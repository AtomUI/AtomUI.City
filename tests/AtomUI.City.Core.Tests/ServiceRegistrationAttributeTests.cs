using AtomUI.City.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Core.Tests;

public sealed class ServiceRegistrationAttributeTests
{
    [Fact]
    public void ServiceAttributeStoresLifetimeAndRegistrationOptions()
    {
        var attribute = new ServiceAttribute(ServiceLifetime.Singleton)
        {
            Replace = true,
            TryAdd = true,
            Key = "system-clock",
        };

        Assert.Equal(ServiceLifetime.Singleton, attribute.Lifetime);
        Assert.True(attribute.Replace);
        Assert.True(attribute.TryAdd);
        Assert.Equal("system-clock", attribute.Key);
    }

    [Fact]
    public void ScopedServiceAttributeStoresExposedServiceTypes()
    {
        var attribute = new ScopedServiceAttribute(typeof(IClock), typeof(ISystemClock));

        Assert.Equal(ServiceLifetime.Scoped, attribute.Lifetime);
        Assert.Equal([typeof(IClock), typeof(ISystemClock)], attribute.ServiceTypes);
    }

    [Fact]
    public void ExposeServicesAttributeStoresExposedServiceTypes()
    {
        var attribute = new ExposeServicesAttribute(typeof(IClock));

        Assert.Equal([typeof(IClock)], attribute.ServiceTypes);
    }

    [Fact]
    public void DependencyMarkerInterfacesAreEmptyContracts()
    {
        Assert.True(typeof(ISingletonDependency).IsInterface);
        Assert.Empty(typeof(ISingletonDependency).GetInterfaces());
        Assert.True(typeof(IScopedDependency).IsInterface);
        Assert.Empty(typeof(IScopedDependency).GetInterfaces());
        Assert.True(typeof(ITransientDependency).IsInterface);
        Assert.Empty(typeof(ITransientDependency).GetInterfaces());
    }

    private interface IClock;

    private interface ISystemClock;
}
