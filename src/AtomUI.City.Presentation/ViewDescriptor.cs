namespace AtomUI.City.Presentation;

public sealed class ViewDescriptor
{
    private readonly Func<ViewFactoryContext, object> _viewFactory;

    public ViewDescriptor(
        Type viewModelType,
        Type viewType,
        string? viewKey,
        Func<ViewFactoryContext, object> viewFactory,
        string? contributionId = null)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        ArgumentNullException.ThrowIfNull(viewType);
        ArgumentNullException.ThrowIfNull(viewFactory);

        ViewModelType = viewModelType;
        ViewType = viewType;
        ViewKey = string.IsNullOrWhiteSpace(viewKey) ? null : viewKey;
        ContributionId = string.IsNullOrWhiteSpace(contributionId) ? null : contributionId;
        _viewFactory = viewFactory;
    }

    public Type ViewModelType { get; }

    public Type ViewType { get; }

    public string? ViewKey { get; }

    public string? ContributionId { get; }

    public object CreateView(ViewFactoryContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var view = _viewFactory(context);

        if (!ViewType.IsInstanceOfType(view))
        {
            throw new PresentationException(
                PresentationError.ViewCreationFailed,
                $"View factory for '{ViewModelType.FullName}' returned '{view.GetType().FullName}', expected '{ViewType.FullName}'.");
        }

        return view;
    }
}
