using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Services;

public class UserService(IUserApiClient userApiClient, ILogger<UserService> logger) : IUserService
{
    public async Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(UserFilterDto filter, CancellationToken cancellationToken = default)
    {
        return await userApiClient.FilterAsync(filter, cancellationToken);
    }

    public async Task<ApiResponse<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await userApiClient.GetByIdAsync(id, cancellationToken);
    }

    public async Task<ApiResponse<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        return await userApiClient.CreateAsync(request, cancellationToken);
    }

    public async Task<ApiResponse<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        return await userApiClient.UpdateAsync(id, request, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await userApiClient.DeleteAsync(id, cancellationToken);
    }
}