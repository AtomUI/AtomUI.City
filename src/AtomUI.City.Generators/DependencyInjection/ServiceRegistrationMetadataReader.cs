using Microsoft.CodeAnalysis;

namespace AtomUI.City.Generators.DependencyInjection;

public static class ServiceRegistrationMetadataReader
{
    private const string ExposeServicesAttributeName = "AtomUI.City.DependencyInjection.ExposeServicesAttribute";
    private const string ScopedDependencyInterfaceName = "AtomUI.City.DependencyInjection.IScopedDependency";
    private const string ScopedServiceAttributeName = "AtomUI.City.DependencyInjection.ScopedServiceAttribute";
    private const string ServiceAttributeName = "AtomUI.City.DependencyInjection.ServiceAttribute";
    private const string SingletonDependencyInterfaceName = "AtomUI.City.DependencyInjection.ISingletonDependency";
    private const string TransientDependencyInterfaceName = "AtomUI.City.DependencyInjection.ITransientDependency";

    public static ServiceRegistrationMetadata? TryRead(INamedTypeSymbol type)
    {
        if (type.TypeKind != TypeKind.Class || type.IsAbstract)
        {
            return null;
        }

        var implementationTypeName = GetTypeName(type);
        var serviceDeclaration = ReadServiceDeclaration(type);

        if (serviceDeclaration is null)
        {
            serviceDeclaration = ReadMarkerInterfaceDeclaration(type);
        }

        if (serviceDeclaration is null)
        {
            return null;
        }

        var exposedServiceTypeNames = ReadExposedServiceTypeNames(type)
            .Concat(serviceDeclaration.ExposedServiceTypeNames)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (exposedServiceTypeNames.Length == 0)
        {
            exposedServiceTypeNames = [implementationTypeName];
        }

        return new ServiceRegistrationMetadata(
            implementationTypeName,
            serviceDeclaration.Lifetime,
            exposedServiceTypeNames,
            serviceDeclaration.Replace,
            serviceDeclaration.TryAdd,
            serviceDeclaration.Key);
    }

    private static ServiceDeclaration? ReadServiceDeclaration(INamedTypeSymbol type)
    {
        foreach (var attribute in type.GetAttributes())
        {
            var attributeName = GetAttributeTypeName(attribute);

            if (string.Equals(attributeName, ServiceAttributeName, StringComparison.Ordinal))
            {
                return new ServiceDeclaration(
                    ReadLifetime(attribute),
                    [],
                    ReadNamedBoolean(attribute, "Replace"),
                    ReadNamedBoolean(attribute, "TryAdd"),
                    ReadNamedString(attribute, "Key"));
            }

            if (string.Equals(attributeName, ScopedServiceAttributeName, StringComparison.Ordinal))
            {
                return new ServiceDeclaration(
                    ServiceRegistrationLifetime.Scoped,
                    ReadConstructorServiceTypes(attribute),
                    ReadNamedBoolean(attribute, "Replace"),
                    ReadNamedBoolean(attribute, "TryAdd"),
                    ReadNamedString(attribute, "Key"));
            }
        }

        return null;
    }

    private static ServiceDeclaration? ReadMarkerInterfaceDeclaration(INamedTypeSymbol type)
    {
        foreach (var @interface in type.AllInterfaces)
        {
            var interfaceName = GetTypeName(@interface);

            if (string.Equals(interfaceName, SingletonDependencyInterfaceName, StringComparison.Ordinal))
            {
                return new ServiceDeclaration(ServiceRegistrationLifetime.Singleton, [], replace: false, tryAdd: false, key: null);
            }

            if (string.Equals(interfaceName, ScopedDependencyInterfaceName, StringComparison.Ordinal))
            {
                return new ServiceDeclaration(ServiceRegistrationLifetime.Scoped, [], replace: false, tryAdd: false, key: null);
            }

            if (string.Equals(interfaceName, TransientDependencyInterfaceName, StringComparison.Ordinal))
            {
                return new ServiceDeclaration(ServiceRegistrationLifetime.Transient, [], replace: false, tryAdd: false, key: null);
            }
        }

        return null;
    }

    private static IReadOnlyList<string> ReadExposedServiceTypeNames(INamedTypeSymbol type)
    {
        return type
            .GetAttributes()
            .Where(attribute => string.Equals(GetAttributeTypeName(attribute), ExposeServicesAttributeName, StringComparison.Ordinal))
            .SelectMany(ReadConstructorServiceTypes)
            .ToArray();
    }

    private static IReadOnlyList<string> ReadConstructorServiceTypes(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length == 0)
        {
            return [];
        }

        return ReadTypeNames(attribute.ConstructorArguments[0]);
    }

    private static IReadOnlyList<string> ReadTypeNames(TypedConstant constant)
    {
        if (constant.Kind == TypedConstantKind.Array)
        {
            return constant
                .Values
                .Select(ReadTypeName)
                .Where(typeName => typeName is not null)
                .Cast<string>()
                .ToArray();
        }

        var singleTypeName = ReadTypeName(constant);

        return singleTypeName is null ? [] : [singleTypeName];
    }

    private static string? ReadTypeName(TypedConstant constant)
    {
        return constant.Value is INamedTypeSymbol type ? GetTypeName(type) : null;
    }

    private static ServiceRegistrationLifetime ReadLifetime(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length == 0)
        {
            return ServiceRegistrationLifetime.Transient;
        }

        var value = attribute.ConstructorArguments[0].Value;

        return value switch
        {
            0 => ServiceRegistrationLifetime.Singleton,
            1 => ServiceRegistrationLifetime.Scoped,
            _ => ServiceRegistrationLifetime.Transient,
        };
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
        return attribute.AttributeClass is null ? null : GetTypeName(attribute.AttributeClass);
    }

    private static string GetTypeName(INamedTypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    }

    private sealed class ServiceDeclaration
    {
        public ServiceDeclaration(
            ServiceRegistrationLifetime lifetime,
            IReadOnlyList<string> exposedServiceTypeNames,
            bool replace,
            bool tryAdd,
            string? key)
        {
            Lifetime = lifetime;
            ExposedServiceTypeNames = exposedServiceTypeNames;
            Replace = replace;
            TryAdd = tryAdd;
            Key = key;
        }

        public ServiceRegistrationLifetime Lifetime { get; }

        public IReadOnlyList<string> ExposedServiceTypeNames { get; }

        public bool Replace { get; }

        public bool TryAdd { get; }

        public string? Key { get; }
    }
}
