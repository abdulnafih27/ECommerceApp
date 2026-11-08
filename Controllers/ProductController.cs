using ECommerceApp.DTOs.Requests.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Authorize(policy:"ActiveStatus")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts([FromQuery] Pagination pagination,[FromQuery] int? categoryId, CancellationToken cancellationToken)
        {
            var res = await _productService.GetAllProducts(pagination, categoryId, cancellationToken);
            return this.ToActionResult(res);
        }

        [HttpGet("{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProduct(int productId, CancellationToken cancellationToken)
        {
            var res = await _productService.GetProduct(productId, cancellationToken);
            return this.ToActionResult(res);
        }

        // Admin controls
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
        {
            var res = await _productService.CreateProduct(request, cancellationToken);
            return this.ToActionResult(res);
        }

        [HttpPatch("{productId}/update")]
        public async Task<IActionResult> UpdateProduct(int productId,[FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
        {
            var res = await _productService.UpdateProduct(productId, request, cancellationToken);
            return this.ToActionResult(res);
        }

        [HttpDelete("{productId}/delete")]
        public async Task<IActionResult> DeleteProduct(int productId, CancellationToken cancellationToken)
        {
            var res = await _productService.DeleteProduct(productId, cancellationToken);
            return this.ToActionResult(res);
        }

    }
}
