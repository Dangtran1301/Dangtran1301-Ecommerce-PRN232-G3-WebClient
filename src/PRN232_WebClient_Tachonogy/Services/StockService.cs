using PRN232_WebClient_Tachonogy.ApiClients.Interfaces;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Services;

public class StockService(IStockApiClient stockApiClient, ILogger<StockService> logger) : IStockService
{
    public async Task<ApiResponse<PagedResult<StockDto>>> GetStocksAsync(StockFilterDto filter, CancellationToken cancellationToken = default)
    {
        return await stockApiClient.FilterAsync(filter, cancellationToken);
    }

    public async Task<ApiResponse<StockDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await stockApiClient.GetByIdAsync(id, cancellationToken);
    }

    public async Task<ApiResponse<StockDto>> CreateAsync(CreateStockRequest request, CancellationToken cancellationToken = default)
    {
        return await stockApiClient.CreateAsync(request, cancellationToken);
    }

    public async Task<ApiResponse<StockDto>> UpdateAsync(Guid id, UpdateStockRequest request, CancellationToken cancellationToken = default)
    {
        return await stockApiClient.UpdateAsync(id, request, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await stockApiClient.DeleteAsync(id, cancellationToken);
    }
}

