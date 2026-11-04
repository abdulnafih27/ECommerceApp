using AutoMapper;
using ECommerceApp.DTOs.Requests.Orders;
using ECommerceApp.DTOs.Responses.Orders;
using ECommerceApp.Models;
using ECommerceApp.Services.Orders;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services.Order
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;
        private readonly IMapper _mapper;
        public OrderService(AppDbContext context, ILogger<OrderService> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ServiceResult<PagedResult<OrderSummaryResponse>>> GetMyOrders(int userId, Pagination pagination, CancellationToken cancellationToken)
        {
            var page = pagination.Page;
            var pageSize = pagination.PageSize;

            var query = _context.Orders.Where(o => o.UserId == userId);

            var ordersCount = await query.CountAsync(cancellationToken);
            var orders = await query
                                    .OrderBy(o => o.CreatedOn)
                                    .ThenBy(o => o.OrderId)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync(cancellationToken);

            var mappedOrders = _mapper.Map<List<OrderSummaryResponse>>(orders);
            var res = PagedResult<OrderSummaryResponse>.Create(mappedOrders, pagination, ordersCount);

            return ServiceResult<PagedResult<OrderSummaryResponse>>.Success(res);
        }
        public async Task<ServiceResult<OrderDetailsResponse>> GetMyOrder(int userId, int orderId, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                                            .Include(o => o.Items)
                                            .Where(o => o.OrderId == orderId && o.UserId == userId)
                                            .ToListAsync(cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("Get Order Details Failed: Order doesn't exist {OrderId}", orderId);
                return ServiceResult<OrderDetailsResponse>.Failed("Order doesn't exist", ErrorType.NotFound);
            }

            var res = _mapper.Map<OrderDetailsResponse>(order);

            return ServiceResult<OrderDetailsResponse>.Success(res);
        }
        //public async Task<ServiceResult<OrderDetailsResponse>> PlaceOrder(int userId, PlaceOrderRequest request, CancellationToken cancellationToken)
        //{

        //}

        // Admin Service
        public async Task<ServiceResult<PagedResult<OrderSummaryResponse>>> GetAllOrders(Pagination pagination, CancellationToken cancellationToken)
        {
            var page = pagination.Page;
            var pageSize = pagination.PageSize;

            var query = _context.Orders;

            var ordersCount = await query.CountAsync(cancellationToken);
            var orders = await query
                                    .OrderBy(o => o.CreatedOn)
                                    .ThenBy(o => o.OrderId)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync(cancellationToken);

            var mappedOrders = _mapper.Map<List<OrderSummaryResponse>>(orders);
            var res = PagedResult<OrderSummaryResponse>.Create(mappedOrders, pagination, ordersCount);

            return ServiceResult<PagedResult<OrderSummaryResponse>>.Success(res);
        }
        public async Task<ServiceResult<OrderDetailsResponse>> GetOrderById(int orderId, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                                            .Include(o => o.Items)
                                            .Where(o => o.OrderId == orderId)
                                            .ToListAsync(cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("Get Order Details Failed: Order doesn't exist {OrderId}", orderId);
                return ServiceResult<OrderDetailsResponse>.Failed("Order doesn't exist", ErrorType.NotFound);
            }

            var res = _mapper.Map<OrderDetailsResponse>(order);

            return ServiceResult<OrderDetailsResponse>.Success(res);
        }
    }
}

