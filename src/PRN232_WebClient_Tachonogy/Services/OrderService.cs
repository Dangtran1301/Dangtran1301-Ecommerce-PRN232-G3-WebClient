using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Services
{
    public class OrderService(IOrderApiClient orderApiClient, ILogger<OrderService> logger) : IOrderService
    {
        public async Task<ApiResponse<PagedResult<OrderDto>>> GetOrdersAsync(OrderFilterDto filter, CancellationToken cancellationToken = default)
            => await orderApiClient.FilterAsync(filter, cancellationToken);

        public async Task<ApiResponse<OrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await orderApiClient.GetByIdAsync(id, cancellationToken);

        public async Task<ApiResponse<OrderDto>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
            => await orderApiClient.CreateAsync(request, cancellationToken);

        public async Task<ApiResponse<OrderDto>> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default)
            => await orderApiClient.UpdateAsync(id, request, cancellationToken);

        public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => await orderApiClient.DeleteAsync(id, cancellationToken);
    }
}
