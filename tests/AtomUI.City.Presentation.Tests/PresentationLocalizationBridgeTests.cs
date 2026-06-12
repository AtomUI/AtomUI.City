using System.Globalization;
using AtomUI.City.Diagnostics;
using AtomUI.City.Localization;
using AtomUI.City.Presentation;
using AtomUI.City.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Presentation.Tests;

public sealed class PresentationLocalizationBridgeTests
{
    [Fact]
    public async Task ApplyCultureAsyncRunsAppliersOnUiDispatcherInOrder()
    {
        var dispatcher = new RecordingDispatcher();
        var first = new RecordingCultureApplier("first", dispatcher);
        var second = new RecordingCultureApplier("second", dispatcher);
        var bridge = new PresentationLocalizationBridge(dispatcher, [first, second]);

        var result = await bridge.ApplyCultureAsync(State("zh-CN"));

        Assert.True(result.Succeeded);
        Assert.Equal(1, dispatcher.PostCount);
        Assert.Equal(["first:zh-CN"], first.AppliedCultures);
        Assert.Equal(["second:zh-CN"], second.AppliedCultures);
        Assert.True(first.WasOnDispatcher);
        Assert.True(second.WasOnDispatcher);
        Assert.True(first.AppliedBefore(second));
    }

    [Fact]
    public async Task ApplyCultureAsyncStopsWhenApplierFails()
    {
        var dispatcher = new RecordingDispatcher();
        var failing = new RecordingCultureApplier("failing", dispatcher)
        {
            Failure = new LocalizationError(
                LocalizationErrorKind.PresentationApplyFailed,
                "Resource dictionary apply failed."),
        };
        var skipped = new RecordingCultureApplier("skipped", dispatcher);
        var bridge = new PresentationLocalizationBridge(dispatcher, [failing, skipped]);

        var result = await bridge.ApplyCultureAsync(State("en-US"));

        Assert.False(result.Succeeded);
        Assert.Equal(failing.Failure, result.Error);
        Assert.Equal(["failing:en-US"], failing.AppliedCultures);
        Assert.Empty(skipped.AppliedCultures);
    }

    [Fact]
    public async Task ServiceCollectionRegistersPresentationLocalizationBridge()
    {
        var services = new ServiceCollection();
        var dispatcher = new RecordingDispatcher();
        services.AddSingleton<IUiDispatcher>(dispatcher);
        services.AddPresentationCultureApplier<RegisteredCultureApplier>();
        services.AddPresentationLocalizationBridge();

        var provider = services.BuildServiceProvider();
        var bridge = provider.GetRequiredService<IPresentationLocalizationBridge>();

        var result = await bridge.ApplyCultureAsync(State("ja-JP"));

        Assert.True(result.Succeeded);
        Assert.IsType<PresentationLocalizationBridge>(bridge);
        var applier = provider.GetRequiredService<IEnumerable<IPresentationCultureApplier>>()
            .OfType<RegisteredCultureApplier>()
            .Single();
        Assert.Equal(["ja-JP"], applier.AppliedCultures);
    }

    [Fact]
    public async Task ServiceCollectionBridgeAppliesCurrentThreadCultures()
    {
        var originalCurrentCulture = CultureInfo.CurrentCulture;
        var originalCurrentUICulture = CultureInfo.CurrentUICulture;
        var originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        var originalDefaultUICulture = CultureInfo.DefaultThreadCurrentUICulture;

        try
        {
            var services = new ServiceCollection();
            services.AddSingleton<IUiDispatcher>(new RecordingDispatcher());
            services.AddPresentationLocalizationBridge();
            var provider = services.BuildServiceProvider();
            var bridge = provider.GetRequiredService<IPresentationLocalizationBridge>();

            var result = await bridge.ApplyCultureAsync(State("fr-FR", "ja-JP"));

            Assert.True(result.Succeeded);
            Assert.Equal("fr-FR", CultureInfo.CurrentCulture.Name);
            Assert.Equal("ja-JP", CultureInfo.CurrentUICulture.Name);
            Assert.Equal("fr-FR", CultureInfo.DefaultThreadCurrentCulture?.Name);
            Assert.Equal("ja-JP", CultureInfo.DefaultThreadCurrentUICulture?.Name);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCurrentCulture;
            CultureInfo.CurrentUICulture = originalCurrentUICulture;
            CultureInfo.DefaultThreadCurrentCulture = originalDefaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = originalDefaultUICulture;
        }
    }

