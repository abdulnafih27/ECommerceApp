using AutoMapper;
using ECommerceApp.DTOs.Requests.Orders;
using ECommerceApp.DTOs.Responses.Orders;
using ECommerceApp.Models;

namespace ECommerceApp.Mappings
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile() 
        {
            CreateMap<PlaceOrderRequest, Order>();
            CreateMap<Order, OrderDetailsResponse>();

            // Order Items
            CreateMap<OrderItem, OrderItemResponse>();
            CreateMap<CartItem, OrderItem>()
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.ProductName, obj => obj.MapFrom(src => src.Product!.ProductName))
                .ForMember(dest => dest.UnitPrice, obj => obj.MapFrom(src => src.Product!.Price));
        }
    }
}
