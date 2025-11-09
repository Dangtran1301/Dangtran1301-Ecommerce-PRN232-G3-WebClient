using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients;

public class AdminClient(IApiClient apiClient) : IAdminClient
{
    private const string BaseUrl = "auth-users";

    public Task<ApiResponse<AuthUserResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => apiClient.GetAsync<AuthUserResponse>($"{BaseUrl}/{id}", cancellationToken);

    public Task<ApiResponse<IEnumerable<AuthUserResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
        => apiClient.GetAsync<IEnumerable<AuthUserResponse>>(BaseUrl, cancellationToken);

    public Task<ApiResponse<AuthUserResponse>> CreateAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<AuthUserResponse>(BaseUrl, request, cancellationToken);

    public async Task<ApiResponse> UpdateRoleAsync(Guid id, RemoteAuthUserRequest request, CancellationToken cancellationToken = default)
        => await apiClient.PutAsync<ApiResponse>($"{BaseUrl}/{id}/role", request, cancellationToken);

    public async Task<ApiResponse> UpdateStatusAsync(Guid id, UpdateStatusAuthUserRequest request, CancellationToken cancellationToken = default)
        => await apiClient.PutAsync<ApiResponse>($"{BaseUrl}/{id}/status", request, cancellationToken);

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => await apiClient.DeleteAsync($"{BaseUrl}/{id}", cancellationToken);

    public async Task<ApiResponse<IEnumerable<AuthUserResponse>>> FilterAsync(UserFilterRequest filter, CancellationToken cancellationToken = default)
        => await apiClient.PostAsync<IEnumerable<AuthUserResponse>>($"{BaseUrl}/filter", filter, cancellationToken);
}