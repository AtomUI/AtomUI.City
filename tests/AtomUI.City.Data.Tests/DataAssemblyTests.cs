using System.Reflection;

namespace AtomUI.City.Data.Tests;

public sealed class DataAssemblyTests
{
    [Fact]
    public void DataAssemblyCanBeLoaded()
    {
        var assembly = Assembly.Load("AtomUI.City.Data");

        Assert.Equal("AtomUI.City.Data", assembly.GetName().Name);
    }
}
