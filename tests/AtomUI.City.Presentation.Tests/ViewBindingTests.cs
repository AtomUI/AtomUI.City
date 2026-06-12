using AtomUI.City.Diagnostics;
using AtomUI.City.Presentation;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation.Tests;

public sealed class ViewBindingTests
{
    [Fact]
    public async Task ViewFactoryCreatesViewThroughUiDispatcher()
    {
        var dispatcher = new RecordingDispatcher();
        var descriptor = new ViewDescriptor(
            typeof(SettingsViewModel),
            typeof(SettingsView),
            viewKey: null,
            _ => new SettingsView());
        var factory = new ViewFactory(dispatcher);

        var view = await factory.CreateAsync(descriptor);

        Assert.IsType<SettingsView>(view);
        Assert.Equal(1, dispatcher.InvokeCount);
    }

    [Fact]
    public async Task ViewFactoryRejectsFactoryResultWithWrongViewType()
    {
        var dispatcher = new RecordingDispatcher();
        var descriptor = new ViewDescriptor(
            typeof(SettingsViewModel),
            typeof(SettingsView),
            viewKey: null,
            _ => new object());
        var factory = new ViewFactory(dispatcher);

        var exception = await Assert.ThrowsAsync<PresentationException>(
            async () => await factory.CreateAsync(descriptor));

        Assert.Equal(PresentationError.ViewCreationFailed, exception.Error);
    }

    [Fact]
    public async Task ViewFactoryRecordsCreationDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var dispatcher = new RecordingDispatcher();
        var descriptor = new ViewDescriptor(
            typeof(SettingsViewModel),
            typeof(SettingsView),
            viewKey: null,
            _ => new SettingsView());
        var factory = new ViewFactory(dispatcher, diagnostics);

        await factory.CreateAsync(descriptor);

        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ViewCreated &&
                record.Severity == HostDiagnosticSeverity.Info &&
                record.Message.Contains(typeof(SettingsViewModel).FullName!, StringComparison.Ordinal) &&
                record.Message.Contains(typeof(SettingsView).FullName!, StringComparison.Ordinal));
    }

    [Fact]
    public async Task ViewFactoryRecordsCreationFailureDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var dispatcher = new RecordingDispatcher();
        var descriptor = new ViewDescriptor(
            typeof(SettingsViewModel),
            typeof(SettingsView),
            viewKey: null,
            _ => new object());
        var factory = new ViewFactory(dispatcher, diagnostics);

        await Assert.ThrowsAsync<PresentationException>(
            async () => await factory.CreateAsync(descriptor));

        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ViewCreationFailed &&
                record.Severity == HostDiagnosticSeverity.Error &&
                record.Message.Contains(typeof(SettingsViewModel).FullName!, StringComparison.Ordinal) &&
                record.Message.Contains(typeof(SettingsView).FullName!, StringComparison.Ordinal));
    }

    [Fact]
    public async Task ViewFactoryPassesServiceProviderToViewFactoryContext()
    {
        var dependency = new ViewDependency();
        var serviceProvider = new FixedServiceProvider(dependency);
        var dispatcher = new RecordingDispatcher();
        var descriptor = new ViewDescriptor(
            typeof(SettingsViewModel),
            typeof(SettingsViewWithDependency),
            viewKey: null,
            context => new SettingsViewWithDependency(
                (ViewDependency)context.Services.GetService(typeof(ViewDependency))!));
        var factory = new ViewFactory(dispatcher, serviceProvider);

        var view = await factory.CreateAsync(descriptor);

        var typedView = Assert.IsType<SettingsViewWithDependency>(view);
        Assert.Same(dependency, typedView.Dependency);
    }

    [Fact]
    public void ViewBinderSetsDataContextAndClearsItOnDispose()
    {
        var binder = new ViewBinder();
        var descriptor = new ViewDescriptor(
            typeof(SettingsViewModel),
            typeof(SettingsView),
            viewKey: null,
            _ => new SettingsView());
        var view = new SettingsView();
        var viewModel = new SettingsViewModel();

        var handle = binder.Bind(descriptor, view, viewModel);

        Assert.Same(viewModel, view.DataContext);

        handle.Dispose();

        Assert.Null(view.DataContext);
        Assert.True(handle.IsDisposed);
    }

    [Fact]
    public void ViewBinderRejectsViewWithoutDataContextContract()
    {
        var binder = new ViewBinder();
        var descriptor = new ViewDescriptor(
            typeof(SettingsViewModel),
            typeof(object),
            viewKey: null,
            _ => new object());

        var exception = Assert.Throws<PresentationException>(
            () => binder.Bind(descriptor, new object(), new SettingsViewModel()));

        Assert.Equal(PresentationError.BindingFailed, exception.Error);
    }

    [Fact]
    public void ViewBinderRecordsBindingDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var binder = new ViewBinder(diagnostics);
        var descriptor = new ViewDescriptor(
            typeof(SettingsViewModel),
            typeof(SettingsView),
            viewKey: null,
            _ => new SettingsView());
        var view = new SettingsView();
        var viewModel = new SettingsViewModel();

        binder.Bind(descriptor, view, viewModel);

        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ViewBound &&
                record.Severity == HostDiagnosticSeverity.Info &&
                record.Message.Contains(typeof(SettingsViewModel).FullName!, StringComparison.Ordinal) &&
                record.Message.Contains(typeof(SettingsView).FullName!, StringComparison.Ordinal));
    }

    [Fact]
    public void ViewBinderRecordsBindingFailureDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var binder = new ViewBinder(diagnostics);
        var descriptor = new ViewDescriptor(
            typeof(SettingsViewModel),
            typeof(object),
            viewKey: null,
            _ => new object());

        Assert.Throws<PresentationException>(
            () => binder.Bind(descriptor, new object(), new SettingsViewModel()));

        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ViewBindingFailed &&
                record.Severity == HostDiagnosticSeverity.Error &&
                record.Message.Contains(typeof(SettingsViewModel).FullName!, StringComparison.Ordinal));
    }

    private sealed class RecordingDispatcher : IUiDispatcher
    {
        public int InvokeCount { get; private set; }

        public bool CheckAccess() => true;

        public ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default)
        {
            InvokeCount++;
            callback();

            return ValueTask.CompletedTask;
        }

        public ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)
        {
            InvokeCount++;

            return ValueTask.FromResult(callback());
        }

        public ValueTask PostAsync(
            Func<CancellationToken, ValueTask> callback,
            CancellationToken cancellationToken = default)
        {
            return callback(cancellationToken);
        }
    }

    private sealed class SettingsViewModel;

    private sealed class SettingsView : IViewDataContextAware
    {
        public object? DataContext { get; set; }
    }

    private sealed class SettingsViewWithDependency(ViewDependency dependency)
    {
        public ViewDependency Dependency { get; } = dependency;
    }

    private sealed class ViewDependency;

    private sealed class FixedServiceProvider(object service) : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            return serviceType == service.GetType() ? service : null;
        }
    }
}
