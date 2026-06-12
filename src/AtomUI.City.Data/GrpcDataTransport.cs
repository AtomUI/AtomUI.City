namespace AtomUI.City.Data;

public sealed class GrpcDataTransport : IRequestResponseTransport
{
    public DataTransportKind Kind => DataTransportKind.Grpc;

    public async ValueTask<DataResult<TResponse>> SendAsync<TResponse>(
        DataRequest<TResponse> request,
        DataRequestContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        if (request is not GrpcDataRequest<TResponse> grpcRequest)
        {
            return DataResult<TResponse>.Failed(
                new DataError(
                    DataErrorKind.PolicyRejected,
                    "gRPC transport requires a gRPC data request."));
        }

        try
        {
            var callResult = await grpcRequest
                .Invoker(new GrpcRequestContext(context), cancellationToken)
                .ConfigureAwait(false);

            if (callResult.Succeeded)
            {
                return DataResult<TResponse>.Success(callResult.Value!);
            }

            var error = DataErrorMapper.FromGrpcStatus(callResult.StatusCode, callResult.Detail);

            return error.Kind == DataErrorKind.Cancelled
                ? DataResult<TResponse>.Cancelled(error.Message)
                : DataResult<TResponse>.Failed(error);
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
