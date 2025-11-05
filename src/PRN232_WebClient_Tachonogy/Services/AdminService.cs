using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Models;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Services;

public class AdminService(IAdminClient adminClient, ILogger<AdminService> logger) : IAdminService
{
    public async Task<ApiResponse<IEnumerable<AuthUserResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await adminClient.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while getting all users");
            return new ApiResponse<IEnumerable<AuthUserResponse>>
            {
                Success = false,
                Error = new Error
                {
                    Code = "ADMIN_GET_ALL_FAILED",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<ApiResponse<AuthUserResponse>> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await adminClient.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while getting user by id: {UserId}", id);
            return new ApiResponse<AuthUserResponse>
            {
                Success = false,
                Error = new Error
                {
                    Code = "ADMIN_GET_BY_ID_FAILED",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<ApiResponse<AuthUserResponse>> CreateAsync(RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await adminClient.CreateAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while creating new user");
            return new ApiResponse<AuthUserResponse>
            {
                Success = false,
                Error = new Error
                {
                    Code = "ADMIN_CREATE_FAILED",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<ApiResponse> UpdateRoleAsync(Guid id, RemoteAuthUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await adminClient.UpdateRoleAsync(id, request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while updating role for user {UserId}", id);
            return new ApiResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "ADMIN_UPDATE_ROLE_FAILED",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<ApiResponse> UpdateStatusAsync(Guid id, UpdateStatusAuthUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await adminClient.UpdateStatusAsync(id, request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while updating status for user {UserId}", id);
            return new ApiResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "ADMIN_UPDATE_STATUS_FAILED",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await adminClient.DeleteAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while deleting user {UserId}", id);
            return new ApiResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "ADMIN_DELETE_FAILED",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<AuthUserResponse>>> FilterAsync(UserFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await adminClient.FilterAsync(filter, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while filtering users");
            return new ApiResponse<IEnumerable<AuthUserResponse>>
            {
                Success = false,
                Error = new Error
                {
                    Code = "ADMIN_FILTER_FAILED",
                    Message = ex.Message
                }
            };
        }
    }
}