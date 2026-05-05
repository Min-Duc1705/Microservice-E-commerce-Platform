using CommonService.Annotations;
using CommonService.Common;
using Microsoft.AspNetCore.Authorization;
using CommonService.Filters;
using Microsoft.AspNetCore.Mvc;
using ProductService.Models.Request;
using ProductService.Models.Response;
using ProductService.Services.Interface;
using ProductService.Utils.Enum;

namespace ProductService.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET: /api/v1/products?searchTerm=áo&categoryId=...&status=0&pageNumber=1&pageSize=10
    [HttpGet]
    [AllowAnonymous]
    [ApiMessage("Lấy danh sách hàng hóa thành công")]
    public async Task<ActionResult<ResultPaginationDto<ProductResponse>>> GetAllProducts(
        [FromQuery] ProductFilterRequest filter)
    {
        var result = await _productService.GetPagedProductsAsync(filter);
        return Ok(result);
    }

    // GET: /api/v1/products/{id}
    [HttpGet("{id}")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [ApiMessage("Lấy thông tin hàng hóa thành công")]
    public async Task<ActionResult<ProductResponse>> GetProductById(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return Ok(product);
    }

    // POST: /api/v1/products
    [HttpPost]
    [RequiresPermission("POST", "/api/v1/products")]
    [ApiMessage("Thêm mới hàng hóa thành công")]
    public async Task<ActionResult<ProductResponse>> CreateProduct(
        [FromBody] CreateProductRequest request)
    {
        var product = await _productService.CreateProductAsync(request);
        return StatusCode(201, product);
    }

    // PUT: /api/v1/products/{id}
    [HttpPut("{id}")]
    [RequiresPermission("PUT", "/api/v1/products/{id}")]
    [ApiMessage("Cập nhật hàng hóa thành công")]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(
        Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = await _productService.UpdateProductAsync(id, request);
        return Ok(product);
    }

    // DELETE: /api/v1/products/{id}  (Soft delete)
    [HttpDelete("{id}")]
    [RequiresPermission("DELETE", "/api/v1/products/{id}")]
    [ApiMessage("Xóa hàng hóa thành công")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _productService.DeleteProductAsync(id);
        return Ok(null);
    }

    // PATCH: /api/v1/products/{id}/status
    // Dùng để Khóa (Ngừng bán) hoặc mở khóa sản phẩm (Impl.md)
    [HttpPatch("{id}/status")]
    [RequiresPermission("PATCH", "/api/v1/products/{id}/status")]
    [ApiMessage("Cập nhật trạng thái hàng hóa thành công")]
    public async Task<IActionResult> ToggleProductStatus(
        Guid id, [FromBody] ProductStatus newStatus)
    {
        await _productService.ToggleProductStatusAsync(id, newStatus);
        return Ok(null);
    }

    // PATCH: /api/v1/products/{id}/restore  — Khôi phục sản phẩm đã soft delete
    [HttpPatch("{id}/restore")]
    [RequiresPermission("PATCH", "/api/v1/products/{id}/restore")]
    [ApiMessage("Khôi phục hàng hóa thành công")]
    public async Task<IActionResult> RestoreProduct(Guid id)
    {
        await _productService.RestoreProductAsync(id);
        return Ok(null);
    }
}
