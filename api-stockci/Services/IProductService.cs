using api_stockci.Models;

namespace api_stockci.Services;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(string id);
    Task CreateAsync(Product product);
    Task UpdateAsync(string id, Product product);
    Task DeleteAsync(string id);
}
