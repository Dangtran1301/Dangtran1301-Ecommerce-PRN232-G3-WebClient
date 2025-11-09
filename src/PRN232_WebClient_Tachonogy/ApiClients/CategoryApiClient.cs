using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients
{
    public class CategoryApiClient : ICategoryApiClient
    {
        private const string BaseUrl = "odata/categories";
        private readonly IApiClient apiClient;
        private readonly ODataApiClient odataClient;

        public CategoryApiClient(IApiClient apiClient, ODataApiClient odataClient)
        {
            this.apiClient = apiClient;
            this.odataClient = odataClient;
        }

        public async Task<ApiResponse<PagedResult<CategoryDto>>> FilterAsync(CategoryFilterDto filter, CancellationToken cancellationToken = default)
        {
            var skip = ((filter.PageIndex ?? 1) - 1) * (filter.PageSize ?? 10);
            var top = filter.PageSize ?? 10;

            var query = $"?$count=true&$top={top}&$skip={skip}&$orderby={filter.OrderBy} {(filter.Descending ? "desc" : "asc")}";
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Replace("'", "''");
                query += $"&$filter=contains(CategoryName,'{Uri.EscapeDataString(keyword)}')";
            }

            var endpoint = $"{BaseUrl}{query}";
            var odataResponse = await odataClient.GetAsync<CategoryDto>(endpoint, cancellationToken);

            var items = odataResponse.Value ?? new List<CategoryDto>();
            var totalCount = odataResponse.Count ?? items.Count;

            var paged = new PagedResult<CategoryDto>(items, totalCount, filter.PageIndex ?? 1, filter.PageSize ?? 10);
            return new ApiResponse<PagedResult<CategoryDto>>
            {
                Success = true,
                Data = paged
            };
        }

        public Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => apiClient.GetAsync<CategoryDto>($"categories/{id}", cancellationToken);

        public Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
            => apiClient.PostAsync<CategoryDto>("categories", request, cancellationToken);

        public Task<ApiResponse<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
            => apiClient.PutAsync<CategoryDto>($"categories/{id}", request, cancellationToken);

        public Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => apiClient.DeleteAsync($"categories/{id}", cancellationToken);
    }
}
