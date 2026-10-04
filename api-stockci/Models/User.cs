using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_stockci.Models;

public enum UserRole
{
    Admin,
    Manager,
    Employee
}

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    [BsonRepresentation(BsonType.String)]
    public UserRole Role { get; set; } = UserRole.Employee;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
