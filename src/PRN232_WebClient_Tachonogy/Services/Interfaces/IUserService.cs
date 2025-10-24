using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.Services.Interfaces;

public interface IUserService
{
    Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(UserFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}