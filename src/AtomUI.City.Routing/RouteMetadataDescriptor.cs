namespace AtomUI.City.Routing;

public sealed class RouteMetadataDescriptor
{
    public RouteMetadataDescriptor(
        string? titleKey = null,
        string? descriptionKey = null,
        string? breadcrumbKey = null,
        string? groupKey = null,
        string? errorTitleKey = null)
    {
        TitleKey = NormalizeKey(titleKey);
        DescriptionKey = NormalizeKey(descriptionKey);
        BreadcrumbKey = NormalizeKey(breadcrumbKey);
        GroupKey = NormalizeKey(groupKey);
        ErrorTitleKey = NormalizeKey(errorTitleKey);
    }

    public static RouteMetadataDescriptor Empty { get; } = new();

    public string? TitleKey { get; }

    public string? DescriptionKey { get; }

    public string? BreadcrumbKey { get; }

    public string? GroupKey { get; }

    public string? ErrorTitleKey { get; }

    private static string? NormalizeKey(string? key)
    {
        return string.IsNullOrWhiteSpace(key) ? null : key;
    }
}
