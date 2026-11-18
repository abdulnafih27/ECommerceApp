using ECommerceApp.Common;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs.Requests.Categories
{
    public class CreateCategoryRequest
    {
        [Required]
        [MaxLength(CategoryLimits.CategoryNameMaxLength)]
        public string? CategoryName { get; set => field = value.TrimToNull(); }
        [MaxLength(CategoryLimits.CategoryDescriptionMaxLength)]
        public string? CategoryDescription { get; set => field = value.TrimToNull(); }
    }
}
