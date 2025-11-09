using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients
{
    public class BrandApiClient : IBrandApiClient
    {
        private const string BaseUrl = "odata/brands";
        private readonly IApiClient apiClient;
        private readonly ODataApiClient odataClient;

        public BrandApiClient(IApiClient apiClient, ODataApiClient odataClient)
        {
            this.apiClient = apiClient;
            this.odataClient = odataClient;
        }

        public async Task<ApiResponse<PagedResult<BrandDto>>> FilterAsync(BrandFilterDto filter, CancellationToken cancellationToken = default)
        {
            var skip = ((filter.PageIndex ?? 1) - 1) * (filter.PageSize ?? 10);
            var top = filter.PageSize ?? 10;

            var query = $"?$count=true&$top={top}&$skip={skip}&$orderby={filter.OrderBy} {(filter.Descending ? "desc" : "asc")}";
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Replace("'", "''");
                query += $"&$filter=contains(BrandName,'{Uri.EscapeDataString(keyword)}')";
            }

            var endpoint = $"{BaseUrl}{query}";
            var odataResponse = await odataClient.GetAsync<BrandDto>(endpoint, cancellationToken);

            var items = odataResponse.Value ?? new List<BrandDto>();
            var totalCount = odataResponse.Count ?? items.Count;

            var paged = new PagedResult<BrandDto>(items, totalCount, filter.PageIndex ?? 1, filter.PageSize ?? 10);
            return new ApiResponse<PagedResult<BrandDto>>
            {
                Success = true,
                Data = paged
            };
        }

        public Task<ApiResponse<BrandDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => apiClient.GetAsync<BrandDto>($"brands/{id}", cancellationToken);

        public Task<ApiResponse<BrandDto>> CreateAsync(CreateBrandRequest request, CancellationToken cancellationToken = default)
            => apiClient.PostAsync<BrandDto>("brands", request, cancellationToken);

        public Task<ApiResponse<BrandDto>> UpdateAsync(Guid id, UpdateBrandRequest request, CancellationToken cancellationToken = default)
            => apiClient.PutAsync<BrandDto>($"brands/{id}", request, cancellationToken);

        public Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => apiClient.DeleteAsync($"brands/{id}", cancellationToken);
    }
}