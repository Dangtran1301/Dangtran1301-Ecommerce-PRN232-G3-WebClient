using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Services;

public class ProductVariantService(IProductVariantApiClient productVariantApiClient, ILogger<ProductVariantService> logger) : IProductVariantService
{
    public async Task<ApiResponse<PagedResult<ProductVariantDto>>> GetProductVariantsAsync(ProductVariantFilterDto filter, CancellationToken cancellationToken = default)
    {
        return await productVariantApiClient.FilterAsync(filter, cancellationToken);
    }

    public async Task<ApiResponse<ProductVariantDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await productVariantApiClient.GetByIdAsync(id, cancellationToken);
    }

    public async Task<ApiResponse<ProductVariantDto>> CreateAsync(CreateProductVariantRequest request, CancellationToken cancellationToken = default)
    {
        return await productVariantApiClient.CreateAsync(request, cancellationToken);
    }

    public async Task<ApiResponse<ProductVariantDto>> UpdateAsync(Guid id, UpdateProductVariantRequest request, CancellationToken cancellationToken = default)
    {
        return await productVariantApiClient.UpdateAsync(id, request, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await productVariantApiClient.DeleteAsync(id, cancellationToken);
    }
}

