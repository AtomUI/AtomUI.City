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

            return callResult.Succeeded
                ? DataResult<TResponse>.Success(callResult.Value!)
                : DataResult<TResponse>.Failed(DataErrorMapper.FromGrpcStatus(callResult.StatusCode, callResult.Detail));
        }
        catch (OperationCanceledException)
        {
            return DataResult<TResponse>.Cancelled();
        }
    }
}
