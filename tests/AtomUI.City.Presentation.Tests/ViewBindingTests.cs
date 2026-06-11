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
}
