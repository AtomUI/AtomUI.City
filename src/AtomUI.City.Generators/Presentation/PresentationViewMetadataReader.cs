using Microsoft.CodeAnalysis;

namespace AtomUI.City.Generators.Presentation;

public static class PresentationViewMetadataReader
{
    private const string ViewForAttributeName = "AtomUI.City.Presentation.ViewForAttribute";

    public static IReadOnlyList<PresentationViewMetadata> Read(INamedTypeSymbol type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var viewTypeName = GetTypeName(type);

        return type
            .GetAttributes()
            .Where(attribute => string.Equals(GetAttributeTypeName(attribute), ViewForAttributeName, StringComparison.Ordinal))
            .Select(attribute => ReadView(viewTypeName, attribute))
            .Where(view => view is not null)
            .Cast<PresentationViewMetadata>()
            .ToArray();
    }

    private static PresentationViewMetadata? ReadView(
        string viewTypeName,
        AttributeData attribute)
    {
        var viewModelTypeName = ReadConstructorTypeName(attribute, 0);
        if (string.IsNullOrWhiteSpace(viewModelTypeName))
        {
            return null;
        }

        return new PresentationViewMetadata(
            viewTypeName,
            viewModelTypeName!,
            ReadNamedString(attribute, "Key"),
            ReadNamedString(attribute, "ContributionId"));
    }

    private static string? ReadConstructorTypeName(AttributeData attribute, int index)
    {
        if (attribute.ConstructorArguments.Length <= index)
        {
            return null;
        }

        return attribute.ConstructorArguments[index].Value is INamedTypeSymbol type ? GetTypeName(type) : null;
    }

    private static string? ReadNamedString(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal))
            {
                return argument.Value.Value as string;
            }
        }

        return null;
    }

    private static string? GetAttributeTypeName(AttributeData attribute)
    {
        return attribute.AttributeClass is null ? null : GetTypeName(attribute.AttributeClass);
    }

    private static string GetTypeName(INamedTypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    }
}
