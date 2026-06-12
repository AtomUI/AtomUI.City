using AtomUI.City.Diagnostics;
using AtomUI.City.Localization;
using AtomUI.City.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Presentation.Tests;

public sealed class PresentationPluginUnloadCoordinatorTests
{
    [Fact]
    public void ServiceCollectionRegistersPresentationPluginUnloadCoordinator()
    {
        var services = new ServiceCollection();
        var activeViews = new RecordingActivePluginViewRegistry([]);
        var interactions = new RecordingInteractionHandlerRegistry();
        var viewRegistry = new RecordingViewRegistry();
        var resources = new RecordingResourceRegistry();
        var resourceDictionaries = new RecordingResourceDictionaryRevoker();

        services.AddSingleton<IActivePluginViewRegistry>(activeViews);
        services.AddSingleton<IInteractionHandlerRegistry>(interactions);
        services.AddSingleton<IViewRegistry>(viewRegistry);
        services.AddSingleton<IPresentationResourceRegistry>(resources);
        services.AddSingleton<IPresentationResourceDictionaryRevoker>(resourceDictionaries);
        services.AddPresentationPluginUnloadCoordinator();

        using var provider = services.BuildServiceProvider();
        var coordinator = provider.GetRequiredService<IPresentationPluginUnloadCoordinator>();

        Assert.Same(
            provider.GetRequiredService<PresentationPluginUnloadCoordinator>(),
            coordinator);
    }

