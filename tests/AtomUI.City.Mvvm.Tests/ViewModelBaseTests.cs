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
}
