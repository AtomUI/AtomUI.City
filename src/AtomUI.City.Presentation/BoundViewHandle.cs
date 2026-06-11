namespace AtomUI.City.Presentation;

public sealed class BoundViewHandle : IDisposable
{
    private readonly Action? _dispose;

    private BoundViewHandle(
        ViewDescriptor? descriptor,
        object view,
        object viewModel,
        Action? dispose)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(viewModel);

        Descriptor = descriptor;
        View = view;
        ViewModel = viewModel;
        _dispose = dispose;
    }

    public ViewDescriptor? Descriptor { get; }

    public object View { get; }

    public object ViewModel { get; }

    public bool IsDisposed { get; private set; }

    public static BoundViewHandle FromExisting(
        object view,
        object viewModel,
        Action? dispose = null)
    {
        return new BoundViewHandle(
            descriptor: null,
            view,
            viewModel,
            dispose);
    }

    internal static BoundViewHandle Create(
        ViewDescriptor descriptor,
        object view,
        object viewModel,
        Action? dispose)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return new BoundViewHandle(
            descriptor,
            view,
            viewModel,
            dispose);
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
        _dispose?.Invoke();
    }
}
