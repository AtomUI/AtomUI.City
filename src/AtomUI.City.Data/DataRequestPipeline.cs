using AtomUI.City.Lifecycle;

namespace AtomUI.City.Data;

public sealed class DataRequestPipeline : IDataRequestPipeline
{
    private readonly IReadOnlyDictionary<DataTransportKind, IRequestResponseTransport> _transports;
    private readonly IDataCredentialProvider? _credentialProvider;
    private readonly IDataDiagnostics? _diagnostics;
    private readonly IDataRequestCache? _cache;

    public DataRequestPipeline(
        IRequestResponseTransport transport,
        IDataCredentialProvider? credentialProvider = null,
        IDataDiagnostics? diagnostics = null,
        IDataRequestCache? cache = null)
        : this([transport], credentialProvider, diagnostics, cache)
    {
    }

    public DataRequestPipeline(
        IEnumerable<IRequestResponseTransport> transports,
        IDataCredentialProvider? credentialProvider = null,
        IDataDiagnostics? diagnostics = null,
        IDataRequestCache? cache = null)
    {
        ArgumentNullException.ThrowIfNull(transports);

        _transports = transports.ToDictionary(transport => transport.Kind);
        _credentialProvider = credentialProvider;
        _diagnostics = diagnostics;
        _cache = cache;
    }

