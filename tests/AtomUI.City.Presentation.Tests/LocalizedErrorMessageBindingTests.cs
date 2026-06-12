using System.Globalization;
using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Presentation;
using AtomUI.City.Security;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation.Tests;

public sealed class LocalizedErrorMessageBindingTests
{
    [Fact]
    public async Task BindAsyncAppliesErrorMessageDescriptor()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Errors.Data.Timeout", "请求 {0} 超时"));
        var localization = new LocalizationService(
            [zh.Descriptor],
            [new TestLanguagePackageProvider(zh)]);
        var dispatcher = new RecordingDispatcher();
        var binding = new LocalizedErrorMessageBinding(localization, dispatcher);
        var target = new ErrorMessageTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new ErrorMessageDescriptor(
                "data-timeout",
                "Request timed out.",
                "Errors.Data.Timeout",
                ["GET /orders"]),
            target);

        Assert.Equal("请求 GET /orders 超时", target.Message);
        Assert.Equal(1, dispatcher.InvokeCount);
    }

    [Fact]
    public async Task BoundErrorMessageRefreshesWhenCultureChanges()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Errors.Data.Timeout", "请求 {0} 超时"));
        var en = Package("Host.en-US", "en-US", ("Errors.Data.Timeout", "Request {0} timed out."));
        var localization = new LocalizationService(
            [zh.Descriptor, en.Descriptor],
            [new TestLanguagePackageProvider(zh, en)]);
        var binding = new LocalizedErrorMessageBinding(localization, new RecordingDispatcher());
        var target = new ErrorMessageTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new ErrorMessageDescriptor(
                "data-timeout",
                "Request timed out.",
                "Errors.Data.Timeout",
                ["GET /orders"]),
            target);
        await localization.SetCultureAsync("en-US");

        Assert.Equal("Request GET /orders timed out.", target.Message);
    }

    [Fact]
    public async Task BindAsyncUsesLiteralMessageWhenMessageKeyIsMissing()
    {
        var zh = Package("Host.zh-CN", "zh-CN");
        var localization = new LocalizationService(
            [zh.Descriptor],
            [new TestLanguagePackageProvider(zh)]);
        var binding = new LocalizedErrorMessageBinding(localization, new RecordingDispatcher());
        var target = new ErrorMessageTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new ErrorMessageDescriptor("data-unknown", "Request failed."),
            target);

        Assert.Equal("Request failed.", target.Message);
    }

    [Fact]
    public async Task BindAsyncMapsAuthorizationResultToErrorMessage()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Errors.AuthorizationForbidden", "没有权限执行 {0}"));
        var localization = new LocalizationService(
            [zh.Descriptor],
            [new TestLanguagePackageProvider(zh)]);
        var binding = new LocalizedErrorMessageBinding(localization, new RecordingDispatcher());
        var target = new ErrorMessageTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            AuthorizationResult.Forbidden("Orders.Write"),
            target);

        Assert.Equal("没有权限执行 Orders.Write", target.Message);
    }

    [Fact]
    public async Task ActivationScopeDisposesErrorMessageBinding()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Errors.Data.Timeout", "请求 {0} 超时"));
        var en = Package("Host.en-US", "en-US", ("Errors.Data.Timeout", "Request {0} timed out."));
        var localization = new LocalizationService(
            [zh.Descriptor, en.Descriptor],
            [new TestLanguagePackageProvider(zh, en)]);
        var binding = new LocalizedErrorMessageBinding(localization, new RecordingDispatcher());
        var target = new ErrorMessageTarget();
        using var activationScope = new ActivationScope();

        await localization.SetCultureAsync("zh-CN");
        await binding.BindAsync(
            new ErrorMessageDescriptor(
                "data-timeout",
                "Request timed out.",
                "Errors.Data.Timeout",
                ["GET /orders"]),
            target,
            activationScope);
        activationScope.Dispose();
        await localization.SetCultureAsync("en-US");

        Assert.Equal("请求 GET /orders 超时", target.Message);
    }

    private static LanguagePackage Package(
        string packageId,
        string cultureName,
        params (string Key, string Value)[] resources)
    {
        return LanguagePackage.Create(
            new LanguagePackageDescriptor(
                packageId,
                CultureInfo.GetCultureInfo(cultureName),
                ResourceScope.Host),
            resources.ToDictionary(resource => resource.Key, resource => resource.Value));
    }

    private sealed class ErrorMessageTarget : ILocalizedErrorMessageTarget
    {
        public string? Message { get; set; }
    }

    private sealed class RecordingDispatcher : IUiDispatcher
    {
        public int InvokeCount { get; private set; }

        public bool CheckAccess() => true;

        public ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default)
        {
            InvokeCount++;
            callback();

            return ValueTask.CompletedTask;
        }

        public ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)
        {
            InvokeCount++;

            return ValueTask.FromResult(callback());
        }

        public ValueTask PostAsync(
            Func<CancellationToken, ValueTask> callback,
            CancellationToken cancellationToken = default)
        {
            return callback(cancellationToken);
        }
    }

    private sealed class TestLanguagePackageProvider : ILanguagePackageProvider
    {
        private readonly Dictionary<string, LanguagePackage> _packages;

        public TestLanguagePackageProvider(params LanguagePackage[] packages)
        {
            _packages = packages.ToDictionary(package => package.Descriptor.PackageId, StringComparer.Ordinal);
        }

        public LanguagePackageProviderKind Kind => LanguagePackageProviderKind.InMemory;

        public ValueTask<LanguagePackageLoadResult> LoadAsync(
            LanguagePackageDescriptor descriptor,
            CancellationToken cancellationToken = default)
        {
            return _packages.TryGetValue(descriptor.PackageId, out var package)
                ? ValueTask.FromResult(LanguagePackageLoadResult.Success(package))
                : ValueTask.FromResult(
                    LanguagePackageLoadResult.Failed(
                        new LocalizationError(
                            LocalizationErrorKind.PackageNotFound,
                            "Package was not found.")));
        }
    }
}
