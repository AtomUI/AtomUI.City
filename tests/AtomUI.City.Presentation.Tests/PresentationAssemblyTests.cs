using System.Reflection;

namespace AtomUI.City.Presentation.Tests;

public sealed class PresentationAssemblyTests
{
    [Fact]
    public void PresentationAssemblyCanBeLoaded()
    {
        var assembly = Assembly.Load("AtomUI.City.Presentation");

        Assert.Equal("AtomUI.City.Presentation", assembly.GetName().Name);
    }
}
