namespace ECommerceApp.DTOs.Responses.Orders
{
    public class OrderSummaryResponse
    {
        public int OrderId { get; set; }
        public DateTime CreatedOn { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; } 
        public int ItemCount { get; set; }
    }
}
