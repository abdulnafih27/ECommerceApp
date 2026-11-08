using ECommerceApp.DTOs.Requests.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
[Authorize(policy:"ActiveStatus")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllCategories(CancellationToken cancellationToken)
    {
        var res = await _categoryService.GetAllCategories(cancellationToken);
        return this.ToActionResult(res);
    }

    [HttpGet("{categoryId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategory(int categoryId, CancellationToken cancellationToken)
    {
        var res = await _categoryService.GetCategory(categoryId, cancellationToken);
        return this.ToActionResult(res);
    }


    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var res = await _categoryService.CreateCategory(request, cancellationToken);
        return this.ToActionResult(res);
    }

    [HttpPatch("{categoryId}/update")]
    public async Task<IActionResult> UpdateCategory(int categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var res = await _categoryService.UpdateCategory(categoryId, request, cancellationToken);
        return this.ToActionResult(res);
    }
    
    [HttpDelete("{categoryId}/delete")]
    public async Task<IActionResult> DeleteCategory(int categoryId, CancellationToken cancellationToken)
    {
        var res = await _categoryService.DeleteCategory(categoryId, cancellationToken);
        return this.ToActionResult(res);
    }

}

