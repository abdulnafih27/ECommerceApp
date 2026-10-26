using AutoMapper;
using ECommerceApp.DTOs.Requests.Product;
using ECommerceApp.DTOs.Responses.Product;
using ECommerceApp.Models;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDetailsResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category!.CategoryName));
        CreateMap<CreateProductRequest, Product>();
    }
}