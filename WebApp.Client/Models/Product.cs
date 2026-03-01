namespace WebApp.Client.Models;

public class Product {
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastConsulted { get; set; }
    public int StockLevel { get; set; }
}