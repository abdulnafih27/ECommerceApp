using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] Pagination pagination, CancellationToken cancellationToken)
        {
            var res = await _productService.GetAllProducts(pagination, cancellationToken);
            return this.ToActionResult(res);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProduct(int productId, CancellationToken cancellationToken)
        {
            var res = await _productService.GetProduct(productId, cancellationToken);
            return this.ToActionResult(res);
        }

        // Admin controls
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
        {
            var res = await _productService.CreateProduct(request, cancellationToken);
            return this.ToActionResult(res);
        }

        [HttpPatch("{productId}/update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int productId,[FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
        {
            var res = await _productService.UpdateProduct(productId, request, cancellationToken);
            return this.ToActionResult(res);
        }

        [HttpDelete("{productId}/delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int productId, CancellationToken cancellationToken)
        {
            var res = await _productService.DeleteProduct(productId, cancellationToken);
            return this.ToActionResult(res);
        }

    }
}
