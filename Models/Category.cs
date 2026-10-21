namespace ECommerceApp.Models
{
    public class Category : IAuditable
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryDescription { get; set; }
        public DateTime CreatedOn {get; set;}
        public DateTime ModifiedOn {get; set;}
        public ICollection<Product> Products {get; set;} = new List<Product>();
    }
}
