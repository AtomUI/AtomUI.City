using AtomUI.City.Diagnostics;
using AtomUI.City.Mvvm;
using AtomUI.City.Presentation;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation.Tests;

public sealed class ValidationVisualStateBindingTests
{
    [Fact]
    public async Task ApplyAsyncMapsInvalidScopeToTargetOnUiDispatcher()
    {
        var dispatcher = new RecordingDispatcher();
        var binding = new ValidationVisualStateBinding(dispatcher);
        var target = new RecordingValidationTarget(dispatcher);
        var scope = new ValidationScope();
        scope.SetInvalid(
            "Name",
            "Name is required.",
            "Validation.Name.Required",
            ["Name"]);

        await binding.ApplyAsync(scope, target);

        Assert.True(target.WasOnDispatcher);
        Assert.Equal(ValidationStatus.Invalid, target.Snapshot?.Status);
        Assert.Equal("Name is required.", target.Snapshot?.Errors["Name"][0]);
        Assert.Equal("Validation.Name.Required", target.Snapshot?.Messages["Name"][0].MessageKey);
        Assert.Equal(1, dispatcher.InvokeCount);
    }

    [Fact]
    public async Task ApplyAsyncMapsFailedScopeExceptionToTarget()
    {
        var binding = new ValidationVisualStateBinding(new RecordingDispatcher());
        var target = new RecordingValidationTarget();
        var exception = new InvalidOperationException("validator failed");
        var scope = new ValidationScope();
        scope.SetInvalid("Name", "Name is required.");
        scope.SetFailed(exception);

        await binding.ApplyAsync(scope, target);

        Assert.Equal(ValidationStatus.Failed, target.Snapshot?.Status);
        Assert.Same(exception, target.Snapshot?.Exception);
        Assert.Empty(target.Snapshot!.Errors);
        Assert.Empty(target.Snapshot.Messages);
    }

    [Fact]
    public async Task ApplyAsyncRecordsAppliedDiagnostics()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var binding = new ValidationVisualStateBinding(new RecordingDispatcher(), diagnostics);
        var scope = new ValidationScope();
        scope.SetInvalid("Name", "Name is required.");

        await binding.ApplyAsync(scope, new RecordingValidationTarget());

        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ValidationVisualStateApplied &&
                record.Severity == HostDiagnosticSeverity.Info &&
                record.Message.Contains(nameof(ValidationStatus.Invalid), StringComparison.Ordinal) &&
                record.Message.Contains("Name", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ApplyAsyncRecordsFailureDiagnosticsAndPropagatesTargetFailure()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var binding = new ValidationVisualStateBinding(new RecordingDispatcher(), diagnostics);
        var target = new RecordingValidationTarget
        {
            Failure = new InvalidOperationException("visual state failed"),
        };
        var scope = new ValidationScope();
        scope.SetInvalid("Name", "Name is required.");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => binding.ApplyAsync(scope, target).AsTask());

        Assert.Equal("visual state failed", exception.Message);
        Assert.Contains(
            diagnostics.Records,
            record =>
                record.Code == PresentationDiagnosticIds.ValidationVisualStateApplyFailed &&
                record.Severity == HostDiagnosticSeverity.Error &&
                record.Message.Contains("visual state failed", StringComparison.Ordinal));
    }

    private sealed class RecordingValidationTarget : IValidationVisualStateTarget
    {
        private readonly RecordingDispatcher? _dispatcher;

        public RecordingValidationTarget()
        {
        }

        public RecordingValidationTarget(RecordingDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public ValidationVisualStateSnapshot? Snapshot { get; private set; }

        public bool WasOnDispatcher { get; private set; }

        public Exception? Failure { get; init; }

        public void ApplyValidationState(ValidationVisualStateSnapshot snapshot)
        {
            if (Failure is not null)
            {
                throw Failure;
            }

            Snapshot = snapshot;
            WasOnDispatcher = _dispatcher?.IsOnDispatcher ?? true;
        }
    }

    private sealed class RecordingDispatcher : IUiDispatcher
    {
        public bool IsOnDispatcher { get; private set; }

        public int InvokeCount { get; private set; }

        public bool CheckAccess()
        {
            return IsOnDispatcher;
        }

        public ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default)
        {
            InvokeCount++;
            cancellationToken.ThrowIfCancellationRequested();
            IsOnDispatcher = true;

            try
            {
                callback();
            }
            finally
            {
                IsOnDispatcher = false;
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)
        {
            InvokeCount++;
            cancellationToken.ThrowIfCancellationRequested();
            IsOnDispatcher = true;

            try
            {
                return ValueTask.FromResult(callback());
            }
            finally
            {
                IsOnDispatcher = false;
            }
        }

        public ValueTask PostAsync(
            Func<CancellationToken, ValueTask> callback,
            CancellationToken cancellationToken = default)
        {
            return callback(cancellationToken);
        }
    }
}
