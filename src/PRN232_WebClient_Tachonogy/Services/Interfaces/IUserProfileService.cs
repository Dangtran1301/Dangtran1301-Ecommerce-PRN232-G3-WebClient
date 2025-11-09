using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.Services.Interfaces;

public interface IUserProfileService
{
    Task<ApiResponse<UserProfileDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse> UpdateAsync(Guid id, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
}