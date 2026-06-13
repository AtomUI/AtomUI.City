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
        var constructorParameters = ReadConstructorParameters(type);

        return Array.AsReadOnly(
            type
                .GetAttributes()
                .Where(attribute => string.Equals(GetAttributeTypeName(attribute), ViewForAttributeName, StringComparison.Ordinal))
                .Select(attribute => ReadView(viewTypeName, constructorParameters, attribute))
                .Where(view => view is not null)
                .Cast<PresentationViewMetadata>()
                .ToArray());
    }

    private static PresentationViewMetadata? ReadView(
        string viewTypeName,
        IReadOnlyList<PresentationViewConstructorParameter> constructorParameters,
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
            ReadNamedString(attribute, "PluginId"),
            ReadNamedString(attribute, "ContributionId"),
            attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation(),
            constructorParameters);
    }

    private static IReadOnlyList<PresentationViewConstructorParameter> ReadConstructorParameters(INamedTypeSymbol type)
    {
        var constructor = type.InstanceConstructors
            .Where(candidate => candidate.DeclaredAccessibility == Accessibility.Public)
            .OrderByDescending(candidate => candidate.Parameters.Length)
            .FirstOrDefault();

        if (constructor is null || constructor.Parameters.Length == 0)
        {
            return [];
        }

        return constructor.Parameters
            .Select(parameter => new PresentationViewConstructorParameter(GetTypeName(parameter.Type)))
            .ToArray();
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

    private static string GetTypeName(ITypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    }
}
