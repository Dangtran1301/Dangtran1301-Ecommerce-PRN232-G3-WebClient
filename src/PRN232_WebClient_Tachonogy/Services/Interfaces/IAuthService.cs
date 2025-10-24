using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<LoginResponseDto>> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> LogoutAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default);
}