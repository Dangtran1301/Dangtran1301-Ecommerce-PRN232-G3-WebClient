using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients;

public class ProductAttributeApiClient : IProductAttributeApiClient
{
    private const string BaseUrl = "odata/ODataProductAttributes";
    private readonly IApiClient apiClient;
    private readonly ODataApiClient odataClient;

    public ProductAttributeApiClient(IApiClient apiClient, ODataApiClient odataClient)
    {
        this.apiClient = apiClient;
        this.odataClient = odataClient;
    }

    public async Task<ApiResponse<PagedResult<ProductAttributeDto>>> FilterAsync(ProductAttributeFilterDto filter, CancellationToken cancellationToken = default)
    {
        var skip = ((filter.PageIndex ?? 1) - 1) * (filter.PageSize ?? 10);
        var top = filter.PageSize ?? 10;

        var queryParams = new List<string> { "$count=true", $"$top={top}", $"$skip={skip}" };
        
        // Validate and set OrderBy field
        var orderByField = !string.IsNullOrWhiteSpace(filter.OrderBy) ? filter.OrderBy.Trim() : "Id";
        // ProductAttributeDto has: Id, ProductId, AttributeName, AttributeValue
        var validOrderByFields = new[] { "Id", "AttributeName", "AttributeValue", "ProductId" };
        if (!validOrderByFields.Contains(orderByField, StringComparer.OrdinalIgnoreCase))
        {
            orderByField = "Id"; // Fallback to Id if invalid field
        }
        
        var orderByClause = $"{orderByField} {(filter.Descending ? "desc" : "asc")}";
        queryParams.Add($"$orderby={Uri.EscapeDataString(orderByClause)}");
        
        var filterParts = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Replace("'", "''");
            filterParts.Add($"(contains(AttributeName,'{keyword}') or contains(AttributeValue,'{keyword}'))");
        }
        
        if (filter.ProductId.HasValue)
        {
            filterParts.Add($"ProductId eq {filter.ProductId.Value}");
        }

        if (filterParts.Any())
        {
            var filterClause = string.Join(" and ", filterParts);
            queryParams.Add($"$filter={Uri.EscapeDataString(filterClause)}");
        }

        var query = "?" + string.Join("&", queryParams);

        var endpoint = $"{BaseUrl}{query}";
        var odataResponse = await odataClient.GetAsync<ProductAttributeDto>(endpoint, cancellationToken);

        var items = odataResponse.Value ?? new List<ProductAttributeDto>();
        var totalCount = odataResponse.Count ?? items.Count;

        var paged = new PagedResult<ProductAttributeDto>(items, totalCount, filter.PageIndex ?? 1, filter.PageSize ?? 10);
        return new ApiResponse<PagedResult<ProductAttributeDto>>
        {
            Success = true,
            Data = paged
        };
    }

    public Task<ApiResponse<ProductAttributeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => apiClient.GetAsync<ProductAttributeDto>($"api/v1/catalog/product-attributes/{id}", cancellationToken);

    public Task<ApiResponse<ProductAttributeDto>> CreateAsync(CreateProductAttributeRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<ProductAttributeDto>("api/v1/catalog/product-attributes", request, cancellationToken);

    public Task<ApiResponse<ProductAttributeDto>> UpdateAsync(Guid id, UpdateProductAttributeRequest request, CancellationToken cancellationToken = default)
        => apiClient.PutAsync<ProductAttributeDto>($"api/v1/catalog/product-attributes/{id}", request, cancellationToken);

    public Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => apiClient.DeleteAsync($"api/v1/catalog/product-attributes/{id}", cancellationToken);
}

