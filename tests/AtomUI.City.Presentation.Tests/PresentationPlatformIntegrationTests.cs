using AtomUI.City.Presentation;
using AtomUI.City.Testing;
using Avalonia.Threading;

namespace AtomUI.City.Presentation.Tests;

public sealed class PresentationPlatformIntegrationTests
{
    [Fact]
    [Trait("Category", TestLayerNames.PlatformIntegration)]
    [TestLayer(TestLayer.PlatformIntegration)]
    public async Task AvaloniaDispatcherExecutesCallbacksOnTheCurrentUiThread()
    {
        var dispatcher = new AvaloniaUiDispatcher(Dispatcher.CurrentDispatcher);
        var callbackThread = -1;

        var result = await dispatcher.InvokeAsync(() =>
        {
            callbackThread = Environment.CurrentManagedThreadId;

            return 42;
        });

        Assert.True(dispatcher.CheckAccess());
        Assert.Equal(Environment.CurrentManagedThreadId, callbackThread);
        Assert.Equal(42, result);
    }
}
