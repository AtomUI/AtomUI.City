using System.Reflection;

namespace AtomUI.City.PluginSystem.Tests;

public sealed class PluginSystemAssemblyTests
{
    [Fact]
    public void PluginSystemAssemblyCanBeLoaded()
    {
        var assembly = Assembly.Load("AtomUI.City.PluginSystem");

        Assert.Equal("AtomUI.City.PluginSystem", assembly.GetName().Name);
    }
}
