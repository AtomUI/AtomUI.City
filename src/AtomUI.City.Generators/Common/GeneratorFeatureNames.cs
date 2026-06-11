namespace AtomUI.City.Generators.Common;

public static class GeneratorFeatureNames
{
    public const string Modularity = nameof(Modularity);
    public const string Routing = nameof(Routing);
    public const string Presentation = nameof(Presentation);
    public const string Security = nameof(Security);
    public const string EventBus = nameof(EventBus);
    public const string Localization = nameof(Localization);
    public const string PluginSystem = nameof(PluginSystem);

    public static string GetName(GeneratorFeature feature)
    {
        switch (feature)
        {
            case GeneratorFeature.Modularity:
                return Modularity;
            case GeneratorFeature.Routing:
                return Routing;
            case GeneratorFeature.Presentation:
                return Presentation;
            case GeneratorFeature.Security:
                return Security;
            case GeneratorFeature.EventBus:
                return EventBus;
            case GeneratorFeature.Localization:
                return Localization;
            case GeneratorFeature.PluginSystem:
                return PluginSystem;
            default:
                throw new ArgumentOutOfRangeException(nameof(feature), feature, "Unknown generator feature.");
        }
    }
}
