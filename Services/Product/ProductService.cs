using AutoMapper;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductService> _logger;
    private readonly IMapper _mapper;

    public ProductService(AppDbContext context, ILogger<ProductService> logger, IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }
    public async Task<ServiceResult<PagedResult<ProductDetailsResponse>>> GetAllProducts(Pagination pagination, CancellationToken cancellationToken)
    {
        var page = pagination.Page;
        var pageSize = pagination.PageSize;
        
        var productsCount = await _context.Products.CountAsync(cancellationToken);
        var products = await _context.Products
                                    .OrderBy(p => p.ProductName)
                                    .ThenBy(p => p.ProductId)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .AsNoTracking()
                                    .ToListAsync(cancellationToken);

        PagedResult<ProductDetailsResponse> res = PagedResult<ProductDetailsResponse>.Create(_mapper.Map<List<ProductDetailsResponse>>(products), pagination, productsCount);
        
        return ServiceResult<PagedResult<ProductDetailsResponse>>.Success(res);
    }
    public async Task<ServiceResult<ProductDetailsResponse>> GetProduct(int productId, CancellationToken cancellationToken)
    {
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Get Product Details Failed: Product doesn't exist {ProductId}",productId);
            return ServiceResult<ProductDetailsResponse>.Failed("Product doesn't exist", ErrorType.NotFound);
        }

        return ServiceResult<ProductDetailsResponse>.Success(_mapper.Map<ProductDetailsResponse>(product));
    }

    // Admin services
    public async Task<ServiceResult<ProductDetailsResponse>> CreateProduct(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var isValidCategory = await _context.Categories.AnyAsync(c => c.CategoryId == request.CategoryId, cancellationToken);

        if (!isValidCategory)
        {
            _logger.LogWarning("Update Failed: Category doesnt exist {CategoryId}", request.CategoryId);
            return ServiceResult<ProductDetailsResponse>.Failed("Category doesnt exist", ErrorType.NotFound);
        }

        var isProductDublicate = await _context.Products
                                                .AnyAsync(p => p.CategoryId == request.CategoryId && 
                                                                p.ProductName == request.ProductName,
                                                                cancellationToken);
        
        if (isProductDublicate)
        {
             _logger.LogWarning("Create Product Failed: Same Product already exists in the same category {ProductName}", request.ProductName);
            return ServiceResult<ProductDetailsResponse>.Failed("Same Product already exists in the same category", ErrorType.Conflict);
        }

        var newProduct = _mapper.Map<Product>(request);

        await _context.Products.AddAsync(newProduct, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var res = _mapper.Map<ProductDetailsResponse>(newProduct);

        _logger.LogInformation("Product Creation Successfull: product name {ProductName}", res.ProductName);
        return ServiceResult<ProductDetailsResponse>.Success(res);
    }
    public async Task<ServiceResult<ProductDetailsResponse>> UpdateProduct(int productId, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);

        if(product == null)
        {
            _logger.LogWarning("Update Password Failed: Product doesn't exist {ProductId}",productId);
            return ServiceResult<ProductDetailsResponse>.Failed("Product doesn't exist", ErrorType.NotFound);
        }

        // Update the product name and product category
        if(request.ProductName != null)
        {
            var categoryId = request.CategoryId ?? product.CategoryId;

            var isProductDuplicate = await _context.Products
                                                    .AnyAsync(p => p.ProductId != productId && 
                                                                p.CategoryId == categoryId && 
                                                                p.ProductName == request.ProductName,
                                                                cancellationToken);
            
            if (isProductDuplicate)
            {
                _logger.LogWarning("Update Failed: duplicate Product name in category {CategoryId}", categoryId);
                return ServiceResult<ProductDetailsResponse>.Failed("Product Name already exists in this category", ErrorType.Conflict);
            }

            product.ProductName = request.ProductName;
        }

        if(request.CategoryId.HasValue)
        {
            var isValidCategory = await _context.Categories.AnyAsync(c => c.CategoryId == request.CategoryId, cancellationToken);

            if (!isValidCategory)
            {
                _logger.LogWarning("Update Failed: Category doesnt exist {CategoryId}", request.CategoryId);
                return ServiceResult<ProductDetailsResponse>.Failed("Category doesnt exist", ErrorType.NotFound);
            }

            var isProductDuplicate = await _context.Products
                                                    .AnyAsync(p => p.ProductId != productId && 
                                                                p.CategoryId == request.CategoryId && 
                                                                p.ProductName == product.ProductName,
                                                                cancellationToken);
             if (isProductDuplicate)
            {
                _logger.LogWarning("Update Failed: duplicate Product name in category {CategoryId}", request.CategoryId);
                return ServiceResult<ProductDetailsResponse>.Failed("Product Name already exists in this category", ErrorType.Conflict);
            }

            product.CategoryId = request.CategoryId.Value;
        }
        // Update the product description
        if(request.ProductDescription != null)
        {
            product.ProductDescription = request.ProductDescription;
        }

        // Update the product stock quantity
        if(request.StockQuantity.HasValue)
        {
            product.StockQuantity = request.StockQuantity.Value;
        }

        // Update the product price
        if(request.Price.HasValue)
        {
            product.Price = request.Price.Value;
        }

        // Update the product image
        if(request.ImageUrl != null)
        {
            product.ImageUrl = request.ImageUrl;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Update Successfull for product {ProductId}", productId);
        return ServiceResult<ProductDetailsResponse>.Success(_mapper.Map<ProductDetailsResponse>(product));
    }
    public async Task<ServiceResult> DeleteProduct(int productId, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);

        if(product == null)
        {
            _logger.LogWarning("Delete Product Failed: Product doesn't exist {ProductId}",productId);
            return ServiceResult.Failed("Product doesn't exist", ErrorType.NotFound);
        }

        product.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductName} was deleted successfully", product.ProductName);
        return ServiceResult.Success();
    }
}