using api_stockci.Models;

namespace api_stockci.Services;

public enum ProductResult
{
    Success,
    CategoryNotFound
}

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(string id);
    Task<ProductResult> CreateAsync(Product product);
    Task<ProductResult> UpdateAsync(string id, Product product);
    Task DeleteAsync(string id);
}
