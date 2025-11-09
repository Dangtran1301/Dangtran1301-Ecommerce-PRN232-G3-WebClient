using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients.Interfaces;

public interface IApiClient
{
    Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);
    Task<ApiResponse<T>> PostAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default);
    Task<ApiResponse<T>> PutAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(string endpoint, CancellationToken cancellationToken = default);
}