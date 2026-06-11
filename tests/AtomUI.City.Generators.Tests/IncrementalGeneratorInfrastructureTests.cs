using AtomUI.City.Generators;

namespace AtomUI.City.Generators.Tests;

public sealed class IncrementalGeneratorInfrastructureTests
{
    [Fact]
    public void BootstrapperUsesIncrementalGeneratorContract()
    {
        var generatorType = typeof(AtomUICityIncrementalGenerator);

        Assert.Contains(
            generatorType.GetInterfaces(),
            contract => string.Equals(contract.FullName, "Microsoft.CodeAnalysis.IIncrementalGenerator", StringComparison.Ordinal));
    }

    [Fact]
    public void BootstrapperDeclaresRoslynGeneratorAttribute()
    {
        var generatorType = typeof(AtomUICityIncrementalGenerator);

        Assert.Contains(
            generatorType.GetCustomAttributesData(),
            attribute => string.Equals(attribute.AttributeType.FullName, "Microsoft.CodeAnalysis.GeneratorAttribute", StringComparison.Ordinal));
    }
}
