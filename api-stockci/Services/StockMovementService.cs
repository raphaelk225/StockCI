using api_stockci.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace api_stockci.Services;

public class StockMovementService : IStockMovementService
{
    private readonly IMongoCollection<StockMovement> _movements;
    private readonly IProductService _productService;
    private readonly IUserService _userService;

    public StockMovementService(IOptions<StockManagementDatabaseSettings> options,
        IProductService productService, IUserService userService)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _movements = database.GetCollection<StockMovement>("StockMovements");
        _productService = productService;
        _userService = userService;
    }

    public async Task<List<StockMovement>> GetAllAsync() =>
        await _movements.Find(_ => true).ToListAsync();

    public async Task<StockMovement?> GetByIdAsync(string id) =>
        await _movements.Find(m => m.Id == id).FirstOrDefaultAsync();

    public async Task<StockMovementResult> CreateAsync(StockMovement movement)
    {
        var product = await _productService.GetByIdAsync(movement.ProductId);
        if (product is null) return StockMovementResult.ProductNotFound;

        var user = await _userService.GetByIdAsync(movement.UserId);
        if (user is null) return StockMovementResult.UserNotFound;

        if (movement.Type == StockMovementType.Exit && product.Quantity < movement.Quantity)
            return StockMovementResult.InsufficientStock;

        await _movements.InsertOneAsync(movement);

        var delta = movement.Type == StockMovementType.Entry ? movement.Quantity : -movement.Quantity;
        product.Quantity += delta;
        await _productService.UpdateAsync(product.Id, product);

        return StockMovementResult.Success;
    }
}
