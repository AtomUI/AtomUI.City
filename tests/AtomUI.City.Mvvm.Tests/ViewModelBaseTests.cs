using AtomUI.City.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AtomUI.City.Mvvm.Tests;

public sealed class ViewModelBaseTests
{
    [Fact]
    public async Task ActivateAndDeactivateCallTemplateMethods()
    {
        using var scope = new ActivationScope();
        var viewModel = new TestViewModel();

        await viewModel.ActivateAsync(scope);
        await viewModel.DeactivateAsync();

        Assert.Equal(1, viewModel.ActivatedCount);
        Assert.Equal(1, viewModel.DeactivatedCount);
        Assert.IsAssignableFrom<ObservableValidator>(viewModel);
    }

    [Fact]
    public async Task ActivateAndDeactivateUpdateStateAndCurrentScope()
    {
        using var scope = new ActivationScope();
        var viewModel = new TestViewModel();

        Assert.Equal(ActivationState.Constructed, viewModel.ActivationState);
        Assert.False(viewModel.IsActive);

        await viewModel.ActivateAsync(new ActivationContext(scope, "settings-route"));

        Assert.Equal(ActivationState.Active, viewModel.ActivationState);
        Assert.True(viewModel.IsActive);
        Assert.Same(scope, viewModel.CurrentActivationScope);
        Assert.Equal("settings-route", viewModel.ActivationContext?.Source);

        await viewModel.DeactivateAsync();

        Assert.Equal(ActivationState.Deactivated, viewModel.ActivationState);
        Assert.False(viewModel.IsActive);
        Assert.Null(viewModel.CurrentActivationScope);
        Assert.True(scope.CancellationToken.IsCancellationRequested);
    }

    [Fact]
    public async Task DeactivateDisposesActivationResourcesForPresentationBindings()
    {
        using var scope = new ActivationScope();
        var binding = new TestDisposable();
        var viewModel = new TestViewModel();

        scope.Add(binding);
        await viewModel.ActivateAsync(new ActivationContext(scope));
        await viewModel.DeactivateAsync();

        Assert.True(binding.IsDisposed);
    }

    [Fact]
    public void ActivationScopeAccessorRestoresPreviousScope()
    {
        var accessor = new ActivationScopeAccessor();
        using var first = new ActivationScope();
        using var second = new ActivationScope();

        using (accessor.Push(first))
        {
            Assert.Same(first, accessor.Current);

            using (accessor.Push(second))
            {
                Assert.Same(second, accessor.Current);
            }

            Assert.Same(first, accessor.Current);
        }

        Assert.Null(accessor.Current);
    }

    public sealed class TestViewModel : ViewModelBase
    {
        public int ActivatedCount { get; private set; }

        public int DeactivatedCount { get; private set; }

        protected override ValueTask OnActivatedAsync(IActivationScope scope)
        {
            ActivatedCount++;
            return ValueTask.CompletedTask;
        }

        protected override ValueTask OnDeactivatedAsync()
        {
            DeactivatedCount++;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class TestDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
