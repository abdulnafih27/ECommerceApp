using ECommerceApp.DTOs.Responses.Product;
using ECommerceApp.DTOs.Requests.Product;

public interface IProductService
{
    public Task<ServiceResult<PagedResult<ProductDetailsResponse>>> GetAllProducts(Pagination pagination, int? categoryId, CancellationToken cancellationToken);
    public Task<ServiceResult<ProductDetailsResponse>> GetProduct(int productId, CancellationToken cancellationToken);


    // Admin services
    public Task<ServiceResult<ProductDetailsResponse>> CreateProduct(CreateProductRequest request, CancellationToken cancellationToken);
    public Task<ServiceResult<ProductDetailsResponse>> UpdateProduct(int productId, UpdateProductRequest request, CancellationToken cancellationToken);
    public Task<ServiceResult> DeleteProduct(int productId, CancellationToken cancellationToken);
    
}