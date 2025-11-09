using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.Extensions.Interfaces
{
    public interface IOrderApiClient
    {
        Task<ApiResponse<PagedResult<OrderDto>>> FilterAsync(OrderFilterDto filter, CancellationToken cancellationToken = default);

        Task<ApiResponse<OrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<ApiResponse<OrderDto>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

        Task<ApiResponse<OrderDto>> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default);

        Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}