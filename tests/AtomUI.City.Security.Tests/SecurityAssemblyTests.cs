using System.Reflection;

namespace AtomUI.City.Security.Tests;

public sealed class SecurityAssemblyTests
{
    [Fact]
    public void SecurityAssemblyCanBeLoaded()
    {
        var assembly = Assembly.Load("AtomUI.City.Security");

        Assert.Equal("AtomUI.City.Security", assembly.GetName().Name);
    }
}
