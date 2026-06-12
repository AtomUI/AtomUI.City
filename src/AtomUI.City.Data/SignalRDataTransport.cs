namespace AtomUI.City.Data;

public sealed class SignalRDataTransport : IRequestResponseTransport
{
    public DataTransportKind Kind => DataTransportKind.SignalR;

    public async ValueTask<DataResult<TResponse>> SendAsync<TResponse>(
        DataRequest<TResponse> request,
        DataRequestContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        if (request is not SignalRDataRequest<TResponse> signalRRequest)
        {
            return DataResult<TResponse>.Failed(
                new DataError(
                    DataErrorKind.PolicyRejected,
                    "SignalR transport requires a SignalR data request."));
        }

        try
        {
            var response = await signalRRequest
                .Invoker(
                    new SignalRInvocationContext(
                        signalRRequest.HubName,
                        signalRRequest.MethodName,
                        context),
                    cancellationToken)
                .ConfigureAwait(false);

            return DataResult<TResponse>.Success(response);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            return DataResult<TResponse>.Failed(
                new DataError(
                    DataErrorKind.Timeout,
                    exception.Message,
                    Exception: exception));
        }
        catch (OperationCanceledException)
        {
            return DataResult<TResponse>.Cancelled();
        }
        catch (Exception exception)
        {
            return DataResult<TResponse>.Failed(
                new DataError(
                    DataErrorKind.TransportError,
                    exception.Message,
                    Exception: exception));
        }
    }
}
