using AtomUI.City.Generators.Diagnostics;
using AtomUI.City.Generators.Presentation;

namespace AtomUI.City.Generators.Tests;

public sealed class PresentationViewManifestBuilderTests
{
    [Fact]
    public void BuildSortsViewsDeterministically()
    {
        var result = PresentationViewManifestBuilder.Build(
            [
                View("Sample.App.SettingsView", "Sample.App.SettingsViewModel", viewKey: "settings"),
                View("Sample.App.AdminView", "Sample.App.AdminViewModel"),
            ]);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(
            ["Sample.App.AdminView", "Sample.App.SettingsView"],
            result.Manifest.Views.Select(view => view.ViewTypeName));
    }

    [Fact]
    public void BuildReportsDuplicateViewsForSameViewModelAndKey()
    {
        var result = PresentationViewManifestBuilder.Build(
            [
                View("Sample.App.SettingsView", "Sample.App.SettingsViewModel"),
                View("Sample.App.AlternateSettingsView", "Sample.App.SettingsViewModel"),
            ]);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.DuplicatePresentationView, diagnostic.Id);
        Assert.Empty(result.Manifest.Views);
    }

    [Fact]
    public void BuildAllowsMultipleViewsForSameViewModelWhenKeysDiffer()
    {
        var result = PresentationViewManifestBuilder.Build(
            [
                View("Sample.App.SettingsView", "Sample.App.SettingsViewModel"),
                View("Sample.App.SettingsDialogView", "Sample.App.SettingsViewModel", viewKey: "dialog"),
            ]);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(
            [null, "dialog"],
            result.Manifest.Views.Select(view => view.ViewKey));
    }

    [Fact]
    public void BuildCarriesContributionMetadata()
    {
        var result = PresentationViewManifestBuilder.Build(
            [
                View(
                    "Sample.Plugin.SettingsView",
                    "Sample.Plugin.SettingsViewModel",
                    pluginId: "com.company.sales",
                    contributionId: "plugin.settings.view"),
            ]);

        var view = Assert.Single(result.Manifest.Views);

        Assert.Equal("com.company.sales", view.PluginId);
        Assert.Equal("plugin.settings.view", view.ContributionId);
    }

    [Fact]
    public void BuildReportsPluginViewsWithoutContributionIds()
    {
        var result = PresentationViewManifestBuilder.Build(
            [
                View(
                    "Sample.Plugin.SettingsView",
                    "Sample.Plugin.SettingsViewModel",
                    pluginId: "com.company.sales"),
            ]);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.InvalidManifestInput, diagnostic.Id);
        Assert.Contains("ContributionId", diagnostic.Message, StringComparison.Ordinal);
        Assert.Empty(result.Manifest.Views);
    }

    [Fact]
    public void BuildReportsContributionViewsWithoutPluginIds()
    {
        var result = PresentationViewManifestBuilder.Build(
            [
                View(
                    "Sample.Plugin.SettingsView",
                    "Sample.Plugin.SettingsViewModel",
                    contributionId: "plugin.settings.view"),
            ]);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.InvalidManifestInput, diagnostic.Id);
        Assert.Contains("PluginId", diagnostic.Message, StringComparison.Ordinal);
        Assert.Empty(result.Manifest.Views);
    }

    [Fact]
    public void BuildReturnsReadonlyPresentationViewCollections()
    {
        var result = PresentationViewManifestBuilder.Build(
            [
                View("Sample.App.SettingsView", "Sample.App.SettingsViewModel"),
            ]);
        var views = Assert.IsAssignableFrom<IList<PresentationViewManifestEntry>>(result.Manifest.Views);
        var diagnostics = Assert.IsAssignableFrom<IList<GeneratorDiagnostic>>(result.Diagnostics);

        Assert.Throws<NotSupportedException>(() => views[0] = new PresentationViewManifestEntry("Sample.App.ChangedView", "Sample.App.ChangedViewModel", null, null, null));
        Assert.Throws<NotSupportedException>(() => diagnostics.Add(new GeneratorDiagnostic(GeneratorDiagnostics.InvalidManifestInput, "Changed")));
        Assert.Equal("Sample.App.SettingsView", result.Manifest.Views[0].ViewTypeName);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void PresentationViewConstructorParametersRejectExternalMutation()
    {
        var constructorParameters = new List<PresentationViewConstructorParameter>
        {
            new("Sample.App.IClock"),
        };
        var metadata = new PresentationViewMetadata(
            "Sample.App.SettingsView",
            "Sample.App.SettingsViewModel",
            viewKey: null,
            pluginId: null,
            contributionId: null,
            location: null,
            constructorParameters);
        var manifestEntry = new PresentationViewManifestEntry(
            "Sample.App.SettingsView",
            "Sample.App.SettingsViewModel",
            viewKey: null,
            pluginId: null,
            contributionId: null,
            constructorParameters);
        var metadataParameters = Assert.IsAssignableFrom<IList<PresentationViewConstructorParameter>>(metadata.ConstructorParameters);
        var manifestParameters = Assert.IsAssignableFrom<IList<PresentationViewConstructorParameter>>(manifestEntry.ConstructorParameters);

        Assert.Throws<NotSupportedException>(() => metadataParameters[0] = new PresentationViewConstructorParameter("Sample.App.IChangedClock"));
        Assert.Throws<NotSupportedException>(() => manifestParameters[0] = new PresentationViewConstructorParameter("Sample.App.IChangedClock"));
        Assert.Equal("Sample.App.IClock", metadata.ConstructorParameters[0].TypeName);
        Assert.Equal("Sample.App.IClock", manifestEntry.ConstructorParameters[0].TypeName);
    }

    private static PresentationViewMetadata View(
        string viewTypeName,
        string viewModelTypeName,
        string? viewKey = null,
        string? pluginId = null,
        string? contributionId = null)
    {
        return new PresentationViewMetadata(
            viewTypeName,
            viewModelTypeName,
            viewKey,
            pluginId,
            contributionId);
    }
}
