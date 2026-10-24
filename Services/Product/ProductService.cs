using AutoMapper;
using ECommerceApp.Extensions;
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
        var checkValidProduct = await ValidateNameAndCategoryAsync(request.CategoryId, request.ProductName!, null,cancellationToken);

        if (!checkValidProduct.IsSuccess)
        {
            return ServiceResult<ProductDetailsResponse>.Failed(checkValidProduct.Error!, checkValidProduct.ErrorType);
        }

        var newProduct = _mapper.Map<Product>(request);

        await _context.Products.AddAsync(newProduct, cancellationToken);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            _logger.LogWarning("Create Failed: Product Name already exists in this category {ProductName}", request.ProductName);
            return ServiceResult<ProductDetailsResponse>.Failed("Product Name already exists in this category.", ErrorType.Conflict);
        }

        var res = _mapper.Map<ProductDetailsResponse>(newProduct);

        _logger.LogInformation("Product Creation Successful: product name {ProductName}", res.ProductName);
        return ServiceResult<ProductDetailsResponse>.Success(res);
    }
    public async Task<ServiceResult<ProductDetailsResponse>> UpdateProduct(int productId, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);

        if(product == null)
        {
            _logger.LogWarning("Update Failed: Product doesn't exist {ProductId}",productId);
            return ServiceResult<ProductDetailsResponse>.Failed("Product doesn't exist", ErrorType.NotFound);
        }


        if(request.ProductName != null || request.CategoryId.HasValue)
        {
            var productName = request.ProductName ?? product.ProductName;
            var categoryId = request.CategoryId ?? product.CategoryId;
            
            var checkValidProduct = await ValidateNameAndCategoryAsync(categoryId, productName, productId, cancellationToken);

            if (!checkValidProduct.IsSuccess)
            {
                return ServiceResult<ProductDetailsResponse>.Failed(checkValidProduct.Error!, checkValidProduct.ErrorType);
            }
        }

        // Patch modified product properties from the request
        if(request.ProductName != null)
        {
            product.ProductName = request.ProductName;
        }

        if(request.CategoryId.HasValue)
        {
            product.CategoryId = request.CategoryId.Value;
        }

        if(request.ProductDescription != null)
        {
            product.ProductDescription = request.ProductDescription.Trim();
        }

        if(request.StockQuantity.HasValue)
        {
            product.StockQuantity = request.StockQuantity.Value;
        }
        
        if(request.Price.HasValue)
        {
            product.Price = request.Price.Value;
        }

        if(request.ImageUrl != null)
        {
            product.ImageUrl = request.ImageUrl;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Update Failed: Product was updated recently please reload {ProductId}", productId);
            return ServiceResult<ProductDetailsResponse>.Failed("Product was updated recently please reload", ErrorType.Conflict);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            _logger.LogWarning("Create Failed: Product Name already exists in this category {ProductId}", productId);
            return ServiceResult<ProductDetailsResponse>.Failed("Product Name already exists in this category.", ErrorType.Conflict);
        }

        _logger.LogInformation("Update Successful for product {ProductId}", productId);
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
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Delete Failed: Product was modified recently, please reload {ProductId}", productId);
            return ServiceResult.Failed("Product was modified recently, please reload", ErrorType.Conflict);
        }

        _logger.LogInformation("Product {ProductName} was deleted successfully", product.ProductName);
        return ServiceResult.Success();
    }
    
    // Helper functions

    // This private method is used to check if the product and category combination is unique in the Database
    private async Task<ServiceResult> ValidateNameAndCategoryAsync(int categoryId, string name, int? excludeProductId, CancellationToken cancellationToken)
    {
            var isValidCategory = await _context.Categories.AnyAsync(c => c.CategoryId == categoryId, cancellationToken);

            if (!isValidCategory)
            {
                _logger.LogWarning("Category doesn't exist {CategoryId}", categoryId);
                return ServiceResult.Failed("Category doesn't exist", ErrorType.ValidationFailed);
            }

            var isProductDuplicate = await _context.Products
                                                    .IgnoreQueryFilters()
                                                    .AnyAsync(p => p.ProductId != excludeProductId && 
                                                                p.CategoryId == categoryId && 
                                                                p.ProductName == name,
                                                                cancellationToken);
            
            if (isProductDuplicate)
            {
                _logger.LogWarning("Duplicate Product name in category {CategoryId}", categoryId);
                return ServiceResult.Failed("Product Name already exists in this category", ErrorType.Conflict);
            }

        return ServiceResult.Success();
    }
}