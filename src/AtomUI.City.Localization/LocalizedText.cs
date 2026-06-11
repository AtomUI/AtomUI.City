using System.Globalization;

namespace AtomUI.City.Localization;

internal sealed class LocalizedText : ILocalizedText
{
    private readonly LocalizationService _owner;
    private readonly object _gate = new();
    private LocalizedString _current;
    private long _revision;
    private bool _disposed;

    private LocalizedText(
        LocalizationService owner,
        LocalizedString current,
        long revision)
    {
        _owner = owner;
        _current = current;
        _revision = revision;
    }

    public event EventHandler<LocalizedTextChangedEventArgs>? Changed;

    public string Key
    {
        get
        {
            lock (_gate)
            {
                return _current.Key;
            }
        }
    }

    public string Value
    {
        get
        {
            lock (_gate)
            {
                return _current.Value;
            }
        }
    }

    public CultureInfo Culture
    {
        get
        {
            lock (_gate)
            {
                return _current.Culture;
            }
        }
    }

    public long Revision
    {
        get
        {
            lock (_gate)
            {
                return _revision;
            }
        }
    }

    public bool IsFallback
    {
        get
        {
            lock (_gate)
            {
                return _current.IsFallback;
            }
        }
    }

    public bool IsMissing
    {
        get
        {
            lock (_gate)
            {
                return _current.IsMissing;
            }
        }
    }

    public static async ValueTask<LocalizedText> CreateAsync(
        LocalizationService owner,
        string key,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var current = await owner.GetStringAsync(key, cancellationToken).ConfigureAwait(false);

        return new LocalizedText(owner, current, owner.CultureRevision);
    }

    public async ValueTask RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (IsDisposed)
        {
            return;
        }

        string key;

        lock (_gate)
        {
            key = _current.Key;
        }

        var next = await _owner.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
        LocalizedTextChangedEventArgs args;

        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            var nextRevision = _owner.CultureRevision;
            if (IsSameText(_current, next) && _revision == nextRevision)
            {
                return;
            }

            _current = next;
            _revision = nextRevision;
            args = new LocalizedTextChangedEventArgs(next, nextRevision);
        }

        OnChanged(args);
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        _owner.UnregisterLocalizedText(this);
    }

    private bool IsDisposed
    {
        get
        {
            lock (_gate)
            {
                return _disposed;
            }
        }
    }

    private void OnChanged(LocalizedTextChangedEventArgs args)
    {
        var handlers = Changed;
        if (handlers is null)
        {
            return;
        }

        foreach (EventHandler<LocalizedTextChangedEventArgs> handler in handlers.GetInvocationList())
        {
            try
            {
                handler(this, args);
            }
            catch (Exception exception)
            {
                _owner.WriteTextRefreshFailed(Key, exception);
            }
        }
    }

    private static bool IsSameText(LocalizedString left, LocalizedString right)
    {
        return string.Equals(left.Key, right.Key, StringComparison.Ordinal)
            && string.Equals(left.Value, right.Value, StringComparison.Ordinal)
            && string.Equals(left.Culture.Name, right.Culture.Name, StringComparison.OrdinalIgnoreCase)
            && left.IsFallback == right.IsFallback
            && left.IsMissing == right.IsMissing;
    }
}
