namespace AtomUI.City.Testing;

public static class TestLayerNames
{
    public const string Unit = nameof(Unit);
    public const string Contract = nameof(Contract);
    public const string FrameworkIntegration = nameof(FrameworkIntegration);
    public const string RuntimeLifecycle = nameof(RuntimeLifecycle);
    public const string PluginLifecycle = nameof(PluginLifecycle);
    public const string PlatformIntegration = nameof(PlatformIntegration);
    public const string TemplateSmoke = nameof(TemplateSmoke);
    public const string Generator = nameof(Generator);
    public const string Analyzer = nameof(Analyzer);
    public const string Build = nameof(Build);

    public static string GetCategory(TestLayer layer)
    {
        return layer switch
        {
            TestLayer.Unit => Unit,
            TestLayer.Contract => Contract,
            TestLayer.FrameworkIntegration => FrameworkIntegration,
            TestLayer.RuntimeLifecycle => RuntimeLifecycle,
            TestLayer.PluginLifecycle => PluginLifecycle,
            TestLayer.PlatformIntegration => PlatformIntegration,
            TestLayer.TemplateSmoke => TemplateSmoke,
            TestLayer.Generator => Generator,
            TestLayer.Analyzer => Analyzer,
            TestLayer.Build => Build,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Unknown test layer."),
        };
    }
}
