namespace AtomUI.City.Generators.Presentation;

public sealed class PresentationViewManifestEntry
{
    public PresentationViewManifestEntry(
        string viewTypeName,
        string viewModelTypeName,
        string? viewKey,
        string? pluginId,
        string? contributionId,
        IReadOnlyList<PresentationViewConstructorParameter>? constructorParameters = null)
    {
        if (string.IsNullOrWhiteSpace(viewTypeName))
        {
            throw new ArgumentException("View type name cannot be empty.", nameof(viewTypeName));
        }

        if (string.IsNullOrWhiteSpace(viewModelTypeName))
        {
            throw new ArgumentException("View model type name cannot be empty.", nameof(viewModelTypeName));
        }

        ViewTypeName = viewTypeName;
        ViewModelTypeName = viewModelTypeName;
        ViewKey = string.IsNullOrWhiteSpace(viewKey) ? null : viewKey;
        PluginId = string.IsNullOrWhiteSpace(pluginId) ? null : pluginId;
        ContributionId = string.IsNullOrWhiteSpace(contributionId) ? null : contributionId;
        ConstructorParameters = Array.AsReadOnly(constructorParameters?.ToArray() ?? []);
    }

    public string ViewTypeName { get; }

    public string ViewModelTypeName { get; }

    public string? ViewKey { get; }

    public string? PluginId { get; }

    public string? ContributionId { get; }

    public IReadOnlyList<PresentationViewConstructorParameter> ConstructorParameters { get; }
}
