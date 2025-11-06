using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Services;

public class ProductAttributeService(IProductAttributeApiClient productAttributeApiClient, ILogger<ProductAttributeService> logger) : IProductAttributeService
{
    public async Task<ApiResponse<PagedResult<ProductAttributeDto>>> GetProductAttributesAsync(ProductAttributeFilterDto filter, CancellationToken cancellationToken = default)
    {
        return await productAttributeApiClient.FilterAsync(filter, cancellationToken);
    }

    public async Task<ApiResponse<ProductAttributeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await productAttributeApiClient.GetByIdAsync(id, cancellationToken);
    }

    public async Task<ApiResponse<ProductAttributeDto>> CreateAsync(CreateProductAttributeRequest request, CancellationToken cancellationToken = default)
    {
        return await productAttributeApiClient.CreateAsync(request, cancellationToken);
    }

    public async Task<ApiResponse<ProductAttributeDto>> UpdateAsync(Guid id, UpdateProductAttributeRequest request, CancellationToken cancellationToken = default)
    {
        return await productAttributeApiClient.UpdateAsync(id, request, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await productAttributeApiClient.DeleteAsync(id, cancellationToken);
    }
}

