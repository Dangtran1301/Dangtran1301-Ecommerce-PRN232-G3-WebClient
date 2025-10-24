using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Services;

public class AuthService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<AuthService> logger) : IAuthService
{
    private const string BaseEndpoint = "auth";
    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/login", dto, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>(cancellationToken: cancellationToken);

        if (result?.Data is not null && result.Success)
        {
            tokenProvider.SetTokens(result.Data.AccessToken, result.Data.RefreshToken);
        }

        return result ?? new ApiResponse<LoginResponseDto> { Success = false };
    }

    public async Task<ApiResponse<LoginResponseDto>> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/refresh", dto, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>(cancellationToken: cancellationToken);

        if (result?.Data is not null && result.Success)
        {
            tokenProvider.SetTokens(result.Data.AccessToken, result.Data.RefreshToken);
        }

        return result ?? new ApiResponse<LoginResponseDto> { Success = false };
    }

    public async Task<ApiResponse<bool>> LogoutAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/logout", dto, cancellationToken);
        tokenProvider.ClearTokens();
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken) ?? new ApiResponse<bool> { Success = false };
    }
}