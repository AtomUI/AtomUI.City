using System.Reflection;

namespace AtomUI.City.Routing.Tests;

public sealed class RoutingAssemblyTests
{
    [Fact]
    public void RoutingAssemblyCanBeLoaded()
    {
        var assembly = Assembly.Load("AtomUI.City.Routing");

        Assert.Equal("AtomUI.City.Routing", assembly.GetName().Name);
    }
}
