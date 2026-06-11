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

    [Fact]
    public void DisposeDisposesRegisteredResourcesInReverseOrder()
    {
        var calls = new List<string>();
        using var scope = new ActivationScope();

        scope.Add(new TestDisposable(() => calls.Add("first")));
        scope.Add(new TestDisposable(() => calls.Add("second")));

        scope.Dispose();

        Assert.Equal(["second", "first"], calls);
    }

    [Fact]
    public void AddAfterDisposeImmediatelyDisposesResource()
    {
        var disposable = new TestDisposable();
        using var scope = new ActivationScope();

        scope.Dispose();
        scope.Add(disposable);

        Assert.True(disposable.IsDisposed);
    }

    [Fact]
    public async Task DisposeAsyncDisposesAsyncResourcesInReverseOrder()
    {
        var calls = new List<string>();
        var scope = new ActivationScope();

        scope.AddAsync(new TestAsyncDisposable(() => calls.Add("first")));
        scope.AddAsync(new TestAsyncDisposable(() => calls.Add("second")));

        await scope.DisposeAsync();

        Assert.Equal(["second", "first"], calls);
        Assert.True(scope.CancellationToken.IsCancellationRequested);
    }

    private sealed class TestDisposable : IDisposable
    {
        private readonly Action? _dispose;

        public TestDisposable(Action? dispose = null)
        {
            _dispose = dispose;
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
            _dispose?.Invoke();
        }
    }

    private sealed class TestAsyncDisposable : IAsyncDisposable
    {
        private readonly Action _dispose;

        public TestAsyncDisposable(Action dispose)
        {
            _dispose = dispose;
        }

        public ValueTask DisposeAsync()
        {
            _dispose();

            return ValueTask.CompletedTask;
        }
    }
}
