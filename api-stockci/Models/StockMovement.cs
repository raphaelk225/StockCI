namespace api_stockci.Models;

public enum StockMovementType
{
    Entry,  // Entrée de stock
    Exit    // Sortie de stock
}

public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