    public async ValueTask<DataResult<TResponse>> SendAsync<TResponse>(
        DataRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (cancellationToken.IsCancellationRequested)
        {
            return DataResult<TResponse>.Cancelled();
        }

        if (!CanUseParentScope(request.ParentScope))
        {
            return DataResult<TResponse>.StaleSuppressed();
        }

        using var timeoutCancellation = CreateTimeoutCancellation(request.Resilience, cancellationToken);
        using var operationCancellation = request.ParentScope is null
            ? CancellationTokenSource.CreateLinkedTokenSource(timeoutCancellation.Token)
            : CancellationTokenSource.CreateLinkedTokenSource(timeoutCancellation.Token, request.ParentScope.CancellationToken);
        var operationToken = operationCancellation.Token;
        var context = DataRequestContext.Create(request, operationToken);

        var credentialResult = await ResolveCredentialAsync(request, context, operationToken).ConfigureAwait(false);
        if (credentialResult is not null)
        {
            WriteRequestResultDiagnostic(context, credentialResult);

            return credentialResult;
        }

        var cacheKey = CreateCacheKey(request, context);
        if (cacheKey is not null)
        {
            var cachedResult = await ReadCacheAsync<TResponse>(cacheKey, operationToken).ConfigureAwait(false);
            if (cachedResult is not null)
            {
                WriteRequestResultDiagnostic(context, cachedResult);

                return cachedResult;
            }
        }

        var maxAttempts = GetMaxAttempts(request);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            context.Attempt = attempt;

            try
            {
                if (!_transports.TryGetValue(request.TransportKind, out var transport))
                {
                    var missingTransportResult = DataResult<TResponse>.Failed(
                        new DataError(
                            DataErrorKind.PolicyRejected,
                            $"No data transport is registered for '{request.TransportKind}'."));
                    WriteRequestResultDiagnostic(context, missingTransportResult);

                    return missingTransportResult;
                }

                var result = await transport
                    .SendAsync(request, context, operationToken)
                    .ConfigureAwait(false);

                if (ShouldSuppress(request))
                {
                    var suppressedResult = DataResult<TResponse>.StaleSuppressed();
                    WriteRequestResultDiagnostic(context, suppressedResult);

                    return suppressedResult;
                }

                if (result.Succeeded || !ShouldRetry(request, result, attempt, maxAttempts))
                {
                    await WriteCacheAsync(cacheKey, result, operationToken).ConfigureAwait(false);
                    WriteRequestResultDiagnostic(context, result);

                    return result;
                }

                WriteDiagnostic(
                    DataDiagnosticIds.RequestRetry,
                    $"Data operation '{request.OperationName}' retry attempt {attempt}.");
            }
            catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                var timeoutResult = DataResult<TResponse>.Failed(
                    new DataError(DataErrorKind.Timeout, "Data operation timed out."));
                WriteRequestResultDiagnostic(context, timeoutResult);

                return timeoutResult;
            }
            catch (OperationCanceledException)
            {
                var cancelledResult = DataResult<TResponse>.Cancelled();
                WriteRequestResultDiagnostic(context, cancelledResult);

                return cancelledResult;
            }
            catch (Exception exception)
            {
                var failedResult = DataResult<TResponse>.Failed(
                    new DataError(
                        DataErrorKind.TransportError,
                        exception.Message,
                        Exception: exception));
                WriteRequestResultDiagnostic(context, failedResult);

                return failedResult;
            }
        }

        var emptyResult = DataResult<TResponse>.Failed(
            new DataError(DataErrorKind.Unknown, "Data operation did not produce a result."));
        WriteRequestResultDiagnostic(context, emptyResult);

        return emptyResult;
    }

    private async ValueTask<DataResult<TResponse>?> ResolveCredentialAsync<TResponse>(
        DataRequest<TResponse> request,
        DataRequestContext context,
        CancellationToken cancellationToken)
    {
        if (request.Authentication.Mode == DataAuthenticationMode.Anonymous)
        {
            return null;
        }

        if (_credentialProvider is null)
        {
            return DataResult<TResponse>.Failed(
                new DataError(
                    DataErrorKind.CredentialUnavailable,
                    "No data credential provider is registered."));
        }

        DataCredentialResult credentialResult;

        try
        {
            credentialResult = await _credentialProvider
                .GetCredentialAsync(
                    new DataAuthenticationContext(
                        request.ClientId,
                        request.OperationName,
                        request.Authentication),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return DataResult<TResponse>.Cancelled();
        }
        catch (Exception exception)
        {
            return DataResult<TResponse>.Failed(
                new DataError(
                    DataErrorKind.CredentialUnavailable,
                    exception.Message,
                    Exception: exception));
        }

        switch (credentialResult.Status)
        {
            case DataCredentialResultStatus.None:
                return null;
            case DataCredentialResultStatus.Success:
                context.SetCredential(credentialResult.Credential!);
                return null;
            case DataCredentialResultStatus.Required:
                return DataResult<TResponse>.Failed(
                    new DataError(
                        DataErrorKind.AuthenticationRequired,
                        credentialResult.Message ?? "Authentication is required."));
            case DataCredentialResultStatus.Expired:
                return DataResult<TResponse>.Failed(
                    new DataError(
                        DataErrorKind.AuthenticationExpired,
                        credentialResult.Message ?? "Authentication has expired."));
            case DataCredentialResultStatus.Cancelled:
                return DataResult<TResponse>.Cancelled();
            default:
                return DataResult<TResponse>.Failed(
                    new DataError(
                        DataErrorKind.CredentialUnavailable,
                        credentialResult.Message ?? "Credential is unavailable."));
        }
    }

    private static CancellationTokenSource CreateTimeoutCancellation(
        DataResilienceOptions resilience,
        CancellationToken cancellationToken)
    {
        var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        if (resilience.Timeout is { } timeout)
        {
            cancellation.CancelAfter(timeout);
        }

        return cancellation;
    }

    private static bool CanUseParentScope(LifecycleScope? parentScope)
    {
        return parentScope is null || parentScope.State == LifecycleScopeState.Running;
    }

    private static bool ShouldSuppress<TResponse>(DataRequest<TResponse> request)
    {
        return request.ParentScope is not null && request.ParentScope.State != LifecycleScopeState.Running;
    }

    private static int GetMaxAttempts<TResponse>(DataRequest<TResponse> request)
    {
        return Math.Max(1, request.Resilience.MaxRetryAttempts + 1);
    }

    private static bool ShouldRetry<TResponse>(
        DataRequest<TResponse> request,
        DataResult<TResponse> result,
        int attempt,
        int maxAttempts)
    {
        if (attempt >= maxAttempts || result.Error is null)
        {
            return false;
        }

        if (!IsRetryAllowedForAccessMode(request))
        {
            return false;
        }

        return result.Error.Kind is
            DataErrorKind.NetworkUnavailable or
            DataErrorKind.ServiceUnavailable or
            DataErrorKind.Timeout or
            DataErrorKind.TransportError or
            DataErrorKind.ServerError or
            DataErrorKind.DeadlineExceeded or
            DataErrorKind.Unavailable;
    }

    private static bool IsRetryAllowedForAccessMode<TResponse>(DataRequest<TResponse> request)
    {
        return request.AccessMode != DataAccessMode.Mutation
            || request.Resilience.AllowMutationRetry
            || !string.IsNullOrWhiteSpace(request.IdempotencyKey);
    }

    private DataCacheKey? CreateCacheKey<TResponse>(
        DataRequest<TResponse> request,
        DataRequestContext context)
    {
        if (_cache is null || request.AccessMode != DataAccessMode.Query || !request.Cache.IsEnabled)
        {
            return null;
        }

        return DataCacheKey.Create(request, GetAuthenticationScheme(request, context));
    }

    private static string GetAuthenticationScheme<TResponse>(
        DataRequest<TResponse> request,
        DataRequestContext context)
    {
        return context.Credential?.Scheme ?? request.Authentication.Mode.ToString();
    }

    private async ValueTask<DataResult<TResponse>?> ReadCacheAsync<TResponse>(
        DataCacheKey cacheKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var lookup = await _cache!
                .TryGetAsync<TResponse>(cacheKey, cancellationToken)
                .ConfigureAwait(false);

            return lookup.IsHit ? DataResult<TResponse>.Success(lookup.Value!) : null;
        }
        catch (OperationCanceledException)
        {
            return DataResult<TResponse>.Cancelled();
        }
        catch
        {
            return null;
        }
    }

    private async ValueTask WriteCacheAsync<TResponse>(
        DataCacheKey? cacheKey,
        DataResult<TResponse> result,
        CancellationToken cancellationToken)
    {
        if (cacheKey is null || !result.Succeeded)
        {
            return;
        }

        try
        {
            await _cache!
                .SetAsync(cacheKey, result.Value, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
        }
    }

    private void WriteDiagnostic(string code, string message)
    {
        _diagnostics?.Write(new DataDiagnosticRecord(code, message, DataDiagnosticSeverity.Trace));
    }

    private void WriteRequestResultDiagnostic<TResponse>(
        DataRequestContext context,
        DataResult<TResponse> result)
    {
        var succeeded = result.Succeeded;

        _diagnostics?.Write(new DataDiagnosticRecord(
            succeeded ? DataDiagnosticIds.RequestCompleted : DataDiagnosticIds.RequestFailed,
            succeeded
                ? $"Data operation '{context.OperationName}' completed."
                : $"Data operation '{context.OperationName}' failed.",
            succeeded ? DataDiagnosticSeverity.Trace : DataDiagnosticSeverity.Warning,
            context.OperationId,
            context.ClientId,
            context.OperationName,
            context.TransportKind,
            context.Attempt,
            result.Error?.Kind));
    }
}
