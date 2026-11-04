using ECommerceApp.DTOs.Requests.Orders;
using ECommerceApp.DTOs.Responses.Orders;

namespace ECommerceApp.Services.Orders
{
    public interface IOrderService
    {
        public Task<ServiceResult<PagedResult<OrderSummaryResponse>>> GetMyOrders(int userId, Pagination pagination, CancellationToken cancellationToken);
        public Task<ServiceResult<OrderDetailsResponse>> GetMyOrder(int userId, int orderId, CancellationToken cancellationToken);
        //public Task<ServiceResult<OrderDetailsResponse>> PlaceOrder(int userId, PlaceOrderRequest request, CancellationToken cancellationToken);
        
        // Admin Service
        public Task<ServiceResult<PagedResult<OrderSummaryResponse>>> GetAllOrders(Pagination pagination, CancellationToken cancellationToken);
        public Task<ServiceResult<OrderDetailsResponse>> GetOrderById(int orderId, CancellationToken cancellationToken);
    }
}
