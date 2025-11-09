using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);

    Task<ApiResponse> RegisterAsync(RegisterRequest dto, CancellationToken cancellationToken = default);

    Task<ApiResponse<LoginResponse>> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> LogoutAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default);

    Task<ApiResponse> ChangePasswordAsync(Guid id, ChangePasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest dto, CancellationToken cancellationToken = default);

    Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest dto, CancellationToken cancellationToken = default);
}