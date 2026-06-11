using System.Threading;

namespace AtomUI.City.Mvvm;

public sealed class ActivationScopeAccessor
{
    private readonly AsyncLocal<IActivationScope?> _current = new();

    public IActivationScope? Current => _current.Value;

    public IDisposable Push(IActivationScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var previous = _current.Value;
        _current.Value = scope;

        return new RestoreHandle(this, previous);
    }

    private sealed class RestoreHandle : IDisposable
    {
        private readonly ActivationScopeAccessor _accessor;
        private readonly IActivationScope? _previous;
        private bool _disposed;

        public RestoreHandle(
            ActivationScopeAccessor accessor,
            IActivationScope? previous)
        {
            _accessor = accessor;
            _previous = previous;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _accessor._current.Value = _previous;
        }
    }
}
