using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class ViewFactory
{
    private readonly IUiDispatcher _dispatcher;

    public ViewFactory(IUiDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        _dispatcher = dispatcher;
    }

    public ValueTask<object> CreateAsync(
        ViewDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return _dispatcher.InvokeAsync(
            () => descriptor.CreateView(new ViewFactoryContext()),
            cancellationToken);
    }
}
