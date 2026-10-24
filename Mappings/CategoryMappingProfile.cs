using AutoMapper;
using ECommerceApp.DTOs.Requests.Category;
using ECommerceApp.DTOs.Responses.Category;
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
