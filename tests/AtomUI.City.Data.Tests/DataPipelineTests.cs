using AtomUI.City.Data;
using AtomUI.City.Lifecycle;

namespace AtomUI.City.Data.Tests;

public sealed class DataPipelineTests
{
    [Fact]
    public async Task PipelineExecutesRequestThroughTransport()
    {
        var transport = new RecordingTransport(_ => DataResult<string>.Success("response"));
        var pipeline = new DataRequestPipeline(transport);
        var request = new DataRequest<string>(
            "catalog",
            "get-items",
            DataTransportKind.Http);

        var result = await pipeline.SendAsync(request);

        Assert.True(result.Succeeded);
        Assert.Equal("response", result.Value);
        Assert.Equal(1, transport.Attempts);
        Assert.Equal("catalog", transport.LastContext?.ClientId);
        Assert.Equal("get-items", transport.LastContext?.OperationName);
    }

    [Fact]
    public async Task PipelineWritesCompletedDiagnosticWithRequestMetadata()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        var transport = new RecordingTransport(_ => DataResult<string>.Success("response"));
        var pipeline = new DataRequestPipeline(transport, diagnostics: diagnostics);
        var request = new DataRequest<string>(
            "catalog",
            "get-items",
            DataTransportKind.Http);

        await pipeline.SendAsync(request);

