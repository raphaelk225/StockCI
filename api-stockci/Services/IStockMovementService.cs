using api_stockci.Models;

namespace api_stockci.Services;

public enum StockMovementResult
{
    Success,
    ProductNotFound,
    UserNotFound,
    InsufficientStock
}

public interface IStockMovementService
{
    Task<List<StockMovement>> GetAllAsync();
    Task<StockMovement?> GetByIdAsync(string id);
    Task<StockMovementResult> CreateAsync(StockMovement movement);
}
