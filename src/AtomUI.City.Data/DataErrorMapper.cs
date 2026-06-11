using System.Net;

namespace AtomUI.City.Data;

public static class DataErrorMapper
{
    public static DataError FromHttpStatusCode(HttpStatusCode statusCode)
    {
        var kind = statusCode switch
        {
            HttpStatusCode.BadRequest => DataErrorKind.BadRequest,
            HttpStatusCode.Unauthorized => DataErrorKind.AuthenticationRequired,
            HttpStatusCode.Forbidden => DataErrorKind.AuthorizationForbidden,
            HttpStatusCode.NotFound => DataErrorKind.NotFound,
            HttpStatusCode.Conflict => DataErrorKind.Conflict,
            HttpStatusCode.RequestTimeout => DataErrorKind.Timeout,
            >= HttpStatusCode.InternalServerError => DataErrorKind.ServerError,
            _ => DataErrorKind.TransportError,
        };

        return new DataError(
            kind,
            $"HTTP request failed with status code {(int)statusCode}.",
            ((int)statusCode).ToString());
    }

    public static DataError FromGrpcStatus(GrpcStatusCode statusCode, string? detail = null)
    {
        var kind = statusCode switch
        {
            GrpcStatusCode.Cancelled => DataErrorKind.Cancelled,
            GrpcStatusCode.DeadlineExceeded => DataErrorKind.DeadlineExceeded,
            GrpcStatusCode.Unauthenticated => DataErrorKind.AuthenticationRequired,
            GrpcStatusCode.PermissionDenied => DataErrorKind.AuthorizationForbidden,
            GrpcStatusCode.NotFound => DataErrorKind.NotFound,
            GrpcStatusCode.AlreadyExists => DataErrorKind.Conflict,
            GrpcStatusCode.Unavailable => DataErrorKind.ServiceUnavailable,
            GrpcStatusCode.Internal => DataErrorKind.ServerError,
            _ => DataErrorKind.Unknown,
        };

        return new DataError(
            kind,
            detail ?? $"gRPC call failed with status '{statusCode}'.",
            statusCode.ToString());
    }
}
