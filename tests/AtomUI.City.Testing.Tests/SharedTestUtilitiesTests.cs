using AtomUI.City.Testing;

namespace AtomUI.City.Testing.Tests;

public sealed class SharedTestUtilitiesTests
{
    [Fact]
    public void TestDirectoryCreatesAndCleansUniqueDirectory()
    {
        string rootPath;

        using (var directory = TestDirectory.Create("shared-utilities"))
        {
            rootPath = directory.RootPath;
            File.WriteAllText(directory.GetPath("nested", "file.txt"), "content");

            Assert.True(Directory.Exists(rootPath));
            Assert.True(File.Exists(Path.Combine(rootPath, "nested", "file.txt")));
        }

        Assert.False(Directory.Exists(rootPath));
    }

    [Fact]
    public async Task DisposableTrackerDisposesResourcesInReverseOrder()
    {
        var calls = new List<string>();
        var tracker = new DisposableTracker();

        tracker.Track(new DelegateDisposable(() => calls.Add("first")));
        tracker.Track(new DelegateAsyncDisposable(() =>
        {
            calls.Add("second");
            return ValueTask.CompletedTask;
        }));

        await tracker.DisposeAsync();

        Assert.Equal(["second", "first"], calls);
    }

    [Fact]
    public void TestDiagnosticsCollectsEntriesInOrder()
    {
        var diagnostics = new TestDiagnostics();

        diagnostics.Add("CITY1001", "first");
        diagnostics.Add("CITY1002", "second", TestLayer.FrameworkIntegration);

        Assert.Collection(
            diagnostics.Entries,
            entry =>
            {
                Assert.Equal("CITY1001", entry.Code);
                Assert.Null(entry.Layer);
            },
            entry =>
            {
                Assert.Equal("CITY1002", entry.Code);
                Assert.Equal(TestLayer.FrameworkIntegration, entry.Layer);
            });
        Assert.True(diagnostics.Contains("CITY1002"));
    }

    private sealed class DelegateDisposable : IDisposable
    {
        private readonly Action _dispose;

        public DelegateDisposable(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            _dispose();
        }
    }

    private sealed class DelegateAsyncDisposable : IAsyncDisposable
    {
        private readonly Func<ValueTask> _disposeAsync;

        public DelegateAsyncDisposable(Func<ValueTask> disposeAsync)
        {
            _disposeAsync = disposeAsync;
        }

        public ValueTask DisposeAsync()
        {
            return _disposeAsync();
        }
    }
}
