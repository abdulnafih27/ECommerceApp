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

            var cartItems = await _context.CartItems
                                            .IgnoreQueryFilters()
                                            .Include(ci => ci.Product)
                                            .Where(ci => ci.UserId == userId)
                                            .OrderBy(ci => ci.ProductId)
                                            .ToListAsync(cancellationToken);
            
            if (cartItems.Count == 0)
            {
                _logger.LogWarning("Place Order Failed: No items in the cart");
                return ServiceResult<OrderDetailsResponse>.Failed("No items in the cart", ErrorType.ValidationFailed);
            }

            var noLongerAvailable = new List<string>();
            var notEnoughStock = new List<string>();
            foreach (var item in cartItems)
            {
                if (item.Product!.IsDeleted)
                {
                    noLongerAvailable.Add(item.Product.ProductName);
                }
                else if (item.Product.StockQuantity < item.Quantity)
                {
                    notEnoughStock.Add(item.Product.ProductName);
                }
            }

            if (noLongerAvailable.Count > 0 || notEnoughStock.Count > 0)
            {
                var errors = new List<string>();
                if (noLongerAvailable.Count > 0)
                    errors.Add($"no longer available: {string.Join(", ", noLongerAvailable)}");
                if (notEnoughStock.Count > 0)
                    errors.Add($"not enough stock: {string.Join(", ", notEnoughStock)}");

                string dynamicMessage = string.Join(" | ", errors);
                _logger.LogWarning("Place Order Failed: {ErrorMessage}", dynamicMessage);
                return ServiceResult<OrderDetailsResponse>.Failed(dynamicMessage, ErrorType.ValidationFailed);
            }


            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            foreach(var item in cartItems)
            {
                int rowsAffected = await _context.Products
                        .Where(p => !p.IsDeleted && p.ProductId == item.ProductId && p.StockQuantity >= item.Quantity)
                        .ExecuteUpdateAsync(setters => setters
                               .SetProperty(p => p.StockQuantity, p => p.StockQuantity - item.Quantity)
                               .SetProperty(p => p.ModifiedOn, DateTime.UtcNow), 
                               cancellationToken
                        );
                if (rowsAffected == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogWarning("Place Order Failed: Product stock changed concurrently or deleted for {ProductName}", item.Product!.ProductName);
                    return ServiceResult<OrderDetailsResponse>.Failed($"Product stock became unavailable for '{item.Product.ProductName}'. Please try again.", ErrorType.Conflict);
                }
            }

            var order = _mapper.Map<Order>(request);
            order.UserId = userId;

            var orderItems = _mapper.Map<List<OrderItem>>(cartItems);
            
            order.TotalAmount = orderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
            order.Items = orderItems;

            try
            {
                _context.CartItems.RemoveRange(cartItems);

                await _context.Orders.AddAsync(order, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch(DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Place Order Failed: Your cart changed, please try again");
                return ServiceResult<OrderDetailsResponse>.Failed("Your cart changed, please try again", ErrorType.Conflict);
            }
            

            var res = _mapper.Map<OrderDetailsResponse>(order);
            
            _logger.LogInformation("Place Order Successful: Order Placed {OrderId}", res.OrderId);
            return ServiceResult<OrderDetailsResponse>.Success(res);
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

