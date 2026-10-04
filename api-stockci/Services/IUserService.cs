using api_stockci.Models;

namespace api_stockci.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id);
    Task CreateAsync(User user, string plainPassword);
    Task UpdateAsync(string id, User user);
    Task<bool> ChangePasswordAsync(string id, string oldPassword, string newPassword);
    Task DeleteAsync(string id);
}
