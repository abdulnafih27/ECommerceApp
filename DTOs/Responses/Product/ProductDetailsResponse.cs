using ECommerceApp.Models;

public class ProductDetailsResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductDescription { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
}