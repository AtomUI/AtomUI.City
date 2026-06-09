using System.Reflection;

namespace AtomUI.City.Localization.Tests;

public sealed class LocalizationAssemblyTests
{
    [Fact]
    public void LocalizationAssemblyCanBeLoaded()
    {
        var assembly = Assembly.Load("AtomUI.City.Localization");

        Assert.Equal("AtomUI.City.Localization", assembly.GetName().Name);
    }
}
