using PRN232_WebClient_Tachonogy.DTOs;

namespace PRN232_WebClient_Tachonogy.Services.Interfaces;

public interface IStockService
{
    Task<ApiResponse<PagedResult<StockDto>>> GetStocksAsync(StockFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<StockDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<StockDto>> CreateAsync(CreateStockRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<StockDto>> UpdateAsync(Guid id, UpdateStockRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

