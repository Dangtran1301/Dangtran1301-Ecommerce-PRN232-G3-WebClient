using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;
using PRN232_WebClient_Tachonogy.Models;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Services;

public class AuthService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<AuthService> logger) : IAuthService
{
    private const string BaseEndpoint = "auth";
    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/login", dto, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(cancellationToken: cancellationToken);

        return result ?? new ApiResponse<LoginResponse> { Success = false };
    }

    public async Task<ApiResponse> RegisterAsync(RegisterRequest dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/register", dto, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(cancellationToken);

        if (result == null)
        {
            return new ApiResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "DESERIALIZATION_ERROR",
                    Message = "Invalid response from server"
                },
                Timestamp = DateTime.UtcNow
            };
        }
        return result;
    }

    public async Task<ApiResponse<LoginResponse>> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/refresh", dto, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(cancellationToken: cancellationToken);

        if (result?.Data is not null && result.Success)
        {
            tokenProvider.SetTokens(result.Data.AccessToken, result.Data.RefreshToken);
        }

        return result ?? new ApiResponse<LoginResponse> { Success = false };
    }

    public async Task<ApiResponse<bool>> LogoutAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/logout", dto, cancellationToken);
        tokenProvider.ClearTokens();
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken) ?? new ApiResponse<bool> { Success = false };
    }
}