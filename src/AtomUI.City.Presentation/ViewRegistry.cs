using AtomUI.City.Diagnostics;

namespace AtomUI.City.Presentation;

public sealed class ViewRegistry : IViewRegistry
{
    private readonly Dictionary<ViewRegistrationKey, ViewDescriptor> _descriptors = new();
    private readonly IHostDiagnostics? _diagnostics;

    public ViewRegistry()
    {
    }

    public ViewRegistry(IHostDiagnostics diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        _diagnostics = diagnostics;
    }

    public void Register(ViewDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var key = ViewRegistrationKey.Create(descriptor.ViewModelType, descriptor.ViewKey);

        if (_descriptors.ContainsKey(key))
        {
            throw new PresentationException(
                PresentationError.DuplicateView,
                $"View model '{descriptor.ViewModelType.FullName}' already has a view registered for key '{key.ViewKey}'.");
        }

        _descriptors.Add(key, descriptor);
    }

    public int RevokePlugin(string pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);

        return Revoke(descriptor => string.Equals(descriptor.PluginId, pluginId, StringComparison.Ordinal));
    }

    public int RevokeContribution(string contributionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contributionId);

        return Revoke(descriptor => string.Equals(descriptor.ContributionId, contributionId, StringComparison.Ordinal));
    }

    private int Revoke(Func<ViewDescriptor, bool> predicate)
    {
        var revokedKeys = _descriptors
            .Where(item => predicate(item.Value))
            .Select(item => item.Key)
            .ToArray();

        foreach (var key in revokedKeys)
        {
            _descriptors.Remove(key);
        }

        return revokedKeys.Length;
    }

    public bool TryLocate(Type viewModelType, out ViewDescriptor? descriptor)
    {
        return TryLocate(viewModelType, viewKey: null, out descriptor);
    }

    public bool TryLocate(
        Type viewModelType,
        string? viewKey,
        out ViewDescriptor? descriptor)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);

        var located = _descriptors.TryGetValue(
            ViewRegistrationKey.Create(viewModelType, viewKey),
            out descriptor);

        if (located && descriptor is not null)
        {
            WriteMatchedDiagnostic(viewModelType, viewKey, descriptor);
        }
        else
        {
            WriteFailedDiagnostic(viewModelType, viewKey);
        }

        return located;
    }

    public ViewDescriptor Locate(Type viewModelType, string? viewKey = null)
    {
        if (TryLocate(viewModelType, viewKey, out var descriptor) && descriptor is not null)
        {
            return descriptor;
        }

        throw new PresentationException(
            PresentationError.ViewNotFound,
            $"No view was registered for view model '{viewModelType.FullName}'.");
    }

    private readonly record struct ViewRegistrationKey(Type ViewModelType, string ViewKey)
    {
        public static ViewRegistrationKey Create(Type viewModelType, string? viewKey)
        {
            return new ViewRegistrationKey(
                viewModelType,
                string.IsNullOrWhiteSpace(viewKey) ? string.Empty : viewKey);
        }
    }

    private void WriteMatchedDiagnostic(
        Type viewModelType,
        string? viewKey,
        ViewDescriptor descriptor)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.ViewLocatorMatched,
            $"View locator matched view model '{viewModelType.FullName}' to view '{descriptor.ViewType.FullName}' with key '{NormalizeViewKey(viewKey)}'.",
            HostDiagnosticSeverity.Info));
    }

    private void WriteFailedDiagnostic(Type viewModelType, string? viewKey)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.ViewLocatorFailed,
            $"View locator failed for view model '{viewModelType.FullName}' with key '{NormalizeViewKey(viewKey)}'.",
            HostDiagnosticSeverity.Warning));
    }

    private static string NormalizeViewKey(string? viewKey)
    {
        return string.IsNullOrWhiteSpace(viewKey) ? "<default>" : viewKey;
    }
}
