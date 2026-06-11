namespace AtomUI.City.Presentation;

public sealed class ViewBinder
{
    public BoundViewHandle Bind(
        ViewDescriptor descriptor,
        object view,
        object viewModel)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(viewModel);

        if (view is not IViewDataContextAware dataContextAware)
        {
            throw new PresentationException(
                PresentationError.BindingFailed,
                $"View '{view.GetType().FullName}' does not expose a Presentation data context contract.");
        }

        dataContextAware.DataContext = viewModel;

        return BoundViewHandle.Create(
            descriptor,
            view,
            viewModel,
            () => dataContextAware.DataContext = null);
    }
}
