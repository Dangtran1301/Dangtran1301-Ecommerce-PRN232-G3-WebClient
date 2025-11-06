using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.Services.Interfaces;

public interface IProductAttributeService
{
    Task<ApiResponse<PagedResult<ProductAttributeDto>>> GetProductAttributesAsync(ProductAttributeFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductAttributeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductAttributeDto>> CreateAsync(CreateProductAttributeRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductAttributeDto>> UpdateAsync(Guid id, UpdateProductAttributeRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

