using ECommerceApp.DTOs.Requests.Cart;

public interface ICartService
{
    public Task<ServiceResult<CartResponse>> GetCartItems(int userId, CancellationToken cancellationToken);
    public Task<ServiceResult<CartItemDetailsResponse>> AddCartItem(int userId, AddCartItemRequest request, CancellationToken cancellationToken);
    public Task<ServiceResult<CartItemDetailsResponse>> UpdateCartItem(int userId, int cartItemId, UpdateCartItemRequest request, CancellationToken cancellationToken);
    public Task<ServiceResult> DeleteCartItem(int userId, int cartItemId, CancellationToken cancellationToken);
}
