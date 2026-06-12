using System.Diagnostics;
using AtomUI.City.Diagnostics;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class ViewFactory
{
    private readonly IUiDispatcher _dispatcher;
    private readonly IServiceProvider _services;
    private readonly IHostDiagnostics? _diagnostics;

    public ViewFactory(IUiDispatcher dispatcher)
        : this(dispatcher, EmptyServiceProvider.Instance, diagnostics: null)
    {
    }

    public ViewFactory(IUiDispatcher dispatcher, IServiceProvider services)
        : this(dispatcher, services, diagnostics: null)
    {
    }

    public ViewFactory(IUiDispatcher dispatcher, IHostDiagnostics? diagnostics)
        : this(dispatcher, EmptyServiceProvider.Instance, diagnostics)
    {
    }

    public ViewFactory(
        IUiDispatcher dispatcher,
        IServiceProvider services,
        IHostDiagnostics? diagnostics)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(services);

        _dispatcher = dispatcher;
        _services = services;
        _diagnostics = diagnostics;
    }

    public async ValueTask<object> CreateAsync(
        ViewDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var view = await _dispatcher.InvokeAsync(
                () => descriptor.CreateView(new ViewFactoryContext(_services)),
                cancellationToken);
            stopwatch.Stop();
            WriteCreatedDiagnostic(descriptor, stopwatch.Elapsed);

            return view;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            WriteCreationFailedDiagnostic(descriptor, stopwatch.Elapsed, exception);

            throw;
        }
    }

    private void WriteCreatedDiagnostic(ViewDescriptor descriptor, TimeSpan elapsed)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.ViewCreated,
            $"View factory created view '{descriptor.ViewType.FullName}' for view model '{descriptor.ViewModelType.FullName}' in {elapsed.TotalMilliseconds:0.###} ms.",
            HostDiagnosticSeverity.Info));
    }

    private void WriteCreationFailedDiagnostic(
        ViewDescriptor descriptor,
        TimeSpan elapsed,
        Exception exception)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.ViewCreationFailed,
            $"View factory failed to create view '{descriptor.ViewType.FullName}' for view model '{descriptor.ViewModelType.FullName}' in {elapsed.TotalMilliseconds:0.###} ms: {exception.Message}",
            HostDiagnosticSeverity.Error));
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public static EmptyServiceProvider Instance { get; } = new();

        public object? GetService(Type serviceType)
        {
            return null;
        }
    }
}
