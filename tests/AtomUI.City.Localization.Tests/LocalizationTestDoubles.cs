using AtomUI.City.Localization;

namespace AtomUI.City.Localization.Tests;

internal sealed class RecordingLanguagePackageProvider : ILanguagePackageProvider
{
    private readonly Dictionary<string, LanguagePackage> _packages;

    public RecordingLanguagePackageProvider(params LanguagePackage[] packages)
    {
        _packages = packages.ToDictionary(package => package.Descriptor.PackageId, StringComparer.Ordinal);
    }

    public LanguagePackageProviderKind Kind => LanguagePackageProviderKind.InMemory;

    public List<string> LoadedCultures { get; } = [];

    public string? FailingCultureName { get; set; }

    public ValueTask<LanguagePackageLoadResult> LoadAsync(
        LanguagePackageDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        LoadedCultures.Add(descriptor.Culture.Name);

        if (descriptor.Culture.Name == FailingCultureName)
        {
            return ValueTask.FromResult(
                LanguagePackageLoadResult.Failed(
                    new LocalizationError(
                        LocalizationErrorKind.PackageLoadFailed,
                        "Package load failed.")));
        }

        return _packages.TryGetValue(descriptor.PackageId, out var package)
            ? ValueTask.FromResult(LanguagePackageLoadResult.Success(package))
            : ValueTask.FromResult(
                LanguagePackageLoadResult.Failed(
                    new LocalizationError(
                        LocalizationErrorKind.PackageNotFound,
                        "Package was not found.")));
    }
}

internal sealed class RecordingPresentationLocalizationBridge : IPresentationLocalizationBridge
{
    public List<string> AppliedCultures { get; } = [];

    public string? FailingCultureName { get; set; }

    public ValueTask<LocalizationResult> ApplyCultureAsync(
        CultureState state,
        CancellationToken cancellationToken = default)
    {
        if (state.CurrentCulture.Name == FailingCultureName)
        {
            return ValueTask.FromResult(
                LocalizationResult.Failed(
                    new LocalizationError(
                        LocalizationErrorKind.PresentationApplyFailed,
                        "Presentation apply failed.")));
        }

        AppliedCultures.Add(state.CurrentCulture.Name);

        return ValueTask.FromResult(LocalizationResult.Success());
    }
}

internal sealed class LocalizationTestWorkspace : IDisposable
{
    public LocalizationTestWorkspace()
    {
        Root = Path.Combine(Path.GetTempPath(), "AtomUICityLocalizationTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);
    }

    public string Root { get; }

    public string WriteLocPack(string json)
    {
        var path = Path.Combine(Root, "test.locpack.json");
        File.WriteAllText(path, json);

        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(Root))
        {
            Directory.Delete(Root, recursive: true);
        }
    }
}
