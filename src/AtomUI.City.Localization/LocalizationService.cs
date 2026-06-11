using System.Globalization;

namespace AtomUI.City.Localization;

public sealed class LocalizationService : ILocalizationService
{
    private readonly IReadOnlyList<LanguagePackageDescriptor> _descriptors;
    private readonly IReadOnlyDictionary<LanguagePackageProviderKind, ILanguagePackageProvider> _providers;
    private readonly IPresentationLocalizationBridge _bridge;
    private readonly ILocalizationDiagnostics? _diagnostics;
    private readonly Dictionary<string, LanguagePackage> _loadedPackages = [];
    private readonly List<LocalizedText> _localizedTexts = [];
    private readonly object _localizedTextGate = new();
    private readonly SemaphoreSlim _switchLock = new(1, 1);

    public LocalizationService(
        IReadOnlyList<LanguagePackageDescriptor> descriptors,
        IEnumerable<ILanguagePackageProvider> providers,
        IPresentationLocalizationBridge? bridge = null,
        ILocalizationDiagnostics? diagnostics = null)
    {
        ArgumentNullException.ThrowIfNull(descriptors);
        ArgumentNullException.ThrowIfNull(providers);

        _descriptors = descriptors.ToArray();
        _providers = providers.ToDictionary(provider => provider.Kind);
        _bridge = bridge ?? NoopPresentationLocalizationBridge.Instance;
        _diagnostics = diagnostics;
        State = new CultureState(
            CultureInfo.InvariantCulture,
            CultureInfo.InvariantCulture,
            [],
            revision: 0,
            loadedPackageIds: []);
    }

    public CultureState State { get; private set; }

    public CultureInfo CurrentCulture => State.CurrentCulture;

    public long CultureRevision => State.Revision;

    public async ValueTask<LocalizationResult> SetCultureAsync(
        string cultureName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureName);

        await _switchLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            var targetDescriptors = GetDescriptors(culture).ToArray();
            var pendingPackages = new List<LanguagePackage>();

            foreach (var descriptor in targetDescriptors)
            {
                var loadResult = await LoadPackageAsync(
                        descriptor,
                        cache: false,
                        cancellationToken)
                    .ConfigureAwait(false);

                if (!loadResult.Succeeded)
                {
                    DisposeAll(pendingPackages);
                    WritePackageLoadFailed(descriptor, loadResult.Error);

                    return LocalizationResult.Failed(loadResult.Error!);
                }

                pendingPackages.Add(loadResult.Package!);
            }

            var nextState = new CultureState(
                culture,
                culture,
                GetFallbackCultures(culture).ToArray(),
                State.Revision + 1,
                targetDescriptors.Select(descriptor => descriptor.PackageId).ToArray());
            var bridgeResult = await _bridge.ApplyCultureAsync(nextState, cancellationToken).ConfigureAwait(false);

            if (!bridgeResult.Succeeded)
            {
                DisposeAll(pendingPackages);
                WriteDiagnostic(
                    LocalizationDiagnosticIds.AtomUiApplyFailed,
                    bridgeResult.Error!.Message,
                    LocalizationDiagnosticSeverity.Error,
                    cultureName: culture.Name,
                    errorKind: bridgeResult.Error.Kind);

                return bridgeResult;
            }

            foreach (var package in pendingPackages)
            {
                _loadedPackages[package.Descriptor.PackageId] = package;
            }

            State = nextState;
            await RefreshLocalizedTextsAsync(cancellationToken).ConfigureAwait(false);

