using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.Presentation;

public static class PresentationViewManifestBuilder
{
    public static PresentationViewManifestResult Build(IReadOnlyList<PresentationViewMetadata> views)
    {
        if (views is null)
        {
            throw new ArgumentNullException(nameof(views));
        }

        var diagnostics = new List<GeneratorDiagnostic>();
        var viewsByViewModelAndKey = new Dictionary<string, PresentationViewMetadata>(StringComparer.Ordinal);

        foreach (var view in views)
        {
            var key = CreateViewModelKey(view.ViewModelTypeName, view.ViewKey);
            if (!viewsByViewModelAndKey.ContainsKey(key))
            {
                viewsByViewModelAndKey.Add(key, view);
                continue;
            }

            diagnostics.Add(new GeneratorDiagnostic(
                GeneratorDiagnostics.DuplicatePresentationView,
                $"View model '{view.ViewModelTypeName}' has more than one view registered for key '{NormalizeViewKey(view.ViewKey)}'.",
                view.ViewModelTypeName));
        }

        if (diagnostics.Count > 0)
        {
            return new PresentationViewManifestResult(new PresentationViewManifest([]), diagnostics);
        }

        var manifestViews = views
            .Select(view => new PresentationViewManifestEntry(
                view.ViewTypeName,
                view.ViewModelTypeName,
                view.ViewKey,
                view.ContributionId))
            .OrderBy(view => view.ViewModelTypeName, StringComparer.Ordinal)
            .ThenBy(view => view.ViewKey ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(view => view.ViewTypeName, StringComparer.Ordinal)
            .ToArray();

        return new PresentationViewManifestResult(new PresentationViewManifest(manifestViews), diagnostics);
    }

    private static string CreateViewModelKey(string viewModelTypeName, string? viewKey)
    {
        return viewModelTypeName + "|" + (viewKey ?? string.Empty);
    }

    private static string NormalizeViewKey(string? viewKey)
    {
        return string.IsNullOrWhiteSpace(viewKey) ? "<default>" : viewKey!;
    }
}
