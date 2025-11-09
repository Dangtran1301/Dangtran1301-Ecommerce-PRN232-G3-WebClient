using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Services;

public class ProductService(IProductApiClient productApiClient, ILogger<ProductService> logger) : IProductService
{
    public async Task<ApiResponse<PagedResult<ProductDto>>> GetProductsAsync(ProductFilterDto filter, CancellationToken cancellationToken = default)
    {
        return await productApiClient.FilterAsync(filter, cancellationToken);
    }

    public async Task<ApiResponse<IReadOnlyList<ProductDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await productApiClient.GetAllAsync(cancellationToken);
    }

    public async Task<ApiResponse<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await productApiClient.GetByIdAsync(id, cancellationToken);
    }

    public async Task<ApiResponse<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        return await productApiClient.CreateAsync(request, cancellationToken);
    }

    public async Task<ApiResponse<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        return await productApiClient.UpdateAsync(id, request, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await productApiClient.DeleteAsync(id, cancellationToken);
    }
}


