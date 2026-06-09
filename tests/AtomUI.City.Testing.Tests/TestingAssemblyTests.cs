using System.Reflection;

namespace AtomUI.City.Testing.Tests;

public sealed class TestingAssemblyTests
{
    [Fact]
    public void TestingAssemblyCanBeLoaded()
    {
        var assembly = Assembly.Load("AtomUI.City.Testing");

        Assert.Equal("AtomUI.City.Testing", assembly.GetName().Name);
    }
}
