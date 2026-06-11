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
    public void ValidationScopeTracksLocalizableMessageMetadata()
    {
        var scope = new ValidationScope();

        scope.SetInvalid(
            "Name",
            "Name is required.",
            "Validation.Name.Required",
            ["Name"]);

        var message = scope.Messages["Name"][0];
        Assert.Equal(ValidationStatus.Invalid, scope.Status);
        Assert.Equal("Name is required.", scope.Errors["Name"][0]);
        Assert.Equal("Name", message.Key);
        Assert.Equal("Name is required.", message.Message);
        Assert.Equal("Validation.Name.Required", message.MessageKey);
        Assert.Equal(["Name"], message.MessageArguments);
    }

    [Fact]
    public void ValidationFailureIsDistinctFromInvalidState()
    {
        var scope = new ValidationScope();
        var exception = new InvalidOperationException("validator failed");

        scope.SetInvalid(
            "Name",
            "Name is required.",
            "Validation.Name.Required",
            ["Name"]);
        scope.SetFailed(exception);

        Assert.Equal(ValidationStatus.Failed, scope.Status);
        Assert.Same(exception, scope.Exception);
        Assert.Empty(scope.Errors);
        Assert.Empty(scope.Messages);
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
