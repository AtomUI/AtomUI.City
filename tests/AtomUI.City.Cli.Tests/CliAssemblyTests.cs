using System.Reflection;

namespace AtomUI.City.Cli.Tests;

public sealed class CliAssemblyTests
{
    [Fact]
    public void CliAssemblyCanBeLoaded()
    {
        var assembly = Assembly.Load("AtomUI.City.Cli");

        Assert.Equal("AtomUI.City.Cli", assembly.GetName().Name);
    }
}
