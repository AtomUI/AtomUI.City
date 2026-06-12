using AtomUI.City.Diagnostics;
using AtomUI.City.Mvvm;
using AtomUI.City.Presentation;
using AtomUI.City.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Presentation.Tests;

public sealed class PresentationInteractionHandlerTests
{
    [Fact]
    public void ServiceCollectionRegistersInteractionHandlerRegistry()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IUiDispatcher>(new RecordingDispatcher());

        services.AddPresentationInteractionHandlers();

        using var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<IInteractionHandlerRegistry>();

        Assert.Same(provider.GetRequiredService<InteractionHandlerRegistry>(), registry);
    }

    [Fact]
    public async Task RegistryRunsHandlerOnUiDispatcherAndRecordsHandledDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var dispatcher = new RecordingDispatcher();
        var registry = new InteractionHandlerRegistry(dispatcher, diagnostics);
        var handlerWasOnDispatcher = false;
        registry.Register<ConfirmRequest, bool>(
            (context, _) =>
            {
                handlerWasOnDispatcher = dispatcher.IsOnDispatcher;
                return ValueTask.FromResult(context.Request.Message == "Delete?");
            });

        var result = await registry.HandleAsync<ConfirmRequest, bool>(
            new ConfirmRequest("Delete?"));

        Assert.Equal(InteractionResultStatus.Completed, result.Status);
        Assert.True(result.Value);
        Assert.True(handlerWasOnDispatcher);
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.InteractionHandled &&
                record.Severity == HostDiagnosticSeverity.Info &&
                record.Message.Contains(typeof(ConfirmRequest).FullName!, StringComparison.Ordinal));
    }

    [Fact]
    public async Task RegistryReturnsNotHandledAndRecordsDiagnosticsWhenHandlerIsMissing()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var registry = new InteractionHandlerRegistry(new RecordingDispatcher(), diagnostics);

        var result = await registry.HandleAsync<ConfirmRequest, bool>(
            new ConfirmRequest("Delete?"));

        Assert.Equal(InteractionResultStatus.NotHandled, result.Status);
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.InteractionNotHandled &&
                record.Severity == HostDiagnosticSeverity.Warning &&
                record.Message.Contains(typeof(ConfirmRequest).FullName!, StringComparison.Ordinal));
    }

    [Fact]
    public async Task RegistryReturnsFailedAndRecordsDiagnosticsWhenHandlerThrows()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var registry = new InteractionHandlerRegistry(new RecordingDispatcher(), diagnostics);
        registry.Register<ConfirmRequest, bool>(
            (_, _) => throw new InvalidOperationException("interaction failed"));

        var result = await registry.HandleAsync<ConfirmRequest, bool>(
            new ConfirmRequest("Delete?"));

        Assert.Equal(InteractionResultStatus.Failed, result.Status);
        Assert.IsType<InvalidOperationException>(result.Exception);
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.InteractionFailed &&
                record.Severity == HostDiagnosticSeverity.Error &&
                record.Message.Contains("interaction failed", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RegistryRemovesHandlerWhenActivationScopeIsDisposed()
    {
        var scope = new ActivationScope();
        var registry = new InteractionHandlerRegistry(new RecordingDispatcher());
        registry.Register<ConfirmRequest, bool>(
            (_, _) => ValueTask.FromResult(true),
            scope);

        scope.Dispose();
        var result = await registry.HandleAsync<ConfirmRequest, bool>(
            new ConfirmRequest("Delete?"));

        Assert.Equal(InteractionResultStatus.NotHandled, result.Status);
    }

    [Fact]
    public async Task RegistryReturnsCanceledWhenActivationScopeStopsPendingHandler()
    {
        var scope = new ActivationScope();
        var registry = new InteractionHandlerRegistry(new RecordingDispatcher());
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        registry.Register<ConfirmRequest, bool>(
            async (_, cancellationToken) =>
            {
                started.SetResult();
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                return true;
            },
            scope);

        var request = registry.HandleAsync<ConfirmRequest, bool>(
            new ConfirmRequest("Delete?")).AsTask();
        await started.Task;

        scope.Dispose();
        var result = await request;

        Assert.Equal(InteractionResultStatus.Canceled, result.Status);
    }

    [Fact]
    public async Task RegistryRevokesHandlersByPluginId()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var registry = new InteractionHandlerRegistry(new RecordingDispatcher(), diagnostics);
        registry.Register<ConfirmRequest, bool>(
            (_, _) => ValueTask.FromResult(true),
            new InteractionHandlerRegistrationOptions
            {
                PluginId = "com.company.sales",
                ContributionId = "sales.confirmation",
            });

        var revoked = registry.RevokePlugin("com.company.sales");
        var result = await registry.HandleAsync<ConfirmRequest, bool>(
            new ConfirmRequest("Delete?"));

        Assert.Equal(1, revoked);
        Assert.Equal(InteractionResultStatus.NotHandled, result.Status);
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.InteractionHandlerRevoked &&
                record.Severity == HostDiagnosticSeverity.Info &&
                record.Message.Contains("com.company.sales", StringComparison.Ordinal) &&
                record.Message.Contains("sales.confirmation", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RegistryRevokesHandlersByContributionId()
    {
        var registry = new InteractionHandlerRegistry(new RecordingDispatcher());
        registry.Register<ConfirmRequest, bool>(
            (_, _) => ValueTask.FromResult(false),
            new InteractionHandlerRegistrationOptions
            {
                PluginId = "com.company.sales",
                ContributionId = "sales.confirmation",
            });
        registry.Register<ConfirmRequest, bool>(
            (_, _) => ValueTask.FromResult(true),
            new InteractionHandlerRegistrationOptions
            {
                PluginId = "com.company.support",
                ContributionId = "support.confirmation",
            });

        var revoked = registry.RevokeContribution("support.confirmation");
        var result = await registry.HandleAsync<ConfirmRequest, bool>(
            new ConfirmRequest("Delete?"));

        Assert.Equal(1, revoked);
        Assert.Equal(InteractionResultStatus.Completed, result.Status);
        Assert.False(result.Value);
    }

    [Fact]
    public async Task RevokingPluginCancelsPendingInteraction()
    {
        var registry = new InteractionHandlerRegistry(new RecordingDispatcher());
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        registry.Register<ConfirmRequest, bool>(
            async (_, cancellationToken) =>
            {
                started.SetResult();
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                return true;
            },
            new InteractionHandlerRegistrationOptions
            {
                PluginId = "com.company.sales",
                ContributionId = "sales.confirmation",
            });

        var request = registry.HandleAsync<ConfirmRequest, bool>(
            new ConfirmRequest("Delete?")).AsTask();
        await started.Task;

        registry.RevokePlugin("com.company.sales");
        var result = await request;

        Assert.Equal(InteractionResultStatus.Canceled, result.Status);
    }

    private readonly record struct ConfirmRequest(string Message);

    private sealed class RecordingDispatcher : IUiDispatcher
    {
        public bool IsOnDispatcher { get; private set; }

        public bool CheckAccess()
        {
            return IsOnDispatcher;
        }

        public ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default)
        {
            return RunOnDispatcherAsync(
                _ =>
                {
                    callback();
                    return ValueTask.CompletedTask;
                },
                cancellationToken);
        }

        public async ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)
        {
            T? result = default;
            await RunOnDispatcherAsync(
                _ =>
                {
                    result = callback();
                    return ValueTask.CompletedTask;
                },
                cancellationToken);

            return result!;
        }

        public ValueTask PostAsync(
            Func<CancellationToken, ValueTask> callback,
            CancellationToken cancellationToken = default)
        {
            return RunOnDispatcherAsync(callback, cancellationToken);
        }

        private async ValueTask RunOnDispatcherAsync(
            Func<CancellationToken, ValueTask> callback,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IsOnDispatcher = true;

            try
            {
                await callback(cancellationToken);
            }
            finally
            {
                IsOnDispatcher = false;
            }
        }
    }
}
