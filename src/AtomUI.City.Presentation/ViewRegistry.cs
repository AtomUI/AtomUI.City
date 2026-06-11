namespace AtomUI.City.Presentation;

public sealed class ViewRegistry : IViewLocator
{
    private readonly Dictionary<ViewRegistrationKey, ViewDescriptor> _descriptors = new();

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

    public void RevokeContribution(string contributionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contributionId);

        var revokedKeys = _descriptors
            .Where(item => string.Equals(item.Value.ContributionId, contributionId, StringComparison.Ordinal))
            .Select(item => item.Key)
            .ToArray();

        foreach (var key in revokedKeys)
        {
            _descriptors.Remove(key);
        }
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

        return _descriptors.TryGetValue(
            ViewRegistrationKey.Create(viewModelType, viewKey),
            out descriptor);
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
}
