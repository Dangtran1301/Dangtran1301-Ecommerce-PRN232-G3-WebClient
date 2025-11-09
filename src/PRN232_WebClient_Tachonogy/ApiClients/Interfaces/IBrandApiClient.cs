using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients.Interfaces
{
    public interface IBrandApiClient
    {
        Task<ApiResponse<PagedResult<BrandDto>>> FilterAsync(BrandFilterDto filter, CancellationToken cancellationToken = default);
        Task<ApiResponse<BrandDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<BrandDto>> CreateAsync(CreateBrandRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<BrandDto>> UpdateAsync(Guid id, UpdateBrandRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
