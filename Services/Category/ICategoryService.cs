using ECommerceApp.DTOs.Requests.Categories;
using ECommerceApp.DTOs.Responses.Categories;
public interface ICategoryService
{
    public Task<ServiceResult<List<CategoryDetailsResponse>>> GetAllCategories(CancellationToken cancellationToken);

    // Admin services
    public Task<ServiceResult<CategoryDetailsResponse>> GetCategory(int categoryId, CancellationToken cancellationToken);
    public Task<ServiceResult<CategoryDetailsResponse>> CreateCategory(CreateCategoryRequest request, CancellationToken cancellationToken);
    public Task<ServiceResult<CategoryDetailsResponse>> UpdateCategory(int categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken);
    public Task<ServiceResult> DeleteCategory(int categoryId, CancellationToken cancellationToken);
}

