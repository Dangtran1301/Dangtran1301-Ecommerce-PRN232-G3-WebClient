using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients.Interfaces;

public interface IUserProfileApiClient
{
    Task<ApiResponse<UserProfileDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse> UpdateAsync(Guid id, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
}