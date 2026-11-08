using ECommerceApp.DTOs.Requests.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Authorize(policy:"ActiveStatus")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService) 
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItems(CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUser();
            var res = await _cartService.GetCartItems(userId, cancellationToken);
            return this.ToActionResult(res);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddCartItem([FromBody] AddCartItemRequest request, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUser();
            var res = await _cartService.AddCartItem(userId, request, cancellationToken);
            return this.ToActionResult(res);
        }

        [HttpPatch("{cartItemId}/update")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUser();
            var res = await _cartService.UpdateCartItem(userId, cartItemId, request, cancellationToken);
            return this.ToActionResult(res);
        }

        [HttpDelete("{cartItemId}/delete")]
        public async Task<IActionResult> DeleteCartItem(int cartItemId, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUser();
            var res = await _cartService.DeleteCartItem(userId, cartItemId, cancellationToken);
            return this.ToActionResult(res);
        }

    }
}
