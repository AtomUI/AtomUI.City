using Microsoft.CodeAnalysis;

namespace AtomUI.City.Generators.Localization;

public static class LocalizationMetadataReader
{
    private const string LanguagePackageAttributeName = "AtomUI.City.Localization.LanguagePackageAttribute";
    private const string LocalizedResourceAttributeName = "AtomUI.City.Localization.LocalizedResourceAttribute";

    public static LocalizationMetadata Read(Compilation compilation)
    {
        if (compilation is null)
        {
            throw new ArgumentNullException(nameof(compilation));
        }

        var attributes = compilation.Assembly.GetAttributes();
        var packages = attributes
            .Where(attribute => string.Equals(GetAttributeTypeName(attribute), LanguagePackageAttributeName, StringComparison.Ordinal))
            .Select(ReadLanguagePackage)
            .Where(package => package is not null)
            .Cast<LanguagePackageMetadata>()
            .ToArray();
        var resources = attributes
            .Where(attribute => string.Equals(GetAttributeTypeName(attribute), LocalizedResourceAttributeName, StringComparison.Ordinal))
            .Select(ReadLocalizedResource)
            .Where(resource => resource is not null)
            .Cast<LocalizedResourceMetadata>()
            .ToArray();

        return new LocalizationMetadata(packages, resources);
    }

    private static LanguagePackageMetadata? ReadLanguagePackage(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length < 2)
        {
            return null;
        }

        var packageId = attribute.ConstructorArguments[0].Value as string;
        var culture = attribute.ConstructorArguments[1].Value as string;

        if (string.IsNullOrWhiteSpace(packageId) || string.IsNullOrWhiteSpace(culture))
        {
            return null;
        }

        return new LanguagePackageMetadata(
            packageId!,
            culture!,
            ReadScope(attribute),
            ReadNamedString(attribute, "ResourceBaseName"),
            ReadNamedString(attribute, "FallbackCulture"),
            ReadNamedString(attribute, "Version"),
            ReadNamedString(attribute, "Checksum"));
    }

    private static LocalizedResourceMetadata? ReadLocalizedResource(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length < 2)
        {
            return null;
        }

        var key = attribute.ConstructorArguments[0].Value as string;
        var packageId = attribute.ConstructorArguments[1].Value as string;

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(packageId))
        {
            return null;
        }

        return new LocalizedResourceMetadata(
            key!,
            packageId!,
            ReadKind(attribute),
            ReadScope(attribute),
            ReadNamedString(attribute, "Version"),
            ReadNamedBoolean(attribute, "Critical"));
    }

    private static LocalizedResourceMetadataKind ReadKind(AttributeData attribute)
    {
        var value = ReadNamedEnumValue(attribute, "Kind");

        return value.HasValue && Enum.IsDefined(typeof(LocalizedResourceMetadataKind), value.Value)
            ? (LocalizedResourceMetadataKind)value.Value
            : LocalizedResourceMetadataKind.String;
    }

    private static ResourceScopeMetadata ReadScope(AttributeData attribute)
    {
        var value = ReadNamedEnumValue(attribute, "Scope");

        return value.HasValue && Enum.IsDefined(typeof(ResourceScopeMetadata), value.Value)
            ? (ResourceScopeMetadata)value.Value
            : ResourceScopeMetadata.Module;
    }

    private static int? ReadNamedEnumValue(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal) &&
                argument.Value.Value is int value)
            {
                return value;
            }
        }

        return null;
    }

    private static bool ReadNamedBoolean(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal))
            {
                return argument.Value.Value is true;
            }
        }

        return false;
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
        return attribute.AttributeClass is null
            ? null
            : attribute.AttributeClass.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    }
}
