using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;
using PRN232_WebClient_Tachonogy.Models;
using PRN232_WebClient_Tachonogy.Services.Interfaces;
using System.Net;
using System.Net.Http.Headers;

namespace PRN232_WebClient_Tachonogy.Services;

public class AuthService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<AuthService> logger)
    : IAuthService
{
    private const string BaseEndpoint = "auth";

    private void AttachAuthorizationHeader()
    {
        var token = tokenProvider.AccessToken;
        httpClient.DefaultRequestHeaders.Authorization =
            !string.IsNullOrEmpty(token)
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
    }

    private static async Task<ApiResponse<T>> ParseResponseAsync<T>(HttpResponseMessage response)
    {
        try
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "UNAUTHORIZED",
                        Message = "Unauthorized. Please log in again."
                    }
                };
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
            if (result is not null) return result;

            return new ApiResponse<T>
            {
                Success = false,
                Error = new Error
                {
                    Code = "DESERIALIZATION_ERROR",
                    Message = "Invalid response format from server"
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Error = new Error
                {
                    Code = "PARSE_ERROR",
                    Message = $"Failed to parse server response: {ex.Message}"
                }
            };
        }
    }

    private static async Task<ApiResponse> ParseResponseAsync(HttpResponseMessage response)
    {
        try
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new ApiResponse
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "UNAUTHORIZED",
                        Message = "Unauthorized. Please log in again."
                    }
                };
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (result is not null) return result;

            return new ApiResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "DESERIALIZATION_ERROR",
                    Message = "Invalid response format from server"
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "PARSE_ERROR",
                    Message = $"Failed to parse server response: {ex.Message}"
                }
            };
        }
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/login", dto, cancellationToken);
            return await ParseResponseAsync<LoginResponse>(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login");
            return new ApiResponse<LoginResponse>
            {
                Success = false,
                Error = new Error { Code = "EXCEPTION", Message = ex.Message }
            };
        }
    }

    public async Task<ApiResponse> RegisterAsync(RegisterRequest dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/register", dto, cancellationToken);
            return await ParseResponseAsync(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during register");
            return new ApiResponse
            {
                Success = false,
                Error = new Error { Code = "EXCEPTION", Message = ex.Message }
            };
        }
    }

    public async Task<ApiResponse<LoginResponse>> RefreshAsync(RefreshTokenRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/refresh", dto, cancellationToken);
            var result = await ParseResponseAsync<LoginResponse>(response);

            if (result is { Success: true, Data: not null })
            {
                tokenProvider.SetTokens(result.Data.AccessToken, result.Data.RefreshToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token refresh");
            return new ApiResponse<LoginResponse>
            {
                Success = false,
                Error = new Error { Code = "EXCEPTION", Message = ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> LogoutAsync(RefreshTokenRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            AttachAuthorizationHeader();

            var response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/logout", dto, cancellationToken);
            var result = await ParseResponseAsync<bool>(response);

            if (result.Success)
                tokenProvider.ClearTokens();

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return new ApiResponse<bool>
            {
                Success = false,
                Error = new Error { Code = "EXCEPTION", Message = ex.Message }
            };
        }
    }

    public async Task<ApiResponse> ChangePasswordAsync(Guid id, ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            AttachAuthorizationHeader();

            var response = await httpClient.PutAsJsonAsync($"{BaseEndpoint}/{id}/password", request, cancellationToken);
            return await ParseResponseAsync(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error changing password for user {UserId}", id);
            return new ApiResponse
            {
                Success = false,
                Error = new Error { Code = "EXCEPTION", Message = ex.Message }
            };
        }
    }

    public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/forgot-password", dto, cancellationToken);
            return await ParseResponseAsync(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending forgot password request for {Email}", dto.Email);
            return new ApiResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "EXCEPTION",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest dto,
        CancellationToken cancellationToken = default)
    {
        try
        {

            var response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/reset-password", dto, cancellationToken);
            return await ParseResponseAsync(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resetting password with token {Token}", dto.Token);
            return new ApiResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "EXCEPTION",
                    Message = ex.Message
                }
            };
        }
    }
}