using PRN232_WebClient_Tachonogy.Models;
using System.Text.Json.Serialization;

namespace PRN232_WebClient_Tachonogy.DTOs;

public class ApiResponse
{
    [JsonPropertyOrder(1)]
    public bool Success { get; set; }

    [JsonPropertyOrder(2)]
    public Error? Error { get; set; }

    [JsonPropertyOrder(4)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static implicit operator ApiResponse(Result result)
        => new()
        {
            Success = result.IsSuccess,
            Error = result.IsSuccess ? null : result.Error
        };
}

public class ApiResponse<T> : ApiResponse
{
    [JsonPropertyOrder(3)]
    public T? Data { get; set; }

    public static implicit operator ApiResponse<T>(Result<T> result)
        => new()
        {
            Success = result.IsSuccess,
            Error = result.IsSuccess ? null : result.Error,
            Data = result.Value
        };

    public static implicit operator ApiResponse<T>(T value)
        => new()
        {
            Success = true,
            Data = value
        };
}