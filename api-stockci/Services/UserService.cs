using api_stockci.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace api_stockci.Services;

public class UserService : IUserService
{
    private readonly IMongoCollection<User> _users;
    private readonly PasswordHasher<User> _hasher = new();

    public UserService(IOptions<StockManagementDatabaseSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _users = database.GetCollection<User>("Users");
    }

    public async Task<List<User>> GetAllAsync() =>
        await _users.Find(_ => true).ToListAsync();

    public async Task<User?> GetByIdAsync(string id) =>
        await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(User user, string plainPassword)
    {
        user.PasswordHash = _hasher.HashPassword(user, plainPassword);
        await _users.InsertOneAsync(user);
    }

    public async Task UpdateAsync(string id, User user) =>
        await _users.ReplaceOneAsync(u => u.Id == id, user);

    public async Task<bool> ChangePasswordAsync(string id, string oldPassword, string newPassword)
    {
        var user = await GetByIdAsync(id);
        if (user is null) return false;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, oldPassword);
        if (result == PasswordVerificationResult.Failed) return false;

        user.PasswordHash = _hasher.HashPassword(user, newPassword);
        await _users.ReplaceOneAsync(u => u.Id == id, user);
        return true;
    }

    public async Task DeleteAsync(string id) =>
        await _users.DeleteOneAsync(u => u.Id == id);
}
