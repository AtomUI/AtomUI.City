using AtomUI.City.Testing;

namespace AtomUI.City.Testing.Tests;

public sealed class TestLayerTests
{
    [Fact]
    public void TestLayerNamesExposeStableCategoryValues()
    {
        Assert.Equal("Unit", TestLayerNames.GetCategory(TestLayer.Unit));
        Assert.Equal("Contract", TestLayerNames.GetCategory(TestLayer.Contract));
        Assert.Equal("FrameworkIntegration", TestLayerNames.GetCategory(TestLayer.FrameworkIntegration));
        Assert.Equal("RuntimeLifecycle", TestLayerNames.GetCategory(TestLayer.RuntimeLifecycle));
        Assert.Equal("PluginLifecycle", TestLayerNames.GetCategory(TestLayer.PluginLifecycle));
        Assert.Equal("PlatformIntegration", TestLayerNames.GetCategory(TestLayer.PlatformIntegration));
        Assert.Equal("TemplateSmoke", TestLayerNames.GetCategory(TestLayer.TemplateSmoke));
        Assert.Equal("Generator", TestLayerNames.GetCategory(TestLayer.Generator));
        Assert.Equal("Analyzer", TestLayerNames.GetCategory(TestLayer.Analyzer));
        Assert.Equal("Build", TestLayerNames.GetCategory(TestLayer.Build));
    }

    [Fact]
    public void TestLayerAttributeStoresRunnerNeutralMetadata()
    {
        var attribute = new TestLayerAttribute(TestLayer.FrameworkIntegration);

        Assert.Equal(TestLayer.FrameworkIntegration, attribute.Layer);
        Assert.Equal("FrameworkIntegration", attribute.Category);
    }
}
