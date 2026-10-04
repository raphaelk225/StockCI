using api_stockci.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace api_stockci.Services;

public class CategoryService : ICategoryService
{
    private readonly IMongoCollection<Category> _categories;

    public CategoryService(IOptions<StockManagementDatabaseSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _categories = database.GetCollection<Category>("Categories");
    }

    public async Task<List<Category>> GetAllAsync() =>
        await _categories.Find(_ => true).ToListAsync();

    public async Task<Category?> GetByIdAsync(string id) =>
        await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Category category) =>
        await _categories.InsertOneAsync(category);

    public async Task UpdateAsync(string id, Category category) =>
        await _categories.ReplaceOneAsync(c => c.Id == id, category);

    public async Task DeleteAsync(string id) =>
        await _categories.DeleteOneAsync(c => c.Id == id);
}
