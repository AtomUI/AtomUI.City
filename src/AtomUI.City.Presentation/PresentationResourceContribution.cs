namespace AtomUI.City.Presentation;

public sealed class PresentationResourceContribution
{
    public PresentationResourceContribution(
        string kind,
        object resource,
        string? pluginId = null,
        string? contributionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentNullException.ThrowIfNull(resource);

        Kind = kind;
        Resource = resource;
        PluginId = string.IsNullOrWhiteSpace(pluginId) ? null : pluginId;
        ContributionId = string.IsNullOrWhiteSpace(contributionId) ? null : contributionId;
    }

    public string Kind { get; }

    public object Resource { get; }

    public string? PluginId { get; }

    public string? ContributionId { get; }
}
