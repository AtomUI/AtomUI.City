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
