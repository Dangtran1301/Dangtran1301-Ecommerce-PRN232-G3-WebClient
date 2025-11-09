using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ApiResponse<PagedResult<OrderDto>>> GetOrdersAsync(OrderFilterDto filter, CancellationToken cancellationToken = default);

        Task<ApiResponse<OrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<ApiResponse<OrderDto>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

        Task<ApiResponse<OrderDto>> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default);

        Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}