using AtomUI.City.Data;

namespace AtomUI.City.Data.Tests;

public sealed class SignalRDataTransportTests
{
    [Fact]
    public async Task SignalRTransportInvokesHubMethod()
    {
        var transport = new SignalRDataTransport();
        var request = new SignalRDataRequest<string>(
            "notifications",
            "load-count",
            "NotificationHub",
            "GetUnreadCount",
            (_, _) => ValueTask.FromResult("42"));

        var result = await transport.SendAsync(
            request,
            DataRequestContext.Create(request, CancellationToken.None));

        Assert.True(result.Succeeded);
        Assert.Equal("42", result.Value);
    }

    [Fact]
    public async Task SignalRTransportMapsInvokeFailure()
    {
        var transport = new SignalRDataTransport();
        var request = new SignalRDataRequest<string>(
            "notifications",
            "load-count",
            "NotificationHub",
            "GetUnreadCount",
            (_, _) => throw new InvalidOperationException("hub failed"));

        var result = await transport.SendAsync(
            request,
            DataRequestContext.Create(request, CancellationToken.None));

        Assert.False(result.Succeeded);
        Assert.Equal(DataErrorKind.TransportError, result.Error?.Kind);
    }

    [Fact]
    public async Task SignalRTransportMapsInvokeTimeout()
    {
        var timeoutException = new TaskCanceledException("hub invoke timed out");
        var transport = new SignalRDataTransport();
        var request = new SignalRDataRequest<string>(
            "notifications",
            "load-count",
            "NotificationHub",
            "GetUnreadCount",
            (_, _) => throw timeoutException);

        var result = await transport.SendAsync(
            request,
            DataRequestContext.Create(request, CancellationToken.None),
            CancellationToken.None);

        Assert.Equal(DataResultStatus.Failed, result.Status);
        Assert.Equal(DataErrorKind.Timeout, result.Error?.Kind);
        Assert.Same(timeoutException, result.Error?.Exception);
    }
}
