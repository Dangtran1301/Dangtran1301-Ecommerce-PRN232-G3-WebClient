using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients;

public class UserProfileApiClient(IApiClient apiClient) : IUserProfileApiClient
{
    private const string BaseUrl = "user-profiles";

    public async Task<ApiResponse<UserProfileDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await apiClient.GetAsync<UserProfileDto>($"{BaseUrl}/{id}", cancellationToken);

    public async Task<ApiResponse> UpdateAsync(Guid id, UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
        => await apiClient.PutAsync<ApiResponse>($"{BaseUrl}/{id}", request, cancellationToken);
}