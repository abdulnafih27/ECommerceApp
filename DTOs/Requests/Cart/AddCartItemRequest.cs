using ECommerceApp.Common;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs.Requests.Cart
{
    public class AddCartItemRequest
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        [Range(1, CartLimits.MaxQuantityPerItem)]
        public int Quantity { get; set; } = 1;
    }

}