
public class CartResponse
{
    public decimal TotalAmount { get; set; }
    public bool HasUnavailableItems { get; set; }
    public ICollection<CartItemDetailsResponse> Items { get; set; } = new List<CartItemDetailsResponse>();
}

