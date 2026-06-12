using System.Globalization;

namespace AtomUI.City.Localization;

internal sealed class LocalizedMessageText : ILocalizedText
{
    private readonly LocalizationService _owner;
    private readonly IReadOnlyList<object?> _arguments;
    private readonly object _gate = new();
    private LocalizedMessage _current;
    private long _revision;
    private bool _disposed;

    private LocalizedMessageText(
        LocalizationService owner,
        IReadOnlyList<object?> arguments,
        LocalizedMessage current,
        long revision)
    {
        _owner = owner;
        _arguments = arguments.ToArray();
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

    public static async ValueTask<LocalizedMessageText> CreateAsync(
        LocalizationService owner,
        string key,
        IReadOnlyList<object?> arguments,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(arguments);

        var current = await owner.GetMessageAsync(key, arguments, cancellationToken).ConfigureAwait(false);

        return new LocalizedMessageText(owner, arguments, current, owner.CultureRevision);
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

        var next = await _owner.GetMessageAsync(key, _arguments, cancellationToken).ConfigureAwait(false);
        LocalizedTextChangedEventArgs args;

        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            var nextRevision = _owner.CultureRevision;
            if (IsSameMessage(_current, next) && _revision == nextRevision)
            {
                return;
            }

            _current = next;
            _revision = nextRevision;
            args = new LocalizedTextChangedEventArgs(ToLocalizedString(next), nextRevision);
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

    private static bool IsSameMessage(LocalizedMessage left, LocalizedMessage right)
    {
        return string.Equals(left.Key, right.Key, StringComparison.Ordinal)
            && string.Equals(left.Value, right.Value, StringComparison.Ordinal)
            && string.Equals(left.Culture.Name, right.Culture.Name, StringComparison.OrdinalIgnoreCase)
            && left.IsFallback == right.IsFallback
            && left.IsMissing == right.IsMissing
            && left.IsFormatFailed == right.IsFormatFailed;
    }

    private static LocalizedString ToLocalizedString(LocalizedMessage message)
    {
        if (message.IsMissing)
        {
            return LocalizedString.Missing(message.Key, message.Culture);
        }

        return message.IsFallback
            ? LocalizedString.Fallback(message.Key, message.Value, message.Culture)
            : LocalizedString.Found(message.Key, message.Value, message.Culture);
    }
}