    [Fact]
    public async Task CleanupPluginClosesViewsBeforeRevokingPresentationContributions()
    {
        var callOrder = new List<string>();
        var diagnostics = new InMemoryHostDiagnostics();
        var activeViews = new RecordingActivePluginViewRegistry(callOrder)
        {
            ClosePluginViewsCount = 2,
        };
        var interactions = new RecordingInteractionHandlerRegistry(callOrder)
        {
            RevokePluginCount = 3,
        };
        var viewRegistry = new RecordingViewRegistry(callOrder)
        {
            RevokePluginCount = 4,
        };
        var resources = new RecordingResourceRegistry(callOrder)
        {
            RevokePluginCount = 5,
        };
        var resourceDictionaries = new RecordingResourceDictionaryRevoker(callOrder);
        var coordinator = new PresentationPluginUnloadCoordinator(
            activeViews,
            interactions,
            viewRegistry,
            resources,
            resourceDictionaries,
            diagnostics);

        var result = await coordinator.CleanupAsync(
            new PresentationPluginUnloadRequest("com.company.sales"));

        Assert.True(result.Succeeded);
        Assert.Equal("com.company.sales", result.PluginId);
        Assert.Null(result.ContributionId);
        Assert.Equal(2, result.ClosedViewCount);
        Assert.Equal(3, result.RevokedInteractionHandlerCount);
        Assert.Equal(4, result.RevokedViewDescriptorCount);
        Assert.Equal(5, result.RevokedResourceContributionCount);
        Assert.True(result.ResourceDictionariesRevoked);
        Assert.Equal(
            [
                "close-plugin:com.company.sales",
                "revoke-interactions-plugin:com.company.sales",
                "revoke-views-plugin:com.company.sales",
                "revoke-dictionaries:com.company.sales:<all>",
                "revoke-resources-plugin:com.company.sales",
            ],
            callOrder);
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.PluginUnloadCleanupCompleted &&
                record.Severity == HostDiagnosticSeverity.Info &&
                record.Message.Contains("com.company.sales", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CleanupContributionUsesContributionSpecificRevocation()
    {
        var callOrder = new List<string>();
        var activeViews = new RecordingActivePluginViewRegistry(callOrder)
        {
            CloseContributionViewsCount = 1,
        };
        var interactions = new RecordingInteractionHandlerRegistry(callOrder)
        {
            RevokeContributionCount = 2,
        };
        var viewRegistry = new RecordingViewRegistry(callOrder)
        {
            RevokeContributionCount = 3,
        };
        var resources = new RecordingResourceRegistry(callOrder)
        {
            RevokeContributionCount = 4,
        };
        var resourceDictionaries = new RecordingResourceDictionaryRevoker(callOrder);
        var coordinator = new PresentationPluginUnloadCoordinator(
            activeViews,
            interactions,
            viewRegistry,
            resources,
            resourceDictionaries);

        var result = await coordinator.CleanupAsync(
            new PresentationPluginUnloadRequest(
                "com.company.sales",
                contributionId: "sales.settings"));

        Assert.True(result.Succeeded);
        Assert.Equal("sales.settings", result.ContributionId);
        Assert.Equal(1, result.ClosedViewCount);
        Assert.Equal(2, result.RevokedInteractionHandlerCount);
        Assert.Equal(3, result.RevokedViewDescriptorCount);
        Assert.Equal(4, result.RevokedResourceContributionCount);
        Assert.Equal(
            [
                "close-contribution:sales.settings",
                "revoke-interactions-contribution:sales.settings",
                "revoke-views-contribution:sales.settings",
                "revoke-dictionaries:com.company.sales:sales.settings",
                "revoke-resources-contribution:sales.settings",
            ],
            callOrder);
    }

    [Fact]
    public async Task CleanupStopsWhenActivePluginViewsRemain()
    {
        var callOrder = new List<string>();
        var diagnostics = new InMemoryHostDiagnostics();
        var activeViews = new RecordingActivePluginViewRegistry(
            callOrder,
            [new ActivePluginView("com.company.sales", new StubOutlet("primary"), BoundViewHandle.FromExisting(new SettingsView(), new SettingsViewModel()))])
        {
            ClosePluginViewsCount = 0,
        };
        var interactions = new RecordingInteractionHandlerRegistry(callOrder);
        var viewRegistry = new RecordingViewRegistry(callOrder);
        var resources = new RecordingResourceRegistry(callOrder);
        var resourceDictionaries = new RecordingResourceDictionaryRevoker(callOrder);
        var coordinator = new PresentationPluginUnloadCoordinator(
            activeViews,
            interactions,
            viewRegistry,
            resources,
            resourceDictionaries,
            diagnostics);

        var result = await coordinator.CleanupAsync(
            new PresentationPluginUnloadRequest("com.company.sales"));

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ClosedViewCount);
        Assert.Single(result.Errors);
        Assert.Equal(PresentationPluginUnloadErrorKind.ActiveViewsRemaining, result.Errors[0].Kind);
        Assert.Equal(["close-plugin:com.company.sales"], callOrder);
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.PluginUnloadCleanupFailed &&
                record.Severity == HostDiagnosticSeverity.Error &&
                record.Message.Contains("active plugin view", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CleanupRecordsResourceDictionaryFailureAndContinuesResourceRevocation()
    {
        var callOrder = new List<string>();
        var activeViews = new RecordingActivePluginViewRegistry(callOrder);
        var interactions = new RecordingInteractionHandlerRegistry(callOrder)
        {
            RevokePluginCount = 1,
        };
        var viewRegistry = new RecordingViewRegistry(callOrder)
        {
            RevokePluginCount = 2,
        };
        var resources = new RecordingResourceRegistry(callOrder)
        {
            RevokePluginCount = 3,
        };
        var resourceDictionaries = new RecordingResourceDictionaryRevoker(callOrder)
        {
            Failure = new LocalizationError(
                LocalizationErrorKind.PresentationApplyFailed,
                "resource dictionary revoke failed"),
        };
        var coordinator = new PresentationPluginUnloadCoordinator(
            activeViews,
            interactions,
            viewRegistry,
            resources,
            resourceDictionaries);

        var result = await coordinator.CleanupAsync(
            new PresentationPluginUnloadRequest("com.company.sales"));

        Assert.False(result.Succeeded);
        Assert.Equal(1, result.RevokedInteractionHandlerCount);
        Assert.Equal(2, result.RevokedViewDescriptorCount);
        Assert.Equal(3, result.RevokedResourceContributionCount);
        Assert.False(result.ResourceDictionariesRevoked);
        Assert.Single(result.Errors);
        Assert.Equal(PresentationPluginUnloadErrorKind.ResourceDictionaryRevokeFailed, result.Errors[0].Kind);
        Assert.Equal(
            [
                "close-plugin:com.company.sales",
                "revoke-interactions-plugin:com.company.sales",
                "revoke-views-plugin:com.company.sales",
                "revoke-dictionaries:com.company.sales:<all>",
                "revoke-resources-plugin:com.company.sales",
            ],
            callOrder);
    }

    [Fact]
    public async Task CleanupRecordsViewDescriptorFailureAndContinuesResourceRevocation()
    {
        var callOrder = new List<string>();
        var activeViews = new RecordingActivePluginViewRegistry(callOrder);
        var interactions = new RecordingInteractionHandlerRegistry(callOrder)
        {
            RevokePluginCount = 1,
        };
        var viewRegistry = new RecordingViewRegistry(callOrder)
        {
            Failure = new InvalidOperationException("view revoke failed"),
        };
        var resources = new RecordingResourceRegistry(callOrder)
        {
            RevokePluginCount = 2,
        };
        var resourceDictionaries = new RecordingResourceDictionaryRevoker(callOrder);
        var coordinator = new PresentationPluginUnloadCoordinator(
            activeViews,
            interactions,
            viewRegistry,
            resources,
            resourceDictionaries);

        var result = await coordinator.CleanupAsync(
            new PresentationPluginUnloadRequest("com.company.sales"));

        Assert.False(result.Succeeded);
        Assert.Equal(1, result.RevokedInteractionHandlerCount);
        Assert.Equal(0, result.RevokedViewDescriptorCount);
        Assert.Equal(2, result.RevokedResourceContributionCount);
        Assert.True(result.ResourceDictionariesRevoked);
        Assert.Single(result.Errors);
        Assert.Equal(PresentationPluginUnloadErrorKind.ViewDescriptorRevokeFailed, result.Errors[0].Kind);
        Assert.Equal(
            [
                "close-plugin:com.company.sales",
                "revoke-interactions-plugin:com.company.sales",
                "revoke-views-plugin:com.company.sales",
                "revoke-dictionaries:com.company.sales:<all>",
                "revoke-resources-plugin:com.company.sales",
            ],
            callOrder);
    }

    private sealed class RecordingActivePluginViewRegistry : IActivePluginViewRegistry
    {
        private readonly List<string> _callOrder;

        public RecordingActivePluginViewRegistry(IReadOnlyList<ActivePluginView> activeViews)
            : this([], activeViews)
        {
        }

        public RecordingActivePluginViewRegistry(
            List<string> callOrder,
            IReadOnlyList<ActivePluginView>? activeViews = null)
        {
            _callOrder = callOrder;
            ActiveViews = activeViews?.ToArray() ?? [];
        }

        public IReadOnlyList<ActivePluginView> ActiveViews { get; private set; }

        public int ClosePluginViewsCount { get; init; }

        public int CloseContributionViewsCount { get; init; }

        public IActivePluginViewLease Track(ActivePluginView view)
        {
            throw new NotSupportedException();
        }

        public ValueTask<int> ClosePluginViewsAsync(
            string pluginId,
            CancellationToken cancellationToken = default)
        {
            _callOrder.Add($"close-plugin:{pluginId}");
            if (ClosePluginViewsCount > 0)
            {
                ActiveViews = ActiveViews
                    .Where(view => !string.Equals(view.PluginId, pluginId, StringComparison.Ordinal))
                    .ToArray();
            }

            return ValueTask.FromResult(ClosePluginViewsCount);
        }

        public ValueTask<int> CloseContributionViewsAsync(
            string contributionId,
            CancellationToken cancellationToken = default)
        {
            _callOrder.Add($"close-contribution:{contributionId}");
            if (CloseContributionViewsCount > 0)
            {
                ActiveViews = ActiveViews
                    .Where(view => !string.Equals(view.ContributionId, contributionId, StringComparison.Ordinal))
                    .ToArray();
            }

            return ValueTask.FromResult(CloseContributionViewsCount);
        }
    }

    private sealed class RecordingInteractionHandlerRegistry(List<string>? callOrder = null) : IInteractionHandlerRegistry
    {
        private readonly List<string> _callOrder = callOrder ?? [];

        public int RevokePluginCount { get; init; }

        public int RevokeContributionCount { get; init; }

        public IDisposable Register<TRequest, TResult>(
            Func<AtomUI.City.Mvvm.InteractionContext<TRequest>, CancellationToken, ValueTask<TResult>> handler,
            AtomUI.City.Mvvm.IActivationScope? activationScope = null)
        {
            throw new NotSupportedException();
        }

        public IDisposable Register<TRequest, TResult>(
            Func<AtomUI.City.Mvvm.InteractionContext<TRequest>, CancellationToken, ValueTask<TResult>> handler,
            InteractionHandlerRegistrationOptions options)
        {
            throw new NotSupportedException();
        }

        public ValueTask<AtomUI.City.Mvvm.InteractionResult<TResult>> HandleAsync<TRequest, TResult>(
            TRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public int RevokePlugin(string pluginId)
        {
            _callOrder.Add($"revoke-interactions-plugin:{pluginId}");

            return RevokePluginCount;
        }

        public int RevokeContribution(string contributionId)
        {
            _callOrder.Add($"revoke-interactions-contribution:{contributionId}");

            return RevokeContributionCount;
        }
    }

    private sealed class RecordingViewRegistry(List<string>? callOrder = null) : IViewRegistry
    {
        private readonly List<string> _callOrder = callOrder ?? [];

        public int RevokePluginCount { get; init; }

        public int RevokeContributionCount { get; init; }

        public Exception? Failure { get; init; }

        public void Register(ViewDescriptor descriptor)
        {
            throw new NotSupportedException();
        }

        public bool TryLocate(Type viewModelType, out ViewDescriptor? descriptor)
        {
            throw new NotSupportedException();
        }

        public bool TryLocate(
            Type viewModelType,
            string? viewKey,
            out ViewDescriptor? descriptor)
        {
            throw new NotSupportedException();
        }

        public ViewDescriptor Locate(Type viewModelType, string? viewKey = null)
        {
            throw new NotSupportedException();
        }

        public int RevokePlugin(string pluginId)
        {
            _callOrder.Add($"revoke-views-plugin:{pluginId}");

            if (Failure is not null)
            {
                throw Failure;
            }

            return RevokePluginCount;
        }

        public int RevokeContribution(string contributionId)
        {
            _callOrder.Add($"revoke-views-contribution:{contributionId}");

            if (Failure is not null)
            {
                throw Failure;
            }

            return RevokeContributionCount;
        }
    }

    private sealed class RecordingResourceRegistry(List<string>? callOrder = null) : IPresentationResourceRegistry
    {
        private readonly List<string> _callOrder = callOrder ?? [];

        public IReadOnlyList<PresentationResourceContribution> Contributions => [];

        public int RevokePluginCount { get; init; }

        public int RevokeContributionCount { get; init; }

        public IPresentationResourceLease Register(PresentationResourceContribution contribution)
        {
            throw new NotSupportedException();
        }

        public int RevokePlugin(string pluginId)
        {
            _callOrder.Add($"revoke-resources-plugin:{pluginId}");

            return RevokePluginCount;
        }

        public int RevokeContribution(string contributionId)
        {
            _callOrder.Add($"revoke-resources-contribution:{contributionId}");

            return RevokeContributionCount;
        }
    }

    private sealed class RecordingResourceDictionaryRevoker(List<string>? callOrder = null) : IPresentationResourceDictionaryRevoker
    {
        private readonly List<string> _callOrder = callOrder ?? [];

        public LocalizationError? Failure { get; init; }

        public ValueTask<LocalizationResult> RevokeAsync(
            PresentationResourceDictionaryRevocation revocation,
            CancellationToken cancellationToken = default)
        {
            _callOrder.Add(
                $"revoke-dictionaries:{revocation.PluginId}:{revocation.ContributionId ?? "<all>"}");

            return ValueTask.FromResult(
                Failure is null
                    ? LocalizationResult.Success()
                    : LocalizationResult.Failed(Failure));
        }
    }

    private sealed class StubOutlet(string name) : IRouteOutlet
    {
        public string Name { get; } = name;

        public object? CurrentContent => null;

        public ValueTask<RouteOutletCommitResult> CommitAsync(
            RouteOutletCommitPlan plan,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(RouteOutletCommitResult.Success());
        }
    }

    private sealed class SettingsViewModel;

    private sealed class SettingsView;
}