            return LocalizationResult.Success();
        }
        catch (OperationCanceledException)
        {
            return LocalizationResult.Failed(
                new LocalizationError(LocalizationErrorKind.Cancelled, "Culture switch was cancelled."));
        }
        finally
        {
            _switchLock.Release();
        }
    }

    public async ValueTask<LocalizedString> GetStringAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        foreach (var descriptor in GetDescriptors(CurrentCulture))
        {
            var loadResult = await LoadPackageAsync(descriptor, cache: true, cancellationToken).ConfigureAwait(false);
            if (loadResult.Succeeded && loadResult.Package!.TryGetString(key, out var value))
            {
                return LocalizedString.Found(key, value, descriptor.Culture);
            }
        }

        foreach (var fallbackCulture in GetFallbackCultures(CurrentCulture))
        {
            foreach (var descriptor in GetDescriptors(fallbackCulture))
            {
                var loadResult = await LoadPackageAsync(descriptor, cache: true, cancellationToken).ConfigureAwait(false);
                if (loadResult.Succeeded && loadResult.Package!.TryGetString(key, out var value))
                {
                    return LocalizedString.Fallback(key, value, descriptor.Culture);
                }
            }
        }

        WriteDiagnostic(
            LocalizationDiagnosticIds.ResourceMissing,
            $"Localized resource '{key}' was not found.",
            LocalizationDiagnosticSeverity.Warning,
            cultureName: CurrentCulture.Name,
            resourceKey: key,
            errorKind: LocalizationErrorKind.ResourceMissing);

        return LocalizedString.Missing(key, CurrentCulture);
    }

    public async ValueTask<LocalizedMessage> GetMessageAsync(
        string key,
        IReadOnlyList<object?> arguments,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(arguments);

        var template = await GetStringAsync(key, cancellationToken).ConfigureAwait(false);

        if (template.IsMissing || arguments.Count == 0)
        {
            return LocalizedMessage.FromString(template, template.Value);
        }

        try
        {
            return LocalizedMessage.FromString(
                template,
                string.Format(template.Culture, template.Value, arguments.ToArray()));
        }
        catch (FormatException exception)
        {
            WriteDiagnostic(
                LocalizationDiagnosticIds.MessageFormatFailed,
                exception.Message,
                LocalizationDiagnosticSeverity.Error,
                cultureName: template.Culture.Name,
                resourceKey: key,
                errorKind: LocalizationErrorKind.FormatFailed);

            return LocalizedMessage.FromString(template, template.Value, isFormatFailed: true);
        }
    }

    public async ValueTask<ILocalizedText> CreateTextAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var text = await LocalizedText.CreateAsync(this, key, cancellationToken).ConfigureAwait(false);
        RegisterLocalizedText(text);

        return text;
    }

    internal void UnregisterLocalizedText(LocalizedText text)
    {
        lock (_localizedTextGate)
        {
            _localizedTexts.Remove(text);
        }
    }

    internal void WriteTextRefreshFailed(string key, Exception exception)
    {
        WriteDiagnostic(
            LocalizationDiagnosticIds.TextRefreshFailed,
            exception.Message,
            LocalizationDiagnosticSeverity.Error,
            cultureName: CurrentCulture.Name,
            resourceKey: key,
            errorKind: LocalizationErrorKind.RefreshFailed);
    }

    private void RegisterLocalizedText(LocalizedText text)
    {
        lock (_localizedTextGate)
        {
            _localizedTexts.Add(text);
        }
    }

    private async ValueTask RefreshLocalizedTextsAsync(CancellationToken cancellationToken)
    {
        LocalizedText[] localizedTexts;

        lock (_localizedTextGate)
        {
            localizedTexts = _localizedTexts.ToArray();
        }

        foreach (var localizedText in localizedTexts)
        {
            try
            {
                await localizedText.RefreshAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                WriteTextRefreshFailed(localizedText.Key, exception);
            }
        }
    }

    private async ValueTask<LanguagePackageLoadResult> LoadPackageAsync(
        LanguagePackageDescriptor descriptor,
        bool cache,
        CancellationToken cancellationToken)
    {
        if (cache && _loadedPackages.TryGetValue(descriptor.PackageId, out var loadedPackage))
        {
            return LanguagePackageLoadResult.Success(loadedPackage);
        }

        if (!_providers.TryGetValue(descriptor.ProviderKind, out var provider))
        {
            return LanguagePackageLoadResult.Failed(
                new LocalizationError(
                    LocalizationErrorKind.PackageNotFound,
                    $"No language package provider is registered for '{descriptor.ProviderKind}'."));
        }

        var loadResult = await provider.LoadAsync(descriptor, cancellationToken).ConfigureAwait(false);

        if (cache && loadResult.Succeeded)
        {
            _loadedPackages[descriptor.PackageId] = loadResult.Package!;
        }

        return loadResult;
    }

    private IEnumerable<LanguagePackageDescriptor> GetDescriptors(CultureInfo culture)
    {
        return _descriptors.Where(descriptor =>
            string.Equals(descriptor.Culture.Name, culture.Name, StringComparison.OrdinalIgnoreCase));
    }

    private IEnumerable<CultureInfo> GetFallbackCultures(CultureInfo culture)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var fallbackCulture in GetDescriptors(culture)
                     .Select(descriptor => descriptor.FallbackCulture)
                     .Where(fallbackCulture => fallbackCulture is not null)
                     .Cast<CultureInfo>())
        {
            if (seen.Add(fallbackCulture.Name))
            {
                yield return fallbackCulture;
            }
        }

        var parent = culture.Parent;
        while (!string.IsNullOrEmpty(parent.Name))
        {
            if (seen.Add(parent.Name))
            {
                yield return parent;
            }

            parent = parent.Parent;
        }

        if (seen.Add(CultureInfo.InvariantCulture.Name))
        {
            yield return CultureInfo.InvariantCulture;
        }
    }

    private void WritePackageLoadFailed(
        LanguagePackageDescriptor descriptor,
        LocalizationError? error)
    {
        WriteDiagnostic(
            LocalizationDiagnosticIds.PackageLoadFailed,
            error?.Message ?? $"Language package '{descriptor.PackageId}' failed to load.",
            LocalizationDiagnosticSeverity.Error,
            cultureName: descriptor.Culture.Name,
            packageId: descriptor.PackageId,
            scope: descriptor.Scope,
            errorKind: error?.Kind);
    }

    private void WriteDiagnostic(
        string code,
        string message,
        LocalizationDiagnosticSeverity severity,
        string? cultureName = null,
        string? resourceKey = null,
        string? packageId = null,
        ResourceScope? scope = null,
        LocalizationErrorKind? errorKind = null)
    {
        _diagnostics?.Write(
            new LocalizationDiagnosticRecord(
                code,
                message,
                severity,
                CultureName: cultureName,
                ResourceKey: resourceKey,
                PackageId: packageId,
                Scope: scope,
                CultureRevision: State.Revision,
                ErrorKind: errorKind));
    }

    private static void DisposeAll(IEnumerable<LanguagePackage> packages)
    {
        foreach (var package in packages)
        {
            package.Dispose();
        }
    }

    private sealed class NoopPresentationLocalizationBridge : IPresentationLocalizationBridge
    {
        public static readonly NoopPresentationLocalizationBridge Instance = new();

        private NoopPresentationLocalizationBridge()
        {
        }

        public ValueTask<LocalizationResult> ApplyCultureAsync(
            CultureState state,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(LocalizationResult.Success());
        }
    }
}
