using AutoMapper;
using ECommerceApp.DTOs.Requests.Category;
using ECommerceApp.DTOs.Responses.Category;
using ECommerceApp.Extensions;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoryService> _logger;
    private readonly IMapper _mapper;

    public CategoryService(AppDbContext context, ILogger<CategoryService> logger, IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }
    public async Task<ServiceResult<List<CategoryDetailsResponse>>> GetAllCategories(CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
                                    .OrderBy(c => c.CategoryName)
                                    .ThenBy(c => c.CategoryId)
                                    .AsNoTracking()
                                    .ToListAsync(cancellationToken);

        var res = _mapper.Map<List<CategoryDetailsResponse>>(categories);

        return ServiceResult<List<CategoryDetailsResponse>>.Success(res);
    }

    //// Admin services
    public async Task<ServiceResult<CategoryDetailsResponse>> GetCategory(int categoryId, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(c => c.CategoryId == categoryId, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Get Category Details Failed: Category doesn't exist {CategoryId}", categoryId);
            return ServiceResult<CategoryDetailsResponse>.Failed("Category doesn't exist", ErrorType.NotFound);
        }

        return ServiceResult<CategoryDetailsResponse>.Success(_mapper.Map<CategoryDetailsResponse>(category));
    }
    public async Task<ServiceResult<CategoryDetailsResponse>> CreateCategory(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var isDuplicateCategory = await _context.Categories.AnyAsync(c => c.CategoryName == request.CategoryName, cancellationToken);

        if (isDuplicateCategory)
        {
            _logger.LogWarning("Create Category Failed: Category already exists {CategoryName}", request.CategoryName);
            return ServiceResult<CategoryDetailsResponse>.Failed("Category already exists", ErrorType.Conflict);
        }

        var newCategory = _mapper.Map<Category>(request);

        await _context.Categories.AddAsync(newCategory, cancellationToken);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            _logger.LogWarning("Create Failed: Category Name already exist {CategoryName}", request.CategoryName);
            return ServiceResult<CategoryDetailsResponse>.Failed("Category Name already exist.", ErrorType.Conflict);
        }

        var res = _mapper.Map<CategoryDetailsResponse>(newCategory);

        _logger.LogInformation("Category Creation Successful: category name {CategoryName}", res.CategoryName);
        return ServiceResult<CategoryDetailsResponse>.Success(res);
    }
    public async Task<ServiceResult<CategoryDetailsResponse>> UpdateCategory(int categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
                                        .FirstOrDefaultAsync(c => c.CategoryId == categoryId, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Update Details Failed: Category doesn't exist {CategoryId}", categoryId);
            return ServiceResult<CategoryDetailsResponse>.Failed("Category doesn't exist", ErrorType.NotFound);
        }

        if(request.CategoryName != null)
        {
            var isDuplicateCategory = await _context.Categories.AnyAsync(c => c.CategoryId != categoryId && c.CategoryName == request.CategoryName, cancellationToken);

            if (isDuplicateCategory)
            {
                _logger.LogWarning("Update Category Failed: Category already exists {CategoryName}", request.CategoryName);
                return ServiceResult<CategoryDetailsResponse>.Failed("Category already exists", ErrorType.Conflict);
            }

            category.CategoryName = request.CategoryName;
        }

        if(request.CategoryDescription != null)
        {
            category.CategoryDescription = request.CategoryDescription;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            _logger.LogWarning("Update Category Failed: Category name already exists {CategoryId}", categoryId);
            return ServiceResult<CategoryDetailsResponse>.Failed("Category name already exists", ErrorType.Conflict);
        }

        var res = _mapper.Map<CategoryDetailsResponse>(category);

        _logger.LogInformation("Category Update Successful: category {CategoryId}", categoryId);
        return ServiceResult<CategoryDetailsResponse>.Success(res);

    }
    public async Task<ServiceResult> DeleteCategory(int categoryId, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(c => c.CategoryId == categoryId, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Delete Category Failed: Category doesn't exist {CategoryId}", categoryId);
            return ServiceResult.Failed("Category doesn't exist", ErrorType.NotFound);
        }

        var categoryHasProducts = await _context.Products.IgnoreQueryFilters().AnyAsync(p => p.CategoryId == categoryId, cancellationToken);
        if (categoryHasProducts)
        {
            _logger.LogWarning("Delete Category Failed: Category still has products {CategoryId}", categoryId);
            return ServiceResult.Failed("Category still has products", ErrorType.Conflict);
        }

        _context.Categories.Remove(category);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch(DbUpdateException ex) when (ex.IsForeignKeyViolation())
        {
            _logger.LogWarning("Delete Category Failed: Category still has products {CategoryId}", categoryId);
            return ServiceResult.Failed("Category still has products", ErrorType.Conflict);
        }
      
        _logger.LogInformation("Delete Category Successful {CategoryId}", categoryId); 
        return ServiceResult.Success();
    }
}

