using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients;

public class UserApiClient(IApiClient apiClient) : IUserApiClient
{
    private const string BaseUrl = "users";

    public async Task<ApiResponse<PagedResult<UserDto>>> FilterAsync(UserFilterDto filter, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            queryParams.Add($"Keyword={Uri.EscapeDataString(filter.Keyword)}");
        if (filter.Role.HasValue)
            queryParams.Add($"Role={filter.Role}");
        if (filter.Status.HasValue)
            queryParams.Add($"Status={filter.Status}");
        if (filter.Gender.HasValue)
            queryParams.Add($"Gender={filter.Gender}");
        if (filter.DobFrom.HasValue)
            queryParams.Add($"DobFrom={filter.DobFrom:yyyy-MM-dd}");
        if (filter.DobTo.HasValue)
            queryParams.Add($"DobTo={filter.DobTo:yyyy-MM-dd}");
        queryParams.Add($"PageIndex={filter.PageIndex}");
        queryParams.Add($"PageSize={filter.PageSize}");
        queryParams.Add($"OrderBy={filter.OrderBy}");
        queryParams.Add($"Descending={filter.Descending}");

        var query = string.Join("&", queryParams);
        var endpoint = $"{BaseUrl}/filter/paged?{query}";

        return await apiClient.GetAsync<PagedResult<UserDto>>(endpoint, cancellationToken);
    }

    public Task<ApiResponse<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => apiClient.GetAsync<UserDto>($"{BaseUrl}/{id}", cancellationToken);

    public Task<ApiResponse<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<UserDto>(BaseUrl, request, cancellationToken);

    public Task<ApiResponse<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
        => apiClient.PutAsync<UserDto>($"{BaseUrl}/{id}", request, cancellationToken);

    public Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => apiClient.DeleteAsync($"{BaseUrl}/{id}", cancellationToken);
}