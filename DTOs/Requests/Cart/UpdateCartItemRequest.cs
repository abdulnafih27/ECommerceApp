using ECommerceApp.Common;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs.Requests.Cart
{
    public class UpdateCartItemRequest
    {
        [Range(1, CartLimits.MaxQuantityPerItem)]
        public int Quantity { get; set; }
    }
}
