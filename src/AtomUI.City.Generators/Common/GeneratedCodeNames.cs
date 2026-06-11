namespace AtomUI.City.Generators.Common;

public static class GeneratedCodeNames
{
    public const string RootHintFolder = "AtomUI.City";
    public const string GeneratedNamespace = "AtomUI.City.Generated";

    public static string CreateHintName(GeneratorFeature feature, string assemblyName, string suffix)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            throw new ArgumentException("Assembly name cannot be empty.", nameof(assemblyName));
        }

        if (string.IsNullOrWhiteSpace(suffix))
        {
            throw new ArgumentException("Suffix cannot be empty.", nameof(suffix));
        }

        return $"{RootHintFolder}/{GeneratorFeatureNames.GetName(feature)}/{assemblyName}.{suffix}.g.cs";
    }

    public static GeneratedTypeName CreateRegistrarTypeName(GeneratorFeature feature, string suffix)
    {
        if (string.IsNullOrWhiteSpace(suffix))
        {
            throw new ArgumentException("Suffix cannot be empty.", nameof(suffix));
        }

        return new GeneratedTypeName(GeneratedNamespace, $"Generated{GeneratorFeatureNames.GetName(feature)}{suffix}");
    }
}
