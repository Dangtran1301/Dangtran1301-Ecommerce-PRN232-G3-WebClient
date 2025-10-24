namespace PRN232_WebClient_Tachonogy.Models;

public class Error
{
    public string Code { get; init; } = string.Empty;
    public string? Message { get; init; } = string.Empty;
    public object? Details { get; init; }

    public static readonly Error None = new() { Code = "None", Message = "" };

    public static Error Validation(string message, object? details = null)
        => new() { Code = ErrorCodes.Validation, Message = message, Details = details };

    public static Error NotFound(string message)
        => new() { Code = ErrorCodes.NotFound, Message = message };

    public static Error Conflict(string message)
        => new() { Code = ErrorCodes.Conflict, Message = message };

    public static Error Failure(string message, object? details = null)
        => new() { Code = ErrorCodes.BadRequest, Message = message, Details = details };

    public static Error Internal(string message, object? details = null)
        => new() { Code = ErrorCodes.InternalServerError, Message = message, Details = details };

    public static Error Unauthorized(string message, object? detail = null)
        => new() { Code = ErrorCodes.Unauthorized, Message = message, Details = detail };

    public static Error Forbidden(string message)
        => new() { Code = ErrorCodes.Forbidden, Message = message };

    public static Error TooManyRequests(string message)
        => new() { Code = ErrorCodes.TooManyRequests, Message = message };

    public override string ToString()
        => Details is null
            ? $"{Code}: {Message}"
            : $"{Code}: {Message} | Details: {Details}";
}

public static class ErrorCodes
{
    // --- 4xx: Client Errors ---
    public const string Validation = "VALIDATION";                 // 400

    public const string BadRequest = "BAD_REQUEST";                // 400
    public const string Unauthorized = "UNAUTHORIZED";             // 401
    public const string Forbidden = "FORBIDDEN";                   // 403
    public const string NotFound = "NOT_FOUND";                    // 404
    public const string Conflict = "CONFLICT";                     // 409
    public const string Gone = "GONE";                             // 410 (resource no longer available)
    public const string PayloadTooLarge = "PAYLOAD_TOO_LARGE";     // 413
    public const string UnsupportedMediaType = "UNSUPPORTED_MEDIA_TYPE"; // 415
    public const string TooManyRequests = "TOO_MANY_REQUESTS";     // 429
    public const string UnprocessableEntity = "UNPROCESSABLE_ENTITY"; // 422 (validation logic beyond schema)

    // --- 5xx: Server Errors ---
    public const string InternalServerError = "INTERNAL_SERVER_ERROR"; // 500

    public const string NotImplemented = "NOT_IMPLEMENTED";         // 501
    public const string BadGateway = "BAD_GATEWAY";                 // 502
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE"; // 503
    public const string GatewayTimeout = "GATEWAY_TIMEOUT";         // 504
    public const string DatabaseError = "DATABASE_ERROR";           // Custom
    public const string IntegrationError = "INTEGRATION_ERROR";     // Custom for external API failure
    public const string Unknown = "UNKNOWN_ERROR";                  // Fallback
}