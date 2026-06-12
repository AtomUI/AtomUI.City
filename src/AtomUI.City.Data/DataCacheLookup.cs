namespace AtomUI.City.Data;

public sealed class DataCacheLookup<TResponse>
{
    private DataCacheLookup(bool hit, TResponse? value)
    {
        IsHit = hit;
        Value = value;
    }

    public bool IsHit { get; }

    public TResponse? Value { get; }

    public static DataCacheLookup<TResponse> Miss()
    {
        return new DataCacheLookup<TResponse>(hit: false, value: default);
    }

    public static DataCacheLookup<TResponse> Hit(TResponse? value)
    {
        return new DataCacheLookup<TResponse>(hit: true, value);
    }
}
