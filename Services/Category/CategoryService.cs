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

        List<CategoryDetailsResponse> res = _mapper.Map<List<CategoryDetailsResponse>>(categories);

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

        var categoryProducts = await _context.Products
                                                .Where(p => p.CategoryId == category.CategoryId)
                                                .AsNoTracking()
                                                .ToListAsync(cancellationToken);


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
    //public async Task<ServiceResult<CategoryDetailsResponse>> UpdateCategory(int categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken)
    //{

    //}
    //public async Task<ServiceResult> DeleteCategory(int categoryId, CancellationToken cancellationToken)
    //{

    //}
}

