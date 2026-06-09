using System.Reflection;

namespace AtomUI.City.Core.Tests;

public sealed class CoreAssemblyTests
{
    [Fact]
    public void CoreAssemblyCanBeLoaded()
    {
        var assembly = Assembly.Load("AtomUI.City.Core");

        Assert.Equal("AtomUI.City.Core", assembly.GetName().Name);
    }
}
