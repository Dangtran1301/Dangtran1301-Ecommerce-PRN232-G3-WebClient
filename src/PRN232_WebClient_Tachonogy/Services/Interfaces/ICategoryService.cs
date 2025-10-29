using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<ApiResponse<PagedResult<CategoryDto>>> GetCategoriesAsync(CategoryFilterDto filter, CancellationToken cancellationToken = default);
        Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
