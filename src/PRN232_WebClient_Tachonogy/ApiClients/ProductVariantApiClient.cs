using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients;

public class ProductVariantApiClient : IProductVariantApiClient
{
    private const string BaseUrl = "odata/ODataProductVariants";
    private readonly IApiClient apiClient;
    private readonly ODataApiClient odataClient;

    public ProductVariantApiClient(IApiClient apiClient, ODataApiClient odataClient)
    {
        this.apiClient = apiClient;
        this.odataClient = odataClient;
    }

    public async Task<ApiResponse<PagedResult<ProductVariantDto>>> FilterAsync(ProductVariantFilterDto filter, CancellationToken cancellationToken = default)
    {
        var skip = ((filter.PageIndex ?? 1) - 1) * (filter.PageSize ?? 10);
        var top = filter.PageSize ?? 10;

        var queryParams = new List<string> { "$count=true", $"$top={top}", $"$skip={skip}" };
        
        // Validate and set OrderBy field
        var orderByField = !string.IsNullOrWhiteSpace(filter.OrderBy) ? filter.OrderBy.Trim() : "Id";
        // ProductVariantDto has: Id, ProductId, VariantName, Price, Sku, ImageUrl
        var validOrderByFields = new[] { "Id", "VariantName", "Price", "ProductId" };
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
            filterParts.Add($"(contains(VariantName,'{keyword}') or contains(Sku,'{keyword}'))");
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
        var odataResponse = await odataClient.GetAsync<ProductVariantDto>(endpoint, cancellationToken);

        var items = odataResponse.Value ?? new List<ProductVariantDto>();
        var totalCount = odataResponse.Count ?? items.Count;

        var paged = new PagedResult<ProductVariantDto>(items, totalCount, filter.PageIndex ?? 1, filter.PageSize ?? 10);
        return new ApiResponse<PagedResult<ProductVariantDto>>
        {
            Success = true,
            Data = paged
        };
    }

    public Task<ApiResponse<ProductVariantDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => apiClient.GetAsync<ProductVariantDto>($"api/v1/catalog/product-variants/{id}", cancellationToken);

    public Task<ApiResponse<ProductVariantDto>> CreateAsync(CreateProductVariantRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<ProductVariantDto>("api/v1/catalog/product-variants", request, cancellationToken);

    public Task<ApiResponse<ProductVariantDto>> UpdateAsync(Guid id, UpdateProductVariantRequest request, CancellationToken cancellationToken = default)
        => apiClient.PutAsync<ProductVariantDto>($"api/v1/catalog/product-variants/{id}", request, cancellationToken);

    public Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => apiClient.DeleteAsync($"api/v1/catalog/product-variants/{id}", cancellationToken);
}

