using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;
using PRN232_WebClient_Tachonogy.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PRN232_WebClient_Tachonogy.Extensions;

public class ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ITokenProvider tokenProvider)
    : IApiClient
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    private void AttachToken()
    {
        var token = tokenProvider.AccessToken;
        httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
        => await SendAsync<T>(() => httpClient.GetAsync(endpoint, cancellationToken));

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default)
        => await SendAsync<T>(() =>
            httpClient.PostAsync(endpoint, new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"), cancellationToken));

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default)
        => await SendAsync<T>(() =>
            httpClient.PutAsync(endpoint, new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"), cancellationToken));

    public async Task<ApiResponse<bool>> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
        => await SendAsync<bool>(() => httpClient.DeleteAsync(endpoint, cancellationToken));

    private async Task<ApiResponse<T>> SendAsync<T>(Func<Task<HttpResponseMessage>> sendFunc)
    {
        AttachToken();

        var response = await sendFunc();
        return await DeserializeResponse<T>(response);
    }

    private async Task<ApiResponse<T>> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions)
                   ?? new ApiResponse<T> { Success = false, Error = new Error { Message = "Empty response." } };

        try
        {
            var error = JsonSerializer.Deserialize<Error>(content, _jsonOptions);
            return new ApiResponse<T> { Success = false, Error = error };
        }
        catch
        {
            return new ApiResponse<T> { Success = false, Error = new Error { Message = response.ReasonPhrase } };
        }
    }
}