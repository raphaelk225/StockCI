using api_stockci.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace api_stockci.Services;

public class ProductService : IProductService
{
    private readonly IMongoCollection<Product> _products;
    private readonly ICategoryService _categoryService;

    public ProductService(IOptions<StockManagementDatabaseSettings> options, ICategoryService categoryService)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _products = database.GetCollection<Product>(settings.ProductsCollectionName);
        _categoryService = categoryService;
    }

    public async Task<List<Product>> GetAllAsync() =>
        await _products.Find(_ => true).ToListAsync();

    public async Task<Product?> GetByIdAsync(string id) =>
        await _products.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task<ProductResult> CreateAsync(Product product)
    {
        var category = await _categoryService.GetByIdAsync(product.CategoryId);
        if (category is null) return ProductResult.CategoryNotFound;

        await _products.InsertOneAsync(product);
        return ProductResult.Success;
    }

    public async Task<ProductResult> UpdateAsync(string id, Product product)
    {
        var category = await _categoryService.GetByIdAsync(product.CategoryId);
        if (category is null) return ProductResult.CategoryNotFound;

        product.UpdatedAt = DateTime.UtcNow;
        await _products.ReplaceOneAsync(p => p.Id == id, product);
        return ProductResult.Success;
    }

    public async Task DeleteAsync(string id) =>
        await _products.DeleteOneAsync(p => p.Id == id);
}
