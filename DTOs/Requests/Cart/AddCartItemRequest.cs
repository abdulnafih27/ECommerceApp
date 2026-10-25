using ECommerceApp.Common;
using ECommerceApp.Models;
using System.ComponentModel.DataAnnotations;


public class AddCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }
    [Range(1, CartLimits.MaxQuantityPerItem)]
    public int Quantity { get; set; } = 1;
}

