using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Services
{
    public class CategoryService(ICategoryApiClient categoryApiClient, ILogger<CategoryService> logger) : ICategoryService
    {
        public async Task<ApiResponse<PagedResult<CategoryDto>>> GetCategoriesAsync(CategoryFilterDto filter, CancellationToken cancellationToken = default)
        {
            return await categoryApiClient.FilterAsync(filter, cancellationToken);
        }

        public async Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await categoryApiClient.GetByIdAsync(id, cancellationToken);
        }

        public async Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            return await categoryApiClient.CreateAsync(request, cancellationToken);
        }

        public async Task<ApiResponse<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            return await categoryApiClient.UpdateAsync(id, request, cancellationToken);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await categoryApiClient.DeleteAsync(id, cancellationToken);
        }
    }
}
