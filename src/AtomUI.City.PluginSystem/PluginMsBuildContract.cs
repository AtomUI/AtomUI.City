namespace AtomUI.City.PluginSystem;

public static class PluginMsBuildContract
{
    public static IReadOnlyList<string> Properties { get; } =
    [
        "AtomUICityPlugin",
        "AtomUICityPluginId",
        "AtomUICityPluginVersion",
        "AtomUICityPluginPublisher",
        "AtomUICityPluginDisplayNameKey",
        "AtomUICityPluginDescriptionKey",
        "AtomUICityMinHostVersion",
        "AtomUICityMaxHostVersion",
        "AtomUICityPluginApiVersion",
        "AtomUICityPluginUnloadable",
        "AtomUICityPluginNativeAotCompatible",
        "AtomUICityPluginResourceMode",
        "AtomUICityPackageAsPlugin",
    ];

    public static IReadOnlyList<string> Items { get; } =
    [
        "AtomUICityPluginCapability",
        "AtomUICityPluginDependency",
        "AtomUICityPluginContract",
        "AtomUICityLanguagePackage",
        "AtomUICityPluginAsset",
        "AtomUICityPluginNativeAsset",
        "AtomUICityContributionManifest",
    ];

    public static IReadOnlyList<string> Targets { get; } =
    [
        "GenerateAtomUICityPluginManifest",
        "GenerateAtomUICityContributionManifests",
        "ValidateAtomUICityPluginManifest",
        "ValidateAtomUICityPluginPackage",
        "PackAtomUICityPlugin",
        "InstallAtomUICityPluginToLocalCache",
        "CleanAtomUICityPluginArtifacts",
    ];
}
