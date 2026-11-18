

using ECommerceApp.Common;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs.Requests.Orders
{
    public class PlaceOrderRequest
    {
        [Required]
        [MaxLength(OrderLimits.ShippingFullNameMaxLength)]
        public string? ShippingFullName { get; set => field = value.TrimToNull(); }
        [Required]
        [Phone]
        [MaxLength(OrderLimits.ShippingPhoneMaxLength)]
        public string? ShippingPhone { get; set => field = value.TrimToNull(); }
        [Required]
        [MaxLength(OrderLimits.ShippingAddressLineMaxLength)]
        public string? ShippingAddressLine { get; set => field = value.TrimToNull(); }
        [Required]
        [MaxLength(OrderLimits.ShippingCityMaxLength)]
        public string? ShippingCity { get; set => field = value.TrimToNull(); }
        [Required]
        [MaxLength(OrderLimits.ShippingStateMaxLength)]
        public string? ShippingState { get; set => field = value.TrimToNull(); }
        [Required]
        [MaxLength(OrderLimits.ShippingPostalCodeMaxLength)]
        public string? ShippingPostalCode { get; set => field = value.TrimToNull(); } 

        [EnumDataType(typeof(PaymentMethod))]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
    }
}
