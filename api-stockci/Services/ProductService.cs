using api_stockci.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace api_stockci.Services;

public class ProductService : IProductService
{
    private readonly IMongoCollection<Product> _products;

    public ProductService(IOptions<StockManagementDatabaseSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _products = database.GetCollection<Product>(settings.ProductsCollectionName);
    }

    public async Task<List<Product>> GetAllAsync() =>
        await _products.Find(_ => true).ToListAsync();

    public async Task<Product?> GetByIdAsync(string id) =>
        await _products.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Product product) =>
        await _products.InsertOneAsync(product);

    public async Task UpdateAsync(string id, Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        await _products.ReplaceOneAsync(p => p.Id == id, product);
    }

    public async Task DeleteAsync(string id) =>
        await _products.DeleteOneAsync(p => p.Id == id);
}
