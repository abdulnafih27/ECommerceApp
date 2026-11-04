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
            CreateMap<Order, OrderSummaryResponse>();

            // Order Items
            CreateMap<OrderItem, OrderItemResponse>();
        }
    }
}
