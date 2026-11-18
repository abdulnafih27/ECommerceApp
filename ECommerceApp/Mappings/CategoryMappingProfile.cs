using AutoMapper;
using ECommerceApp.DTOs.Requests.Categories;
using ECommerceApp.DTOs.Responses.Categories;
using ECommerceApp.Models;

namespace ECommerceApp.Mappings
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<Category, CategoryDetailsResponse>();
        }
    }
}
