using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients;

public class StockApiClient : IStockApiClient
{
    private const string BaseUrl = "odata/ODataStocks";
    private readonly IApiClient apiClient;
    private readonly ODataApiClient odataClient;

    public StockApiClient(IApiClient apiClient, ODataApiClient odataClient)
    {
        this.apiClient = apiClient;
        this.odataClient = odataClient;
    }

    public async Task<ApiResponse<PagedResult<StockDto>>> FilterAsync(StockFilterDto filter, CancellationToken cancellationToken = default)
    {
        var skip = ((filter.PageIndex ?? 1) - 1) * (filter.PageSize ?? 10);
        var top = filter.PageSize ?? 10;

        var queryParams = new List<string> { "$count=true", $"$top={top}", $"$skip={skip}" };

        // Only add $orderby if OrderBy is specified
        if (!string.IsNullOrWhiteSpace(filter.OrderBy))
        {
            var orderByClause = $"{filter.OrderBy} {(filter.Descending ? "desc" : "asc")}";
            queryParams.Add($"$orderby={Uri.EscapeDataString(orderByClause)}");
        }

        var filterParts = new List<string>();

        if (filter.ProductId.HasValue)
        {
            filterParts.Add($"ProductId eq {filter.ProductId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(filter.Location))
        {
            var location = filter.Location.Replace("'", "''");
            filterParts.Add($"contains(Location,'{location}')");
        }

        if (filterParts.Any())
        {
            var filterClause = string.Join(" and ", filterParts);
            queryParams.Add($"$filter={Uri.EscapeDataString(filterClause)}");
        }

        var query = "?" + string.Join("&", queryParams);

        var endpoint = $"{BaseUrl}{query}";
        var odataResponse = await odataClient.GetAsync<StockDto>(endpoint, cancellationToken);

        var items = odataResponse.Value ?? new List<StockDto>();
        var totalCount = odataResponse.Count ?? items.Count;

        var paged = new PagedResult<StockDto>(items, totalCount, filter.PageIndex ?? 1, filter.PageSize ?? 10);
        return new ApiResponse<PagedResult<StockDto>>
        {
            Success = true,
            Data = paged
        };
    }

    public Task<ApiResponse<StockDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => apiClient.GetAsync<StockDto>($"api/v1/catalog/stocks/{id}", cancellationToken);

    public Task<ApiResponse<StockDto>> CreateAsync(CreateStockRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<StockDto>("api/v1/catalog/stocks", request, cancellationToken);

    public Task<ApiResponse<StockDto>> UpdateAsync(Guid id, UpdateStockRequest request, CancellationToken cancellationToken = default)
        => apiClient.PutAsync<StockDto>($"api/v1/catalog/stocks/{id}", request, cancellationToken);

    public Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => apiClient.DeleteAsync($"api/v1/catalog/stocks/{id}", cancellationToken);
}