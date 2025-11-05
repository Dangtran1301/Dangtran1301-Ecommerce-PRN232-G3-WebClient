using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Services;

public class UserProfileService(IUserProfileApiClient userProfileApiClient, ILogger<UserProfileService> logger) : IUserProfileService
{

    public Task<ApiResponse<UserProfileDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => userProfileApiClient.GetByIdAsync(id, cancellationToken);

    public Task<ApiResponse> UpdateAsync(Guid id, UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
        => userProfileApiClient.UpdateAsync(id, request, cancellationToken);
}