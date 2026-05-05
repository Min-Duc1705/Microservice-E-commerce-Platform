using CommonService.Annotations;
using Microsoft.AspNetCore.Mvc;
using CommonService.Filters;
using ProductService.Models.Request;
using ProductService.Models.Response;
using ProductService.Services.Interface;

namespace ProductService.Controllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: /api/v1/categories?pageNumber=1&pageSize=10...
    [HttpGet]
    [RequiresPermission("GET", "/api/v1/categories")]
    [ApiMessage("Lấy danh sách loại hàng hóa thành công")]
    public async Task<ActionResult<CommonService.Common.ResultPaginationDto<CategoryResponse>>> GetAllCategories(
        [FromQuery] CategoryFilterRequest request)
    {
        var result = await _categoryService.GetAllCategoriesAsync(request);
        return Ok(result);
    }

    // GET: /api/v1/categories/dropdown
    [HttpGet("dropdown")]
    [RequiresPermission("GET", "/api/v1/categories/dropdown")]
    [ApiMessage("Lấy danh sách danh mục cho dropdown thành công")]
    public async Task<ActionResult<List<CategoryDropdownResponse>>> GetCategoryDropdown()
    {
        var result = await _categoryService.GetCategoryDropdownAsync();
        return Ok(result);
    }

    // GET: /api/v1/categories/{id}
    [HttpGet("{id}")]
    [RequiresPermission("GET", "/api/v1/categories/{id}")]
    [ApiMessage("Lấy thông tin loại hàng hóa thành công")]
    public async Task<ActionResult<CategoryResponse>> GetCategoryById(Guid id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        return Ok(category);
    }

    // POST: /api/v1/categories
    [HttpPost]
    [RequiresPermission("POST", "/api/v1/categories")]
    [ApiMessage("Tạo loại hàng hóa thành công")]
    public async Task<ActionResult<CategoryResponse>> CreateCategory(
        [FromBody] CreateCategoryRequest request)
    {
        var category = await _categoryService.CreateCategoryAsync(request);
        return StatusCode(201, category);
    }

    // PUT: /api/v1/categories/{id}
    [HttpPut("{id}")]
    [RequiresPermission("PUT", "/api/v1/categories/{id}")]
    [ApiMessage("Cập nhật loại hàng hóa thành công")]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(
        Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _categoryService.UpdateCategoryAsync(id, request);
        return Ok(category);
    }

    // DELETE: /api/v1/categories/{id}  (Soft delete)
    [HttpDelete("{id}")]
    [RequiresPermission("DELETE", "/api/v1/categories/{id}")]
    [ApiMessage("Xóa loại hàng hóa thành công")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        return Ok(null);
    }

    // PATCH: /api/v1/categories/{id}/restore  — Khôi phục danh mục đã soft delete
    [HttpPatch("{id}/restore")]
    [RequiresPermission("PATCH", "/api/v1/categories/{id}/restore")]
    [ApiMessage("Khôi phục loại hàng hóa thành công")]
    public async Task<IActionResult> RestoreCategory(Guid id)
    {
        await _categoryService.RestoreCategoryAsync(id);
        return Ok(null);
    }
}
