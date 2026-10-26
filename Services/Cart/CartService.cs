using AutoMapper;
using ECommerceApp.Common;
using ECommerceApp.DTOs.Requests.Cart;
using ECommerceApp.Extensions;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace ECommerceApp.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CartService> _logger;
        private readonly IMapper _mapper;
        public CartService(AppDbContext context, ILogger<CartService> logger, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<ServiceResult<CartResponse>> GetCartItems(int userId, CancellationToken cancellationToken)
        {
            var cartItems = await _context.CartItems
                                            .Include(ci => ci.Product)
                                            .IgnoreQueryFilters()
                                            .Where(ci => ci.UserId == userId)
                                            .OrderBy(ci => ci.CreatedOn)
                                            .ThenBy(ci => ci.CartItemId)
                                            .AsNoTracking()
                                            .ToListAsync(cancellationToken);

            var items = _mapper.Map<List<CartItemDetailsResponse>>(cartItems);
            var totalAmount = cartItems.Where(ci => !ci.Product!.IsDeleted).Sum(ci => ci.Quantity * ci.Product!.Price);
            var hasUnavailableItems = items.Any(r => !r.IsAvailable);

            var res = new CartResponse
            {
                TotalAmount = totalAmount,
                HasUnavailableItems = hasUnavailableItems,
                Items = items
            };

            return ServiceResult<CartResponse>.Success(res);
        } 
        public async Task<ServiceResult<CartItemDetailsResponse>> AddCartItem(int userId, AddCartItemRequest request, CancellationToken cancellationToken)
        {
            // Check if the user can add more items in the cart

            var cartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == request.ProductId, cancellationToken);
            
            // Check if the product is already in cart? if yes, update the quantity, else add to the cart
            if (cartItem != null)
            {
                // Check if the product exists and also if there is stock available
                var isProductValid = await ValidateProductAndStockAsync(request.ProductId, cartItem.Quantity + request.Quantity, cancellationToken);
                if (!isProductValid.IsSuccess)
                {
                    return ServiceResult<CartItemDetailsResponse>.Failed(isProductValid.Error!, isProductValid.ErrorType);
                }

                // Check if the quantity is exceeding the limit 
                if (cartItem.Quantity + request.Quantity > CartLimits.MaxQuantityPerItem)
                {
                    _logger.LogWarning("Add Item to cart Failed: Product {ProductId} quantity exceeds limit:{Limit}", request.ProductId, CartLimits.MaxQuantityPerItem);
                    return ServiceResult<CartItemDetailsResponse>.Failed($"Product quantity exceeds limit: {CartLimits.MaxQuantityPerItem}", ErrorType.ValidationFailed);
                }
                cartItem.Quantity += request.Quantity;
                
                await _context.SaveChangesAsync(cancellationToken);
                
                var res = _mapper.Map<CartItemDetailsResponse>(cartItem);
                
                _logger.LogInformation("Product Quantity Added to Cart: Product {ProductId}", res.ProductId);
                return ServiceResult<CartItemDetailsResponse>.Success(res);
            }
            else
            {
                var userCartCount = await _context.CartItems.Where(ci => ci.UserId == userId).CountAsync(cancellationToken);
                if(userCartCount >= CartLimits.MaxDistinctItems)
                {
                    _logger.LogWarning("Add Item to cart Failed: Max cart item limit reached {Limit}", CartLimits.MaxDistinctItems);
                    return ServiceResult<CartItemDetailsResponse>.Failed($"Max cart item limit reached: {CartLimits.MaxDistinctItems}", ErrorType.ValidationFailed);
                }

                // Check if the product exists and also if there is stock available
                var isProductValid = await ValidateProductAndStockAsync(request.ProductId, request.Quantity, cancellationToken);
                if (!isProductValid.IsSuccess)
                {
                    return ServiceResult<CartItemDetailsResponse>.Failed(isProductValid.Error!, isProductValid.ErrorType);
                }

                var newCartItem = _mapper.Map<CartItem>(request);
                newCartItem.UserId = userId;

                await _context.CartItems.AddAsync(newCartItem, cancellationToken);
                try
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch(DbUpdateException ex) when (ex.IsUniqueViolation())
                {
                    _logger.LogWarning("Add Item to cart Failed: Product already in the Cart {ProductId}", request.ProductId);
                    return ServiceResult<CartItemDetailsResponse>.Failed("Product already in the Cart", ErrorType.Conflict);
                }
                
                var res = _mapper.Map<CartItemDetailsResponse>(newCartItem);
                res.IsAvailable = true;

                _logger.LogInformation("Product Successfully Added to Cart: Product {ProductId}", res.ProductId);
                return ServiceResult<CartItemDetailsResponse>.Success(res);
            }


        }
        public async Task<ServiceResult<CartItemDetailsResponse>> UpdateCartItem(int userId, int cartItemId, UpdateCartItemRequest request, CancellationToken cancellationToken)
        {
            // Check if the item is in the cart
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.CartItemId == cartItemId, cancellationToken);
            if(cartItem == null)
            {
                _logger.LogWarning("Update CartItem Failed: Cart Item not found {CartItemId}", cartItemId);
                return ServiceResult<CartItemDetailsResponse>.Failed("Cart Item not found", ErrorType.NotFound);
            }

            // Check if the updated quantity is more the limit
            if (request.Quantity > CartLimits.MaxQuantityPerItem)
            {
                _logger.LogWarning("Update CartItem Failed: Product {CartItemId} quantity exceeds limit:{Limit}", cartItemId, CartLimits.MaxQuantityPerItem);
                return ServiceResult<CartItemDetailsResponse>.Failed($"Product quantity exceeds limit: {CartLimits.MaxQuantityPerItem}", ErrorType.ValidationFailed);
            }

            // Also check if there is enough stock to add to the cart
            var isStockValid = await ValidateProductAndStockAsync(cartItem.ProductId, request.Quantity, cancellationToken);
            if (!isStockValid.IsSuccess)
            {
                return ServiceResult<CartItemDetailsResponse>.Failed(isStockValid.Error!, isStockValid.ErrorType);
            }

            cartItem.Quantity = request.Quantity;

            await _context.SaveChangesAsync(cancellationToken);
            
            var res = _mapper.Map<CartItemDetailsResponse>(cartItem);
            res.IsAvailable = true;

            _logger.LogInformation("Cart Item Update Successful: Cart Item {CartItemId}", cartItemId);
            return ServiceResult<CartItemDetailsResponse>.Success(res);
        }
        public async Task<ServiceResult> DeleteCartItem(int userId, int cartItemId, CancellationToken cancellationToken)
        {
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(ci =>ci.UserId == userId && ci.CartItemId == cartItemId, cancellationToken);
            if (cartItem == null)
            {
                _logger.LogWarning("Delete CartItem Failed: Cart Item not found {CartItemId}", cartItemId);
                return ServiceResult.Failed("Cart Item not found", ErrorType.NotFound);
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cart Item deleted Successful: Cart Item {CartItemId}", cartItemId);
            return ServiceResult.Success();
        }

        // Helper functions
        private async Task<ServiceResult> ValidateProductAndStockAsync(int productId, int requestQuantity, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId,cancellationToken);

            if (product == null)
            {
                _logger.LogWarning("Product doesn't exist {ProductId}", productId);
                return ServiceResult.Failed("Product doesn't exist", ErrorType.ValidationFailed);
            }


            if (product.StockQuantity < requestQuantity)
            {
                _logger.LogWarning("Product doesn't have enough stock {ProductId}", productId);
                return ServiceResult.Failed("Product doesn't have enough stock", ErrorType.ValidationFailed);
            }

            return ServiceResult.Success();
        }
    }
}
