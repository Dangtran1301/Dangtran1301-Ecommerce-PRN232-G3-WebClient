using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients;

public class ProductApiClient : IProductApiClient
{
    private const string BaseUrl = "odata/products";
    private readonly IApiClient apiClient;
    private readonly ODataApiClient odataClient;

    public ProductApiClient(IApiClient apiClient, ODataApiClient odataClient)
    {
        this.apiClient = apiClient;
        this.odataClient = odataClient;
    }

    public async Task<ApiResponse<PagedResult<ProductDto>>> FilterAsync(ProductFilterDto filter, CancellationToken cancellationToken = default)
    {
        var skip = ((filter.PageIndex ?? 1) - 1) * (filter.PageSize ?? 10);
        var top = filter.PageSize ?? 10;

        var queryParams = new List<string> { "$count=true", $"$top={top}", $"$skip={skip}" };

        // Only add $orderby if OrderBy is specified and not empty
        // Use Id as default orderby for OData compatibility (always available in DTO)
        var orderByField = !string.IsNullOrWhiteSpace(filter.OrderBy) ? filter.OrderBy.Trim() : "Id";

        // Validate orderBy field - only allow fields that exist in ProductDto
        // ProductDto has: Id, ProductName, Description, Price, Sku, ImageUrl, Specifications, BrandId, BrandName, CategoryId, CategoryName
        var validOrderByFields = new[] { "Id", "ProductName", "Price", "BrandId", "CategoryId" };
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
            filterParts.Add($"(contains(ProductName,'{keyword}') or contains(Description,'{keyword}'))");
        }

        if (filter.BrandId.HasValue)
        {
            filterParts.Add($"BrandId eq {filter.BrandId.Value}");
        }

        if (filter.CategoryId.HasValue)
        {
            filterParts.Add($"CategoryId eq {filter.CategoryId.Value}");
        }

        if (filterParts.Any())
        {
            var filterClause = string.Join(" and ", filterParts);
            queryParams.Add($"$filter={Uri.EscapeDataString(filterClause)}");
        }

        var query = "?" + string.Join("&", queryParams);

        var endpoint = $"{BaseUrl}{query}";
        var odataResponse = await odataClient.GetAsync<ProductDto>(endpoint, cancellationToken);

        var items = odataResponse.Value ?? new List<ProductDto>();
        var totalCount = odataResponse.Count ?? items.Count;

        var paged = new PagedResult<ProductDto>(items, totalCount, filter.PageIndex ?? 1, filter.PageSize ?? 10);
        return new ApiResponse<PagedResult<ProductDto>>
        {
            Success = true,
            Data = paged
        };
    }

    public Task<ApiResponse<IReadOnlyList<ProductDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        => apiClient.GetAsync<IReadOnlyList<ProductDto>>("products", cancellationToken);

    public Task<ApiResponse<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => apiClient.GetAsync<ProductDto>($"products/{id}", cancellationToken);

    public Task<ApiResponse<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<ProductDto>("products", request, cancellationToken);

    public Task<ApiResponse<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
        => apiClient.PutAsync<ProductDto>($"products/{id}", request, cancellationToken);

    public Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => apiClient.DeleteAsync($"products/{id}", cancellationToken);
}