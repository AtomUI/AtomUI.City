using AtomUI.City.Data;

namespace AtomUI.City.Data.Tests;

public sealed class GrpcDataTransportTests
{
    [Fact]
    public async Task GrpcTransportExecutesUnaryInvoker()
    {
        var transport = new GrpcDataTransport();
        var request = new GrpcDataRequest<string>(
            "catalog",
            "get-items",
            (_, _) => ValueTask.FromResult(GrpcCallResult<string>.Success("grpc-response")));

        var result = await transport.SendAsync(
            request,
            DataRequestContext.Create(request, CancellationToken.None));

        Assert.True(result.Succeeded);
        Assert.Equal("grpc-response", result.Value);
    }

    [Theory]
    [InlineData(GrpcStatusCode.Unauthenticated, DataErrorKind.AuthenticationRequired)]
    [InlineData(GrpcStatusCode.PermissionDenied, DataErrorKind.AuthorizationForbidden)]
    [InlineData(GrpcStatusCode.DeadlineExceeded, DataErrorKind.DeadlineExceeded)]
    [InlineData(GrpcStatusCode.Unavailable, DataErrorKind.ServiceUnavailable)]
    [InlineData(GrpcStatusCode.NotFound, DataErrorKind.NotFound)]
    public async Task GrpcTransportMapsStatusCodes(GrpcStatusCode statusCode, DataErrorKind expectedError)
    {
        var transport = new GrpcDataTransport();
        var request = new GrpcDataRequest<string>(
            "catalog",
            "get-items",
            (_, _) => ValueTask.FromResult(GrpcCallResult<string>.Failed(statusCode, "grpc failed")));

        var result = await transport.SendAsync(
            request,
            DataRequestContext.Create(request, CancellationToken.None));

        Assert.False(result.Succeeded);
        Assert.Equal(expectedError, result.Error?.Kind);
    }

    [Fact]
    public async Task GrpcTransportMapsCancellation()
    {
        var transport = new GrpcDataTransport();
        var request = new GrpcDataRequest<string>(
            "catalog",
            "get-items",
            (_, _) => throw new OperationCanceledException());

        var result = await transport.SendAsync(
            request,
            DataRequestContext.Create(request, CancellationToken.None));

        Assert.Equal(DataResultStatus.Cancelled, result.Status);
        Assert.Equal(DataErrorKind.Cancelled, result.Error?.Kind);
    }
}
