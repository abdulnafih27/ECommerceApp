using System.ComponentModel.DataAnnotations;

public class UpdateProductRequest
{
    [MaxLength(ProductLimits.NameMaxLength)]
    public string? ProductName { get; set; }

    [MaxLength(ProductLimits.DescriptionMaxLength)]
    public string? ProductDescription { get; set; }
    
    [Range(typeof(decimal), ProductLimits.MinPriceText, ProductLimits.MaxPriceText, ParseLimitsInInvariantCulture = true)]
    public decimal? Price { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? CategoryId { get; set; }
    
    [Range(0, ProductLimits.MaxStock)]
    public int? StockQuantity { get; set; }
    
    [MaxLength(ProductLimits.ImageUrlMaxLength)]
    [Url]
    public string? ImageUrl { get; set; }
}