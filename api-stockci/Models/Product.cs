using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_stockci.Models;

public class Product
{

    [BsonId] // This attribute indicates that the property is the primary key for the document in MongoDB.
    [BsonRepresentation(BsonType.ObjectId)] // This attribute specifies that the property should be represented as an ObjectId in MongoDB.
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString(); // This initializes the Id property with a new ObjectId converted to a string.
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }      // stock actuel
    public int MinQuantity { get; set; }   // seuil d'alerte
    public string CategoryId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
