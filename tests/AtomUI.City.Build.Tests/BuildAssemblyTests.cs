using System.Reflection;

namespace AtomUI.City.Build.Tests;

public sealed class BuildAssemblyTests
{
    [Fact]
    public void BuildAssemblyCanBeLoaded()
    {
        var assembly = Assembly.Load("AtomUI.City.Build");

        Assert.Equal("AtomUI.City.Build", assembly.GetName().Name);
    }
}
