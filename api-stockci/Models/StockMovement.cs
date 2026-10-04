using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_stockci.Models;

public enum StockMovementType{Entry, Exit}

public class StockMovement
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string ProductId { get; set; } = string.Empty;
    [BsonRepresentation(BsonType.String)]
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
