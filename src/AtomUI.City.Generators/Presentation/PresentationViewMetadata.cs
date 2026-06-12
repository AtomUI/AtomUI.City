namespace AtomUI.City.Generators.Presentation;

public sealed class PresentationViewMetadata
{
    public PresentationViewMetadata(
        string viewTypeName,
        string viewModelTypeName,
        string? viewKey,
        string? contributionId)
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
        ContributionId = string.IsNullOrWhiteSpace(contributionId) ? null : contributionId;
    }

    public string ViewTypeName { get; }

    public string ViewModelTypeName { get; }

    public string? ViewKey { get; }

    public string? ContributionId { get; }
}
