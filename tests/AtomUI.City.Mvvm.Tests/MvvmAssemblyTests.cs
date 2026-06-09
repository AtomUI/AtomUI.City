using System.Reflection;

namespace AtomUI.City.Mvvm.Tests;

public sealed class MvvmAssemblyTests
{
    [Fact]
    public void MvvmAssemblyCanBeLoaded()
    {
        var assembly = Assembly.Load("AtomUI.City.Mvvm");

        Assert.Equal("AtomUI.City.Mvvm", assembly.GetName().Name);
    }
}
