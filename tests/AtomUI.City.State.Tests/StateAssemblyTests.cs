using System.Reflection;

namespace AtomUI.City.State.Tests;

public sealed class StateAssemblyTests
{
    [Fact]
    public void StateAssemblyCanBeLoaded()
    {
        var assembly = Assembly.Load("AtomUI.City.State");

        Assert.Equal("AtomUI.City.State", assembly.GetName().Name);
    }
}
