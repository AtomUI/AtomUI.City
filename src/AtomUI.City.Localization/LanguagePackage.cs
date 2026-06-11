namespace AtomUI.City.Localization;

public sealed class LanguagePackage : IDisposable
{
    private readonly IReadOnlyDictionary<string, string> _strings;
    private bool _disposed;

    private LanguagePackage(
        LanguagePackageDescriptor descriptor,
        IReadOnlyDictionary<string, string> strings)
    {
        Descriptor = descriptor;
        _strings = new Dictionary<string, string>(strings, StringComparer.Ordinal);
    }

    public LanguagePackageDescriptor Descriptor { get; }

    public bool IsDisposed => _disposed;

    public static LanguagePackage Create(
        LanguagePackageDescriptor descriptor,
        IReadOnlyDictionary<string, string> strings)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(strings);

        return new LanguagePackage(descriptor, strings);
    }

    public bool TryGetString(string key, out string value)
    {
        if (_disposed)
        {
            value = string.Empty;

            return false;
        }

        return _strings.TryGetValue(key, out value!);
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
