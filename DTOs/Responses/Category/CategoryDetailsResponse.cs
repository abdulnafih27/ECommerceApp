using ECommerceApp.Models;

namespace ECommerceApp.DTOs.Responses.Category
{
    public class CategoryDetailsResponse
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryDescription { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
