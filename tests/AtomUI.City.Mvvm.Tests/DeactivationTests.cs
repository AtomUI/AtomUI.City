using AtomUI.City.Mvvm;

namespace AtomUI.City.Mvvm.Tests;

public sealed class DeactivationTests
{
    [Fact]
    public async Task CanDeactivateContractReturnsAllowResult()
    {
        ICanDeactivate viewModel = new AllowDeactivateViewModel();

        var result = await viewModel.CanDeactivateAsync(CancellationToken.None);

        Assert.Equal(DeactivationStatus.Allow, result.Status);
    }

    [Fact]
    public async Task ConfirmDeactivateContractCanReturnCancelResult()
    {
        IConfirmDeactivate viewModel = new CancelDeactivateViewModel();

        var result = await viewModel.ConfirmDeactivateAsync(CancellationToken.None);

        Assert.Equal(DeactivationStatus.Cancel, result.Status);
        Assert.Equal("user-cancelled", result.Reason);
    }

    private sealed class AllowDeactivateViewModel : ICanDeactivate
    {
        public ValueTask<DeactivationResult> CanDeactivateAsync(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(DeactivationResult.Allow());
        }
    }

    private sealed class CancelDeactivateViewModel : IConfirmDeactivate
    {
        public ValueTask<DeactivationResult> ConfirmDeactivateAsync(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(DeactivationResult.Cancel("user-cancelled"));
        }
    }
}
