using System.Reflection;

namespace AtomUI.City.EventBus.Tests;

public sealed class EventBusAssemblyTests
{
    [Fact]
    public void EventBusAssemblyCanBeLoaded()
    {
        var assembly = Assembly.Load("AtomUI.City.EventBus");

        Assert.Equal("AtomUI.City.EventBus", assembly.GetName().Name);
    }
}
