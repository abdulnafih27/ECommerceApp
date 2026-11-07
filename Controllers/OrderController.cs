using ECommerceApp.DTOs.Requests.Orders;
using ECommerceApp.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController :ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyOrders([FromQuery] Pagination pagination, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUser();
            var res = await _orderService.GetMyOrders(userId, pagination, cancellationToken);
            return this.ToActionResult(res);
        }

        [HttpGet("me/{orderId}")]
        public async Task<IActionResult> GetMyOrders(int orderId, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUser();
            var res = await _orderService.GetMyOrder(userId, orderId, cancellationToken);
            return this.ToActionResult(res);
        }

        [HttpPost("me")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUser();
            var res = await _orderService.PlaceOrder(userId, request, cancellationToken);
            return this.ToActionResult(res);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders([FromQuery] Pagination pagination, CancellationToken cancellationToken)
        {
            var res = await _orderService.GetAllOrders(pagination, cancellationToken);
            return this.ToActionResult(res);
        }

        [HttpGet("{orderId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders(int orderId, CancellationToken cancellationToken)
        {
            var res = await _orderService.GetOrderById(orderId, cancellationToken);
            return this.ToActionResult(res);
        }

    }
}
