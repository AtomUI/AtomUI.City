using AtomUI.City.Generators.Presentation;

namespace AtomUI.City.Generators.Tests;

public sealed class PresentationViewRegistrarSourceBuilderTests
{
    [Fact]
    public void BuildCreatesAotFriendlyViewRegistrarSource()
    {
        var manifest = new PresentationViewManifest(
            [
                new PresentationViewManifestEntry(
                    "Sample.App.SettingsView",
                    "Sample.App.SettingsViewModel",
                    viewKey: null,
                    pluginId: null,
                    contributionId: null),
            ]);

        var source = PresentationViewRegistrarSourceBuilder.Build(manifest);

        Assert.Contains("namespace AtomUI.City.Generated;", source, StringComparison.Ordinal);
        Assert.Contains("public static class GeneratedPresentationViewRegistrar", source, StringComparison.Ordinal);
        Assert.Contains(
            "public static void RegisterViews(global::AtomUI.City.Presentation.IViewRegistry registry)",
            source,
            StringComparison.Ordinal);
        Assert.Contains("global::System.ArgumentNullException.ThrowIfNull(registry);", source, StringComparison.Ordinal);
        Assert.Contains("typeof(global::Sample.App.SettingsViewModel)", source, StringComparison.Ordinal);
        Assert.Contains("typeof(global::Sample.App.SettingsView)", source, StringComparison.Ordinal);
        Assert.Contains("static context => new global::Sample.App.SettingsView()", source, StringComparison.Ordinal);
        Assert.Contains("pluginId: null", source, StringComparison.Ordinal);
        Assert.Contains("contributionId: null", source, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildCarriesPluginMetadataAndEscapesStringValues()
    {
        var manifest = new PresentationViewManifest(
            [
                new PresentationViewManifestEntry(
                    "Sample.Plugin.SettingsView",
                    "Sample.Plugin.SettingsViewModel",
                    "settings\"panel",
                    "com.company.sales",
                    "plugin.settings\\view"),
            ]);

        var source = PresentationViewRegistrarSourceBuilder.Build(manifest);

        Assert.Contains("@\"settings\"\"panel\"", source, StringComparison.Ordinal);
        Assert.Contains("@\"com.company.sales\"", source, StringComparison.Ordinal);
        Assert.Contains("@\"plugin.settings\\view\"", source, StringComparison.Ordinal);
    }
}
