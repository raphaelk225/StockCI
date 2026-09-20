namespace api_stockci.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }      // stock actuel
    public int MinQuantity { get; set; }   // seuil d'alerte
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public List<StockMovement> StockMovements { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
