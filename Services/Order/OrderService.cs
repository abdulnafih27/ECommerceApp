using AutoMapper;
using ECommerceApp.DTOs.Requests.Orders;
using ECommerceApp.DTOs.Responses.Orders;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ECommerceApp.Services.Orders
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
                                    .OrderByDescending(o => o.CreatedOn)
                                    .ThenByDescending(o => o.OrderId)
                                    .Select(SummaryProjection)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .AsNoTracking()
                                    .ToListAsync(cancellationToken);

            var res = PagedResult<OrderSummaryResponse>.Create(orders, pagination, ordersCount);

            return ServiceResult<PagedResult<OrderSummaryResponse>>.Success(res);
        }
        public async Task<ServiceResult<OrderDetailsResponse>> GetMyOrder(int userId, int orderId, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                                            .Include(o => o.Items.OrderBy(oi => oi.OrderItemId))
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("Get Order Details Failed: Order doesn't exist {OrderId}", orderId);
                return ServiceResult<OrderDetailsResponse>.Failed("Order doesn't exist", ErrorType.NotFound);
            }

            var res = _mapper.Map<OrderDetailsResponse>(order);

            return ServiceResult<OrderDetailsResponse>.Success(res);
        }
        public async Task<ServiceResult<OrderDetailsResponse>> PlaceOrder(int userId, PlaceOrderRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // Admin Service
        public async Task<ServiceResult<PagedResult<OrderSummaryResponse>>> GetAllOrders(Pagination pagination, CancellationToken cancellationToken)
        {
            var page = pagination.Page;
            var pageSize = pagination.PageSize;

            var query = _context.Orders;

            var ordersCount = await query.CountAsync(cancellationToken);
            var orders = await query
                                    .OrderByDescending(o => o.CreatedOn)
                                    .ThenByDescending(o => o.OrderId)
                                    .Select(SummaryProjection)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .AsNoTracking()
                                    .ToListAsync(cancellationToken);

            var res = PagedResult<OrderSummaryResponse>.Create(orders, pagination, ordersCount);

            return ServiceResult<PagedResult<OrderSummaryResponse>>.Success(res);
        }
        public async Task<ServiceResult<OrderDetailsResponse>> GetOrderById(int orderId, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                                            .Include(o => o.Items.OrderBy(oi => oi.OrderItemId))
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("Get Order Details Failed: Order doesn't exist {OrderId}", orderId);
                return ServiceResult<OrderDetailsResponse>.Failed("Order doesn't exist", ErrorType.NotFound);
            }

            var res = _mapper.Map<OrderDetailsResponse>(order);

            return ServiceResult<OrderDetailsResponse>.Success(res);
        }

        private static readonly Expression<Func<Order, OrderSummaryResponse>> SummaryProjection = order => new OrderSummaryResponse
        { 
            OrderId = order.OrderId,
            UserId = order.UserId,
            ShippingFullName = order.ShippingFullName,
            CreatedOn = order.CreatedOn,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            ItemCount = order.Items.Count
        };
    }
}

