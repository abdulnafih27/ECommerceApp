using AutoMapper;
using ECommerceApp.Models;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDetailsResponse>();
        CreateMap<CreateProductRequest, Product>();
    }
}