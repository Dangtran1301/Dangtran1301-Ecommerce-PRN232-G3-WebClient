using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients.Interfaces;

public interface IProductAttributeApiClient
{
    Task<ApiResponse<PagedResult<ProductAttributeDto>>> FilterAsync(ProductAttributeFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductAttributeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductAttributeDto>> CreateAsync(CreateProductAttributeRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductAttributeDto>> UpdateAsync(Guid id, UpdateProductAttributeRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}


