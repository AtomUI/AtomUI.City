using AtomUI.City.PluginSystem;

namespace AtomUI.City.PluginSystem.Tests;

public sealed class PluginResultTests
{
    [Fact]
    public void LoadResultDiagnosticsRejectExternalListMutation()
    {
        var diagnostic = new PluginDiagnostic("AUCPLGTEST", "failure");
        var replacement = new PluginDiagnostic("AUCPLGOTHER", "replacement");
        var result = PluginLoadResult.Failed([diagnostic]);
        var diagnostics = Assert.IsAssignableFrom<IList<PluginDiagnostic>>(result.Diagnostics);

        Assert.Throws<NotSupportedException>(() => diagnostics[0] = replacement);
        Assert.Equal(diagnostic.Code, result.Diagnostics[0].Code);
    }

    [Fact]
    public void InstallResultDiagnosticsRejectExternalListMutation()
    {
        var diagnostic = new PluginDiagnostic("AUCPLGTEST", "failure");
        var replacement = new PluginDiagnostic("AUCPLGOTHER", "replacement");
        var result = PluginInstallResult.Failed([diagnostic]);
        var diagnostics = Assert.IsAssignableFrom<IList<PluginDiagnostic>>(result.Diagnostics);

        Assert.Throws<NotSupportedException>(() => diagnostics[0] = replacement);
        Assert.Equal(diagnostic.Code, result.Diagnostics[0].Code);
    }

    [Fact]
    public void ValidationResultDiagnosticsRejectExternalListMutation()
    {
        var diagnostic = new PluginDiagnostic("AUCPLGTEST", "failure");
        var replacement = new PluginDiagnostic("AUCPLGOTHER", "replacement");
        var result = new PluginValidationResult([diagnostic]);
        var diagnostics = Assert.IsAssignableFrom<IList<PluginDiagnostic>>(result.Diagnostics);

        Assert.Throws<NotSupportedException>(() => diagnostics[0] = replacement);
        Assert.Equal(diagnostic.Code, result.Diagnostics[0].Code);
    }
}