    [Fact]
    public async Task ServiceCollectionBridgeAppliesFlowDirectionTargets()
    {
        var services = new ServiceCollection();
        var dispatcher = new RecordingDispatcher();
        services.AddSingleton(dispatcher);
        services.AddSingleton<IUiDispatcher>(dispatcher);
        services.AddPresentationFlowDirectionTarget<RecordingFlowDirectionTarget>();
        services.AddPresentationLocalizationBridge();
        var provider = services.BuildServiceProvider();
        var bridge = provider.GetRequiredService<IPresentationLocalizationBridge>();

        var rtlResult = await bridge.ApplyCultureAsync(State("ar-SA"));
        var ltrResult = await bridge.ApplyCultureAsync(State("en-US"));

        Assert.True(rtlResult.Succeeded);
        Assert.True(ltrResult.Succeeded);
        var target = provider.GetRequiredService<IEnumerable<IPresentationFlowDirectionTarget>>()
            .OfType<RecordingFlowDirectionTarget>()
            .Single();
        Assert.Equal(
            [PresentationFlowDirection.RightToLeft, PresentationFlowDirection.LeftToRight],
            target.AppliedDirections);
        Assert.Equal([true, true], target.DispatcherAccess);
    }

    [Fact]
    public async Task ServiceCollectionBridgeAppliesResourceDictionaryTargets()
    {
        var services = new ServiceCollection();
        var dispatcher = new RecordingDispatcher();
        services.AddSingleton(dispatcher);
        services.AddSingleton<IUiDispatcher>(dispatcher);
        services.AddPresentationResourceDictionaryTarget<RecordingResourceDictionaryTarget>();
        services.AddPresentationLocalizationBridge();
        var provider = services.BuildServiceProvider();
        var bridge = provider.GetRequiredService<IPresentationLocalizationBridge>();

        var result = await bridge.ApplyCultureAsync(
            State("zh-CN", loadedPackageIds: ["Host.zh-CN", "Orders.zh-CN"]));

        Assert.True(result.Succeeded);
        var target = provider.GetRequiredService<IEnumerable<IPresentationResourceDictionaryTarget>>()
            .OfType<RecordingResourceDictionaryTarget>()
            .Single();
        Assert.Equal(["zh-CN"], target.AppliedCultures);
        Assert.Equal([["Host.zh-CN", "Orders.zh-CN"]], target.AppliedPackageIds);
        Assert.Equal([true], target.DispatcherAccess);
    }

    [Fact]
    public async Task ResourceDictionaryApplierRecordsAppliedDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var dispatcher = new RecordingDispatcher();
        var target = new RecordingResourceDictionaryTarget(dispatcher);
        var applier = new CultureResourceDictionaryApplier([target], diagnostics);

