using AutoMapper;
using ECommerceApp.DTOs.Requests.Category;
using ECommerceApp.DTOs.Responses.Category;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services.Category
{
    public class CategoryService //: ICategoryService
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
        //public async Task<ServiceResult<CategoryDetailsResponse>> GetCategory(int categoryId, CancellationToken cancellationToken)
        //{

        //}
        //public async Task<ServiceResult<CategoryDetailsResponse>> CreateCategory(CreateCategoryRequest request, CancellationToken cancellationToken)
        //{

        //}
        //public async Task<ServiceResult<CategoryDetailsResponse>> UpdateCategory(int categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken)
        //{

        //}
        //public async Task<ServiceResult> DeleteCategory(int categoryId, CancellationToken cancellationToken)
        //{

        //}
    }
}
