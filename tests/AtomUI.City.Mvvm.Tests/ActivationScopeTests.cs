using AtomUI.City.Mvvm;

namespace AtomUI.City.Mvvm.Tests;

public sealed class ActivationScopeTests
{
    [Fact]
    public void DisposeDisposesRegisteredResourcesAndCancelsToken()
    {
        var disposable = new TestDisposable();
        using var scope = new ActivationScope();

        scope.Add(disposable);
        scope.Dispose();

        Assert.True(disposable.IsDisposed);
        Assert.True(scope.CancellationToken.IsCancellationRequested);
    }

    private sealed class TestDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }
}