        await applier.ApplyCultureAsync(
            State("zh-CN", loadedPackageIds: ["Host.zh-CN", "Orders.zh-CN"]));

        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ResourceDictionaryApplied &&
                record.Severity == HostDiagnosticSeverity.Info &&
                record.Message.Contains("zh-CN", StringComparison.Ordinal) &&
                record.Message.Contains("Host.zh-CN", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ResourceDictionaryApplierRecordsApplyFailureDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var dispatcher = new RecordingDispatcher();
        var target = new RecordingResourceDictionaryTarget(dispatcher)
        {
            ApplyFailure = new LocalizationError(
                LocalizationErrorKind.PresentationApplyFailed,
                "Resource dictionary apply failed."),
        };
        var applier = new CultureResourceDictionaryApplier([target], diagnostics);

        await applier.ApplyCultureAsync(
            State("zh-CN", loadedPackageIds: ["Host.zh-CN", "Orders.zh-CN"]));

        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ResourceDictionaryApplyFailed &&
                record.Severity == HostDiagnosticSeverity.Error &&
                record.Message.Contains("zh-CN", StringComparison.Ordinal) &&
                record.Message.Contains("Resource dictionary apply failed.", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ServiceCollectionRevokesResourceDictionaryTargets()
    {
        var services = new ServiceCollection();
        var dispatcher = new RecordingDispatcher();
        services.AddSingleton(dispatcher);
        services.AddSingleton<IUiDispatcher>(dispatcher);
        services.AddPresentationResourceDictionaryTarget<RecordingResourceDictionaryTarget>();
        services.AddPresentationLocalizationBridge();
        var provider = services.BuildServiceProvider();
        var revoker = provider.GetRequiredService<IPresentationResourceDictionaryRevoker>();

        var result = await revoker.RevokeAsync(
            new PresentationResourceDictionaryRevocation(
                "plugin.orders",
                contributionId: "plugin.orders.resources"));

        Assert.True(result.Succeeded);
        var target = provider.GetRequiredService<IEnumerable<IPresentationResourceDictionaryTarget>>()
            .OfType<RecordingResourceDictionaryTarget>()
            .Single();
        Assert.Equal(["plugin.orders"], target.RevokedPluginIds);
        Assert.Equal(["plugin.orders.resources"], target.RevokedContributionIds);
        Assert.Equal([true], target.RevokeDispatcherAccess);
    }

    [Fact]
    public async Task ResourceDictionaryRevokerRecordsRevokedDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var dispatcher = new RecordingDispatcher();
        var target = new RecordingResourceDictionaryTarget(dispatcher);
        var revoker = new PresentationResourceDictionaryRevoker(
            dispatcher,
            [target],
            diagnostics);

        await revoker.RevokeAsync(
            new PresentationResourceDictionaryRevocation(
                "plugin.orders",
                contributionId: "plugin.orders.resources"));

        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ResourceDictionaryRevoked &&
                record.Severity == HostDiagnosticSeverity.Info &&
                record.Message.Contains("plugin.orders", StringComparison.Ordinal) &&
                record.Message.Contains("plugin.orders.resources", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ResourceDictionaryRevokerRecordsRevokeFailureDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var dispatcher = new RecordingDispatcher();
        var target = new RecordingResourceDictionaryTarget(dispatcher)
        {
            RevokeFailure = new LocalizationError(
                LocalizationErrorKind.PresentationApplyFailed,
                "Resource dictionary revoke failed."),
        };
        var revoker = new PresentationResourceDictionaryRevoker(
            dispatcher,
            [target],
            diagnostics);

        await revoker.RevokeAsync(
            new PresentationResourceDictionaryRevocation(
                "plugin.orders",
                contributionId: "plugin.orders.resources"));

        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ResourceDictionaryRevokeFailed &&
                record.Severity == HostDiagnosticSeverity.Error &&
                record.Message.Contains("plugin.orders", StringComparison.Ordinal) &&
                record.Message.Contains("Resource dictionary revoke failed.", StringComparison.Ordinal));
    }

    private static CultureState State(
        string cultureName,
        string? uiCultureName = null,
        IReadOnlyList<string>? loadedPackageIds = null)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);
        var uiCulture = CultureInfo.GetCultureInfo(uiCultureName ?? cultureName);

