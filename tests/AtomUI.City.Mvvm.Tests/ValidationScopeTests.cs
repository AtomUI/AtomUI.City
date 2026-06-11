using AtomUI.City.Mvvm;

namespace AtomUI.City.Mvvm.Tests;

public sealed class ValidationScopeTests
{
    [Fact]
    public void ValidationScopeStartsValidAndTracksInvalidEntries()
    {
        var scope = new ValidationScope();

        Assert.Equal(ValidationStatus.Valid, scope.Status);

        scope.SetInvalid("Name", "Name is required.");

        Assert.Equal(ValidationStatus.Invalid, scope.Status);
        Assert.Equal("Name is required.", scope.Errors["Name"][0]);
    }

    [Fact]
    public void ValidationFailureIsDistinctFromInvalidState()
    {
        var scope = new ValidationScope();
        var exception = new InvalidOperationException("validator failed");

        scope.SetFailed(exception);

        Assert.Equal(ValidationStatus.Failed, scope.Status);
        Assert.Same(exception, scope.Exception);
        Assert.Empty(scope.Errors);
    }

    [Fact]
    public void ActivationScopeDisposalCancelsValidationScope()
    {
        using var activationScope = new ActivationScope();
        var validationScope = new ValidationScope();

        validationScope.BindTo(activationScope);
        validationScope.SetPending();
        activationScope.Dispose();

        Assert.Equal(ValidationStatus.Canceled, validationScope.Status);
    }
}