        var record = Assert.Single(
            diagnostics.Records,
            record => record.Code == DataDiagnosticIds.RequestCompleted);
        Assert.Equal("catalog", record.ClientId);
        Assert.Equal("get-items", record.OperationName);
        Assert.Equal(DataTransportKind.Http, record.TransportKind);
        Assert.Equal(1, record.Attempt);
        Assert.NotEqual(Guid.Empty, record.OperationId);
    }

    [Fact]
    public async Task PipelineWritesFailedDiagnosticWithErrorMetadata()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        var transport = new RecordingTransport(_ =>
            DataResult<string>.Failed(new DataError(DataErrorKind.ServiceUnavailable, "down")));
        var pipeline = new DataRequestPipeline(transport, diagnostics: diagnostics);
        var request = new DataRequest<string>(
            "catalog",
            "get-items",
            DataTransportKind.Http);

        await pipeline.SendAsync(request);

        var record = Assert.Single(
            diagnostics.Records,
            record => record.Code == DataDiagnosticIds.RequestFailed);
        Assert.Equal("catalog", record.ClientId);
        Assert.Equal("get-items", record.OperationName);
        Assert.Equal(DataTransportKind.Http, record.TransportKind);
        Assert.Equal(1, record.Attempt);
        Assert.Equal(DataErrorKind.ServiceUnavailable, record.ErrorKind);
        Assert.NotEqual(Guid.Empty, record.OperationId);
    }

    [Fact]
    public async Task PipelineReturnsCachedQueryResultWhenCacheHits()
    {
        var cache = new RecordingRequestCache();
        var transport = new RecordingTransport(_ => DataResult<string>.Success("transport"));
        var pipeline = new DataRequestPipeline(transport, cache: cache);
        var request = new DataRequest<string>(
            "catalog",
            "get-items",
            DataTransportKind.Http)
        {
            Cache = DataCacheOptions.Enabled("items:v1"),
        };
        cache.Store(DataCacheKey.Create(request, "Anonymous"), "cached");

        var result = await pipeline.SendAsync(request);

        Assert.True(result.Succeeded);
        Assert.Equal("cached", result.Value);
        Assert.Equal(0, transport.Attempts);
        Assert.Equal(1, cache.Reads);
    }

    [Fact]
    public async Task PipelineWritesSuccessfulQueryResultToCacheWhenCacheMisses()
    {
        var cache = new RecordingRequestCache();
        var transport = new RecordingTransport(_ => DataResult<string>.Success("transport"));
        var pipeline = new DataRequestPipeline(transport, cache: cache);
        var request = new DataRequest<string>(
            "catalog",
            "get-items",
            DataTransportKind.Http)
        {
            Cache = DataCacheOptions.Enabled("items:v1"),
        };

        var result = await pipeline.SendAsync(request);

        var key = DataCacheKey.Create(request, "Anonymous");
        Assert.True(result.Succeeded);
        Assert.Equal("transport", result.Value);
        Assert.Equal(1, transport.Attempts);
        Assert.Equal(1, cache.Reads);
        Assert.Equal(1, cache.Writes);
        Assert.Equal("transport", cache.Get<string>(key));
    }

    [Fact]
    public async Task PipelineWritesCacheReadFailureDiagnosticAndContinuesToTransport()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        var cache = new ThrowingRequestCache(readException: new InvalidOperationException("cache offline"));
        var transport = new RecordingTransport(_ => DataResult<string>.Success("transport"));
        var pipeline = new DataRequestPipeline(transport, diagnostics: diagnostics, cache: cache);
        var request = new DataRequest<string>(
            "catalog",
            "get-items",
            DataTransportKind.Http)
        {
            Cache = DataCacheOptions.Enabled("items:v1"),
        };

        var result = await pipeline.SendAsync(request);

        var record = Assert.Single(
            diagnostics.Records,
            record => record.Code == DataDiagnosticIds.CacheReadFailed);
        Assert.True(result.Succeeded);
        Assert.Equal("transport", result.Value);
        Assert.Equal(1, transport.Attempts);
        Assert.Equal("catalog", record.ClientId);
        Assert.Equal("get-items", record.OperationName);
        Assert.Equal(DataTransportKind.Http, record.TransportKind);
    }

    [Fact]
    public async Task PipelineWritesCacheWriteFailureDiagnosticAndReturnsTransportResult()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        var cache = new ThrowingRequestCache(writeException: new InvalidOperationException("cache readonly"));
        var transport = new RecordingTransport(_ => DataResult<string>.Success("transport"));
        var pipeline = new DataRequestPipeline(transport, diagnostics: diagnostics, cache: cache);
        var request = new DataRequest<string>(
            "catalog",
            "get-items",
            DataTransportKind.Http)
        {
            Cache = DataCacheOptions.Enabled("items:v1"),
        };

        var result = await pipeline.SendAsync(request);

        var record = Assert.Single(
            diagnostics.Records,
            record => record.Code == DataDiagnosticIds.CacheWriteFailed);
        Assert.True(result.Succeeded);
        Assert.Equal("transport", result.Value);
        Assert.Equal(1, transport.Attempts);
        Assert.Equal("catalog", record.ClientId);
        Assert.Equal("get-items", record.OperationName);
        Assert.Equal(DataTransportKind.Http, record.TransportKind);
        Assert.Equal(1, record.Attempt);
    }

    [Fact]
    public async Task PipelineSelectsTransportByRequestKind()
    {
        var httpTransport = new RecordingTransport(_ => DataResult<string>.Success("http"));
        var grpcTransport = new RecordingTransport(
            _ => DataResult<string>.Success("grpc"),
            DataTransportKind.Grpc);
        var pipeline = new DataRequestPipeline([httpTransport, grpcTransport]);
        var request = new DataRequest<string>(
            "catalog",
            "get-items",
            DataTransportKind.Grpc);

        var result = await pipeline.SendAsync(request);

        Assert.True(result.Succeeded);
        Assert.Equal("grpc", result.Value);
        Assert.Equal(0, httpTransport.Attempts);
        Assert.Equal(1, grpcTransport.Attempts);
    }

    [Fact]
    public async Task PipelineInjectsBearerCredentialBeforeTransport()
    {
        var credentials = new RecordingCredentialProvider(
            DataCredentialResult.Success(DataCredential.Bearer("access-token")));
        var transport = new RecordingTransport(context =>
        {
            Assert.Equal("Bearer", context.Credential?.Scheme);
            Assert.Equal("access-token", context.Credential?.Parameter);

            return DataResult<string>.Success("authorized");
        });
        var pipeline = new DataRequestPipeline(transport, credentialProvider: credentials);
        var request = new DataRequest<string>(
            "catalog",
            "get-items",
            DataTransportKind.Http)
        {
            Authentication = DataAuthenticationOptions.Bearer(),
        };

        var result = await pipeline.SendAsync(request);

        Assert.True(result.Succeeded);
        Assert.Equal(1, credentials.Requests);
    }

    [Fact]
    public async Task PipelineDoesNotRequestCredentialForAnonymousOperation()
    {
        var credentials = new RecordingCredentialProvider(
            DataCredentialResult.Success(DataCredential.Bearer("access-token")));
        var transport = new RecordingTransport(_ => DataResult<string>.Success("anonymous"));
        var pipeline = new DataRequestPipeline(transport, credentialProvider: credentials);
        var request = new DataRequest<string>(
            "catalog",
            "public-items",
            DataTransportKind.Http);

        var result = await pipeline.SendAsync(request);

        Assert.True(result.Succeeded);
        Assert.Equal(0, credentials.Requests);
    }

    [Fact]
    public async Task PipelineMapsCredentialUnavailableToAuthenticationRequired()
    {
        var credentials = new RecordingCredentialProvider(DataCredentialResult.Required("login required"));
        var transport = new RecordingTransport(_ => DataResult<string>.Success("should-not-run"));
        var pipeline = new DataRequestPipeline(transport, credentialProvider: credentials);
        var request = new DataRequest<string>(
            "catalog",
            "secure-items",
            DataTransportKind.Http)
        {
            Authentication = DataAuthenticationOptions.Bearer(),
        };

        var result = await pipeline.SendAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(DataErrorKind.AuthenticationRequired, result.Error?.Kind);
        Assert.Equal(0, transport.Attempts);
    }

    [Fact]
    public async Task PipelineWritesFailedDiagnosticWhenCredentialResolutionFails()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        var credentials = new RecordingCredentialProvider(DataCredentialResult.Required("login required"));
        var transport = new RecordingTransport(_ => DataResult<string>.Success("should-not-run"));
        var pipeline = new DataRequestPipeline(transport, credentialProvider: credentials, diagnostics);
        var request = new DataRequest<string>(
            "catalog",
            "secure-items",
            DataTransportKind.Http)
        {
            Authentication = DataAuthenticationOptions.Bearer(),
        };

        await pipeline.SendAsync(request);

        var record = Assert.Single(
            diagnostics.Records,
            record => record.Code == DataDiagnosticIds.RequestFailed);
        Assert.Equal("catalog", record.ClientId);
        Assert.Equal("secure-items", record.OperationName);
        Assert.Equal(DataTransportKind.Http, record.TransportKind);
        Assert.Equal(0, record.Attempt);
        Assert.Equal(DataErrorKind.AuthenticationRequired, record.ErrorKind);
    }

    [Fact]
    public async Task PipelineMapsCredentialProviderCancellation()
    {
        var credentials = new ThrowingCredentialProvider(new OperationCanceledException());
        var transport = new RecordingTransport(_ => DataResult<string>.Success("should-not-run"));
        var pipeline = new DataRequestPipeline(transport, credentialProvider: credentials);
        var request = new DataRequest<string>(
            "catalog",
            "secure-items",
            DataTransportKind.Http)
        {
            Authentication = DataAuthenticationOptions.Bearer(),
        };

        var result = await pipeline.SendAsync(request);

        Assert.Equal(DataResultStatus.Cancelled, result.Status);
        Assert.Equal(0, transport.Attempts);
    }

    [Fact]
    public async Task PipelineRetriesQueryTransientFailures()
    {
        var transport = new RecordingTransport(context =>
        {
            return context.Attempt == 1
                ? DataResult<string>.Failed(new DataError(DataErrorKind.NetworkUnavailable, "network"))
                : DataResult<string>.Success("retried");
        });
        var pipeline = new DataRequestPipeline(transport);
        var request = new DataRequest<string>(
            "catalog",
            "get-items",
            DataTransportKind.Http)
        {
            Resilience = new DataResilienceOptions { MaxRetryAttempts = 1 },
        };

        var result = await pipeline.SendAsync(request);

        Assert.True(result.Succeeded);
        Assert.Equal("retried", result.Value);
        Assert.Equal(2, transport.Attempts);
    }

    [Fact]
    public async Task PipelineDoesNotRetryMutationWithoutIdempotency()
    {
        var transport = new RecordingTransport(_ =>
            DataResult<string>.Failed(new DataError(DataErrorKind.NetworkUnavailable, "network")));
        var pipeline = new DataRequestPipeline(transport);
        var request = new DataRequest<string>(
            "catalog",
            "save-item",
            DataTransportKind.Http,
            DataAccessMode.Mutation)
        {
            Resilience = new DataResilienceOptions { MaxRetryAttempts = 3 },
        };

        var result = await pipeline.SendAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(1, transport.Attempts);
    }

    [Fact]
    public async Task PipelineReturnsCancelledWhenRequestTokenIsCancelled()
    {
        var transport = new RecordingTransport(_ => DataResult<string>.Success("should-not-run"));
        var pipeline = new DataRequestPipeline(transport);
        var request = new DataRequest<string>(
            "catalog",
            "get-items",
            DataTransportKind.Http);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await pipeline.SendAsync(request, cancellation.Token);

        Assert.Equal(DataResultStatus.Cancelled, result.Status);
        Assert.Equal(0, transport.Attempts);
    }

    [Fact]
    public async Task PipelineMapsOperationTimeout()
    {
        var transport = new RecordingTransport(async context =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), context.CancellationToken);

            return DataResult<string>.Success("late");
        });
        var pipeline = new DataRequestPipeline(transport);
        var request = new DataRequest<string>(
            "catalog",
            "get-items",
            DataTransportKind.Http)
        {
            Resilience = new DataResilienceOptions { Timeout = TimeSpan.FromMilliseconds(10) },
        };

        var result = await pipeline.SendAsync(request);

        Assert.Equal(DataResultStatus.Failed, result.Status);
        Assert.Equal(DataErrorKind.Timeout, result.Error?.Kind);
    }

    [Fact]
    public async Task PipelineSuppressesResultWhenParentScopeStops()
    {
        var parent = LifecycleScope.CreateRoot(LifecycleScopeKind.Route, "route:/items");
        var release = new TaskCompletionSource();
        var transport = new RecordingTransport(async _ =>
        {
            await release.Task;

            return DataResult<string>.Success("late");
        });
        var pipeline = new DataRequestPipeline(transport);
        var request = new DataRequest<string>(
            "catalog",
            "get-items",
            DataTransportKind.Http)
        {
            ParentScope = parent,
        };

        var resultTask = pipeline.SendAsync(request).AsTask();
        await parent.StopAsync();
        release.SetResult();
        var result = await resultTask;

        Assert.Equal(DataResultStatus.StaleSuppressed, result.Status);
    }

    private sealed class RecordingTransport : IRequestResponseTransport
    {
        private readonly Func<DataRequestContext, ValueTask<DataResult<string>>> _handler;

        public RecordingTransport(Func<DataRequestContext, DataResult<string>> handler)
            : this(context => ValueTask.FromResult(handler(context)), DataTransportKind.Http)
        {
        }

        public RecordingTransport(
            Func<DataRequestContext, DataResult<string>> handler,
            DataTransportKind kind)
            : this(context => ValueTask.FromResult(handler(context)), kind)
        {
        }

        public RecordingTransport(Func<DataRequestContext, ValueTask<DataResult<string>>> handler)
            : this(handler, DataTransportKind.Http)
        {
        }

        public RecordingTransport(
            Func<DataRequestContext, ValueTask<DataResult<string>>> handler,
            DataTransportKind kind)
        {
            _handler = handler;
            Kind = kind;
        }

        public DataTransportKind Kind { get; }

        public int Attempts { get; private set; }

        public DataRequestContext? LastContext { get; private set; }

        public async ValueTask<DataResult<TResponse>> SendAsync<TResponse>(
            DataRequest<TResponse> request,
            DataRequestContext context,
            CancellationToken cancellationToken = default)
        {
            Attempts++;
            LastContext = context;
            var result = await _handler(context);

            return result.Cast<TResponse>();
        }
    }

    private sealed class RecordingCredentialProvider : IDataCredentialProvider
    {
        private readonly DataCredentialResult _result;

        public RecordingCredentialProvider(DataCredentialResult result)
        {
            _result = result;
        }

        public int Requests { get; private set; }

        public ValueTask<DataCredentialResult> GetCredentialAsync(
            DataAuthenticationContext context,
            CancellationToken cancellationToken = default)
        {
            Requests++;

            return ValueTask.FromResult(_result);
        }
    }

    private sealed class ThrowingCredentialProvider : IDataCredentialProvider
    {
        private readonly Exception _exception;

        public ThrowingCredentialProvider(Exception exception)
        {
            _exception = exception;
        }

        public ValueTask<DataCredentialResult> GetCredentialAsync(
            DataAuthenticationContext context,
            CancellationToken cancellationToken = default)
        {
            throw _exception;
        }
    }

    private sealed class ThrowingRequestCache : IDataRequestCache
    {
        private readonly Exception? _readException;
        private readonly Exception? _writeException;

        public ThrowingRequestCache(Exception? readException = null, Exception? writeException = null)
        {
            _readException = readException;
            _writeException = writeException;
        }

        public ValueTask<DataCacheLookup<TResponse>> TryGetAsync<TResponse>(
            DataCacheKey key,
            CancellationToken cancellationToken = default)
        {
            if (_readException is not null)
            {
                throw _readException;
            }

            return ValueTask.FromResult(DataCacheLookup<TResponse>.Miss());
        }

        public ValueTask SetAsync<TResponse>(
            DataCacheKey key,
            TResponse? value,
            CancellationToken cancellationToken = default)
        {
            if (_writeException is not null)
            {
                throw _writeException;
            }

            return ValueTask.CompletedTask;
        }
    }

    private sealed class RecordingRequestCache : IDataRequestCache
    {
        private readonly Dictionary<DataCacheKey, object?> _entries = new();

        public int Reads { get; private set; }

        public int Writes { get; private set; }

        public void Store<TResponse>(DataCacheKey key, TResponse value)
        {
            _entries[key] = value;
        }

        public TResponse? Get<TResponse>(DataCacheKey key)
        {
            return _entries.TryGetValue(key, out var value) && value is TResponse response
                ? response
                : default;
        }

        public ValueTask<DataCacheLookup<TResponse>> TryGetAsync<TResponse>(
            DataCacheKey key,
            CancellationToken cancellationToken = default)
        {
            Reads++;

            if (_entries.TryGetValue(key, out var value) && value is TResponse response)
            {
                return ValueTask.FromResult(DataCacheLookup<TResponse>.Hit(response));
            }

            return ValueTask.FromResult(DataCacheLookup<TResponse>.Miss());
        }

        public ValueTask SetAsync<TResponse>(
            DataCacheKey key,
            TResponse? value,
            CancellationToken cancellationToken = default)
        {
            Writes++;
            _entries[key] = value;

            return ValueTask.CompletedTask;
        }
    }
}