        return new CultureState(
            culture,
            uiCulture,
            [],
            revision: 1,
            loadedPackageIds: loadedPackageIds ?? []);
    }

    private sealed class RegisteredCultureApplier : IPresentationCultureApplier
    {
        public List<string> AppliedCultures { get; } = [];

        public ValueTask<LocalizationResult> ApplyCultureAsync(
            CultureState state,
            CancellationToken cancellationToken = default)
        {
            AppliedCultures.Add(state.CurrentCulture.Name);

            return ValueTask.FromResult(LocalizationResult.Success());
        }
    }

    private sealed class RecordingFlowDirectionTarget : IPresentationFlowDirectionTarget
    {
        private readonly RecordingDispatcher _dispatcher;

        public RecordingFlowDirectionTarget(RecordingDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public List<PresentationFlowDirection> AppliedDirections { get; } = [];

        public List<bool> DispatcherAccess { get; } = [];

        public ValueTask<LocalizationResult> ApplyFlowDirectionAsync(
            PresentationFlowDirection direction,
            CultureState state,
            CancellationToken cancellationToken = default)
        {
            AppliedDirections.Add(direction);
            DispatcherAccess.Add(_dispatcher.IsOnDispatcher);

            return ValueTask.FromResult(LocalizationResult.Success());
        }
    }

    private sealed class RecordingResourceDictionaryTarget : IPresentationResourceDictionaryTarget
    {
        private readonly RecordingDispatcher _dispatcher;

        public RecordingResourceDictionaryTarget(RecordingDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public List<string> AppliedCultures { get; } = [];

        public List<IReadOnlyList<string>> AppliedPackageIds { get; } = [];

        public List<bool> DispatcherAccess { get; } = [];

        public List<string> RevokedPluginIds { get; } = [];

        public List<string?> RevokedContributionIds { get; } = [];

        public List<bool> RevokeDispatcherAccess { get; } = [];

        public LocalizationError? ApplyFailure { get; init; }

        public LocalizationError? RevokeFailure { get; init; }

        public ValueTask<LocalizationResult> ApplyResourcesAsync(
            CultureState state,
            CancellationToken cancellationToken = default)
        {
            AppliedCultures.Add(state.CurrentCulture.Name);
            AppliedPackageIds.Add(state.LoadedPackageIds.ToArray());
            DispatcherAccess.Add(_dispatcher.IsOnDispatcher);

            return ValueTask.FromResult(
                ApplyFailure is null
                    ? LocalizationResult.Success()
                    : LocalizationResult.Failed(ApplyFailure));
        }

        public ValueTask<LocalizationResult> RevokeResourcesAsync(
            PresentationResourceDictionaryRevocation revocation,
            CancellationToken cancellationToken = default)
        {
            RevokedPluginIds.Add(revocation.PluginId);
            RevokedContributionIds.Add(revocation.ContributionId);
            RevokeDispatcherAccess.Add(_dispatcher.IsOnDispatcher);

            return ValueTask.FromResult(
                RevokeFailure is null
                    ? LocalizationResult.Success()
                    : LocalizationResult.Failed(RevokeFailure));
        }
    }

    private sealed class RecordingCultureApplier : IPresentationCultureApplier
    {
        private readonly string _name;
        private readonly RecordingDispatcher _dispatcher;
        private long _sequence;

        public RecordingCultureApplier(string name, RecordingDispatcher dispatcher)
        {
            _name = name;
            _dispatcher = dispatcher;
        }

        public List<string> AppliedCultures { get; } = [];

        public LocalizationError? Failure { get; set; }

        public bool WasOnDispatcher { get; private set; }

        public ValueTask<LocalizationResult> ApplyCultureAsync(
            CultureState state,
            CancellationToken cancellationToken = default)
        {
            _sequence = _dispatcher.NextSequence();
            WasOnDispatcher = _dispatcher.IsOnDispatcher;
            AppliedCultures.Add($"{_name}:{state.CurrentCulture.Name}");

            return ValueTask.FromResult(
                Failure is null
                    ? LocalizationResult.Success()
                    : LocalizationResult.Failed(Failure));
        }

        public bool AppliedBefore(RecordingCultureApplier other)
        {
            return _sequence > 0 && _sequence < other._sequence;
        }
    }

    private sealed class RecordingDispatcher : IUiDispatcher
    {
        private long _sequence;

        public int PostCount { get; private set; }

        public bool IsOnDispatcher { get; private set; }

        public bool CheckAccess()
        {
            return IsOnDispatcher;
        }

        public ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default)
        {
            callback();

            return ValueTask.CompletedTask;
        }

        public ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(callback());
        }

        public async ValueTask PostAsync(
            Func<CancellationToken, ValueTask> callback,
            CancellationToken cancellationToken = default)
        {
            PostCount++;
            IsOnDispatcher = true;

            try
            {
                await callback(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                IsOnDispatcher = false;
            }
        }

        public long NextSequence()
        {
            return Interlocked.Increment(ref _sequence);
        }
    }
}
