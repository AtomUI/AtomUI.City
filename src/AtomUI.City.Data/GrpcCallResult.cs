namespace AtomUI.City.Data;

public sealed class GrpcCallResult<T>
{
    private GrpcCallResult(
        bool succeeded,
        T? value,
        GrpcStatusCode statusCode,
        string? detail)
    {
        Succeeded = succeeded;
        Value = value;
        StatusCode = statusCode;
        Detail = detail;
    }

    public bool Succeeded { get; }

    public T? Value { get; }

    public GrpcStatusCode StatusCode { get; }

    public string? Detail { get; }

    public static GrpcCallResult<T> Success(T value)
    {
        return new GrpcCallResult<T>(succeeded: true, value, GrpcStatusCode.OK, detail: null);
    }

    public static GrpcCallResult<T> Failed(GrpcStatusCode statusCode, string? detail = null)
    {
        return new GrpcCallResult<T>(succeeded: false, value: default, statusCode, detail);
    }
}
