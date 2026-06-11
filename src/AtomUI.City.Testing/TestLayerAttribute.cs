namespace AtomUI.City.Testing;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class TestLayerAttribute : Attribute
{
    public TestLayerAttribute(TestLayer layer)
    {
        Layer = layer;
        Category = TestLayerNames.GetCategory(layer);
    }

    public TestLayer Layer { get; }

    public string Category { get; }
}
