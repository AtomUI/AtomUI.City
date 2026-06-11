using AtomUI.City.Hosting;
using AtomUI.City.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Core.Tests;

public sealed class UiDispatcherIntegrationTests
{
    [Fact]
    public async Task ApplicationHostRegistersUnavailableUiDispatcherByDefault()
    {
        await using var host = ApplicationHost.CreateBuilder().Build();

        var dispatcher = host.Services.GetRequiredService<IUiDispatcher>();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.InvokeAsync(() => { }).AsTask());

        Assert.False(dispatcher.CheckAccess());
        Assert.Contains("UI dispatcher", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApplicationHostAllowsUiDispatcherReplacementBeforeBuild()
    {
        var dispatcher = new InlineUiDispatcher();
        var builder = ApplicationHost.CreateBuilder();

        builder.ConfigureServices(services => services.AddSingleton<IUiDispatcher>(dispatcher));

        await using var host = builder.Build();

        Assert.Same(dispatcher, host.Services.GetRequiredService<IUiDispatcher>());

        var wasCalled = false;
        await dispatcher.InvokeAsync(() => wasCalled = true);

        Assert.True(wasCalled);
    }

    private sealed class InlineUiDispatcher : IUiDispatcher
    {
        public bool CheckAccess()
        {
            return true;
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

        public ValueTask PostAsync(
            Func<CancellationToken, ValueTask> callback,
            CancellationToken cancellationToken = default)
        {
            return callback(cancellationToken);
        }
    }
}
