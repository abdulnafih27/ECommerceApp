namespace ECommerceApp.Models
{
    public class Order : IAuditable
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingFullName { get; set; } = string.Empty;
        public string ShippingPhone { get; set; } = string.Empty;
        public string ShippingAddressLine { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public OrderStatus Status { get; set; } = OrderStatus.Placed;
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    }
}