using System.Net;
using AtomUI.City.Data;

namespace AtomUI.City.Data.Tests;

public sealed class DataResultTests
{
    [Fact]
    public void SuccessResultContainsValue()
    {
        var result = DataResult<string>.Success("loaded");

        Assert.True(result.Succeeded);
        Assert.Equal(DataResultStatus.Success, result.Status);
        Assert.Equal("loaded", result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void FailedResultContainsStandardError()
    {
        var error = new DataError(DataErrorKind.NotFound, "The item was not found.");

        var result = DataResult<string>.Failed(error);

        Assert.False(result.Succeeded);
        Assert.Equal(DataResultStatus.Failed, result.Status);
        Assert.Equal(DataErrorKind.NotFound, result.Error?.Kind);
    }

    [Fact]
    public void DataErrorCarriesLocalizableMessageMetadata()
    {
        var error = new DataError(
            DataErrorKind.AuthorizationForbidden,
            "Permission 'settings.write' is required.",
            MessageKey: "Errors.AuthorizationForbidden",
            MessageArguments: ["settings.write"]);

        var result = DataResult<string>.Failed(error);

        Assert.Equal("Errors.AuthorizationForbidden", result.Error?.MessageKey);
        Assert.Equal(["settings.write"], result.Error?.MessageArguments);
    }

    [Fact]
    public void HttpStatusMappingIncludesLocalizableMessageKey()
    {
        var error = DataErrorMapper.FromHttpStatusCode(HttpStatusCode.Forbidden);

        Assert.Equal(DataErrorKind.AuthorizationForbidden, error.Kind);
        Assert.Equal("Errors.AuthorizationForbidden", error.MessageKey);
    }

    [Fact]
    public void CancelledResultIsNotFailure()
    {
        var result = DataResult<string>.Cancelled();

        Assert.False(result.Succeeded);
        Assert.Equal(DataResultStatus.Cancelled, result.Status);
        Assert.Equal(DataErrorKind.Cancelled, result.Error?.Kind);
    }

    [Fact]
    public void StaleSuppressedResultUsesDedicatedStatus()
    {
        var result = DataResult<string>.StaleSuppressed();

        Assert.False(result.Succeeded);
        Assert.Equal(DataResultStatus.StaleSuppressed, result.Status);
        Assert.Equal(DataErrorKind.Cancelled, result.Error?.Kind);
    }
}
