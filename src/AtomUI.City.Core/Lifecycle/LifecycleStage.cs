namespace AtomUI.City.Lifecycle;

public readonly record struct LifecycleStage
{
    public LifecycleStage(LifecycleStageArea area, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Area = area;
        Name = name;
    }

    public LifecycleStageArea Area { get; }

    public string AreaName => Area.ToString();

    public string Name { get; }

    public string Key => AreaName + "." + Name;
}
