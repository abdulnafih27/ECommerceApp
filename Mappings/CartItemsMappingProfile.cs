using AutoMapper;
using ECommerceApp.DTOs.Requests.Category;
using ECommerceApp.DTOs.Responses.Category;
using ECommerceApp.Models;

namespace ECommerceApp.Mappings
{
    public class CartItemsMappingProfile : Profile
    {
        public CartItemsMappingProfile()
        {
            CreateMap<AddCartItemRequest, CartItem>();
            CreateMap<CartItem, CartItemDetailsResponse>()
                .ForMember(dest => dest.IsAvailable, obj => obj.MapFrom(src => !src.Product!.IsDeleted));
        }
    }
}
