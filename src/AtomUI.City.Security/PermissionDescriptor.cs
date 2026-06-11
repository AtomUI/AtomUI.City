namespace AtomUI.City.Security;

public sealed class PermissionDescriptor
{
    public PermissionDescriptor(
        string name,
        string? displayNameKey = null,
        string? descriptionKey = null,
        string? category = null,
        string? contributionId = null,
        string? defaultPolicy = null,
        bool isHostOnly = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        DisplayNameKey = displayNameKey;
        DescriptionKey = descriptionKey;
        Category = category;
        ContributionId = contributionId;
        DefaultPolicy = defaultPolicy;
        IsHostOnly = isHostOnly;
    }

    public string Name { get; }

    public string? DisplayNameKey { get; }

    public string? DescriptionKey { get; }

    public string? Category { get; }

    public string? ContributionId { get; }

    public string? DefaultPolicy { get; }

    public bool IsHostOnly { get; }
}
