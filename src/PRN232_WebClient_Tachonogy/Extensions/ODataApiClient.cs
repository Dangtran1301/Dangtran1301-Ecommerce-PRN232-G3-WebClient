using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PRN232_WebClient_Tachonogy.DTOs;
namespace PRN232_WebClient_Tachonogy.Extensions
{
    public class ODataApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ODataApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ODataResponse<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<ODataResponse<T>>(json, _jsonOptions);

            return result ?? new ODataResponse<T> { Value = new List<T>() };
        }
    }
}
