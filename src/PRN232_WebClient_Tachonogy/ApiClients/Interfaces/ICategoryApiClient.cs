using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.ApiClients.Interfaces
{
    public interface ICategoryApiClient
    {
        Task<ApiResponse<PagedResult<CategoryDto>>> FilterAsync(CategoryFilterDto filter, CancellationToken cancellationToken = default);

        Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);

        Task<ApiResponse<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);

        Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}