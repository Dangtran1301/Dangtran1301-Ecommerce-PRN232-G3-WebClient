using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Services
{
    public class BrandService(IBrandApiClient brandApiClient, ILogger<BrandService> logger) : IBrandService
    {
        public async Task<ApiResponse<PagedResult<BrandDto>>> GetBrandsAsync(BrandFilterDto filter, CancellationToken cancellationToken = default)
        {
            return await brandApiClient.FilterAsync(filter, cancellationToken);
        }

        public async Task<ApiResponse<BrandDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await brandApiClient.GetByIdAsync(id, cancellationToken);
        }

        public async Task<ApiResponse<BrandDto>> CreateAsync(CreateBrandRequest request, CancellationToken cancellationToken = default)
        {
            return await brandApiClient.CreateAsync(request, cancellationToken);
        }

        public async Task<ApiResponse<BrandDto>> UpdateAsync(Guid id, UpdateBrandRequest request, CancellationToken cancellationToken = default)
        {
            return await brandApiClient.UpdateAsync(id, request, cancellationToken);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await brandApiClient.DeleteAsync(id, cancellationToken);
        }
    }
}
