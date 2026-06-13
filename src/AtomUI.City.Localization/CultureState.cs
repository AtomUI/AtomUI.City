using System.Globalization;

namespace AtomUI.City.Localization;

public sealed class CultureState
{
    public CultureState(
        CultureInfo currentCulture,
        CultureInfo currentUICulture,
        IReadOnlyList<CultureInfo> fallbackCultures,
        long revision,
        IReadOnlyList<string> loadedPackageIds)
    {
        CurrentCulture = currentCulture ?? throw new ArgumentNullException(nameof(currentCulture));
        CurrentUICulture = currentUICulture ?? throw new ArgumentNullException(nameof(currentUICulture));
        FallbackCultures = Array.AsReadOnly(fallbackCultures.ToArray());
        Revision = revision;
        LoadedPackageIds = Array.AsReadOnly(loadedPackageIds.ToArray());
    }

    public CultureInfo CurrentCulture { get; }

    public CultureInfo CurrentUICulture { get; }

    public IReadOnlyList<CultureInfo> FallbackCultures { get; }

    public long Revision { get; }

    public IReadOnlyList<string> LoadedPackageIds { get; }
}
