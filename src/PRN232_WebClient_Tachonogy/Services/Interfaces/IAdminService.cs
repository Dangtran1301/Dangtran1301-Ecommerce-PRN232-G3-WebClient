using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.Services.Interfaces;

public interface IAdminService
{
    Task<ApiResponse<IEnumerable<AuthUserResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthUserResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<AuthUserResponse>> CreateAsync(RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse> UpdateRoleAsync(Guid id, RemoteAuthUserRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse> UpdateStatusAsync(Guid id, UpdateStatusAuthUserRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<IEnumerable<AuthUserResponse>>> FilterAsync(UserFilterRequest filter,
        CancellationToken cancellationToken = default);
}