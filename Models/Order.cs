public class Order : IAuditable
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ShippingFullName { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingAddressLine { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingPostalCode { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public ICollection<OrderItem> Items {get; set;} = new List<OrderItem>();

}