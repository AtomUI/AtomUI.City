using Microsoft.CodeAnalysis;

namespace AtomUI.City.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class AtomUICityIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
    }
}
