namespace AtomUI.City.Templates;

public sealed record TemplateChange(string Type, string Path)
{
    public static TemplateChange Create(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return new TemplateChange("create", path);
    }
}
