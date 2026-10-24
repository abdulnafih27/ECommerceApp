using ECommerceApp.Common;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs.Requests.Category
{
    public class UpdateCategoryRequest
    {
        [MaxLength(CategoryLimits.CategoryNameMaxLength)]
        public string? CategoryName { get; set => field = value.TrimToNull(); }
        [MaxLength(CategoryLimits.CategoryDescriptionMaxLength)]
        public string? CategoryDescription { get; set => field = value.TrimToNull(); }
    }
}
