using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients.Interfaces;

public interface IProductVariantApiClient
{
    Task<ApiResponse<PagedResult<ProductVariantDto>>> FilterAsync(ProductVariantFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductVariantDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductVariantDto>> CreateAsync(CreateProductVariantRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductVariantDto>> UpdateAsync(Guid id, UpdateProductVariantRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

