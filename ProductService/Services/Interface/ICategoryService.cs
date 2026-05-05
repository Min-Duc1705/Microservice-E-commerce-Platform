using CommonService.Common;
using ProductService.Models.Request;
using ProductService.Models.Response;

namespace ProductService.Services.Interface;

public interface ICategoryService
{
    /// <summary>Lấy danh sách danh mục có phân trang và lọc</summary>
    Task<ResultPaginationDto<CategoryResponse>> GetAllCategoriesAsync(CategoryFilterRequest request);

    /// <summary>Lấy danh sách danh mục cho dropdown (chỉ gồm Id và Name, không phân trang)</summary>
    Task<List<CategoryDropdownResponse>> GetCategoryDropdownAsync();

    /// <summary>Lấy chi tiết 1 danh mục theo ID</summary>
    Task<CategoryResponse> GetCategoryByIdAsync(Guid id);

    /// <summary>Tạo mới danh mục</summary>
    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);

    /// <summary>Cập nhật danh mục</summary>
    Task<CategoryResponse> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);

    /// <summary>Soft delete danh mục</summary>
    Task DeleteCategoryAsync(Guid id);

    /// <summary>Khôi phục danh mục đã soft delete</summary>
    Task RestoreCategoryAsync(Guid id);
}
