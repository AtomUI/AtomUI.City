using System.Globalization;
using AtomUI.City.Localization;

namespace AtomUI.City.Localization.Tests;

public sealed class CultureStateTests
{
    [Fact]
    public void CollectionsRejectExternalListMutation()
    {
        var state = new CultureState(
            CultureInfo.GetCultureInfo("zh-CN"),
            CultureInfo.GetCultureInfo("zh-CN"),
            [CultureInfo.GetCultureInfo("en-US")],
            revision: 1,
            ["Host.zh-CN"]);
        var fallbackCultures = Assert.IsAssignableFrom<IList<CultureInfo>>(state.FallbackCultures);
        var loadedPackageIds = Assert.IsAssignableFrom<IList<string>>(state.LoadedPackageIds);

        Assert.Throws<NotSupportedException>(() => fallbackCultures[0] = CultureInfo.GetCultureInfo("ja-JP"));
        Assert.Throws<NotSupportedException>(() => loadedPackageIds[0] = "Changed");
        Assert.Equal("en-US", state.FallbackCultures[0].Name);
        Assert.Equal("Host.zh-CN", state.LoadedPackageIds[0]);
    }
}
