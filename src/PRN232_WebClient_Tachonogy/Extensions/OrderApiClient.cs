using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;

namespace PRN232_WebClient_Tachonogy.Extensions
{
    public class OrderApiClient : IOrderApiClient
    {
        private const string BaseUrl = "https://localhost:7099/api/Orders";
        private readonly IApiClient apiClient;

        public OrderApiClient(IApiClient apiClient)
        {
            this.apiClient = apiClient;
        }

        public async Task<ApiResponse<PagedResult<OrderDto>>> FilterAsync(OrderFilterDto filter, CancellationToken cancellationToken = default)
        {
            var endpoint = $"{BaseUrl}/paged?pageIndex={filter.PageIndex}&pageSize={filter.PageSize}";
            var result = await apiClient.GetAsync<PagedResult<OrderDto>>(endpoint, cancellationToken);
            return result;
        }

        public Task<ApiResponse<OrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => apiClient.GetAsync<OrderDto>($"{BaseUrl}/{id}", cancellationToken);

        public Task<ApiResponse<OrderDto>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
            => apiClient.PostAsync<OrderDto>(BaseUrl, request, cancellationToken);

        public Task<ApiResponse<OrderDto>> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default)
            => apiClient.PutAsync<OrderDto>($"{BaseUrl}/{id}", request, cancellationToken);

        public Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => apiClient.DeleteAsync($"{BaseUrl}/{id}", cancellationToken);
    }
}
