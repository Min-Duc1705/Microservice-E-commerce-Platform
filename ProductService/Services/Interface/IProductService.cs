using CommonService.Common;
using ProductService.Models.Request;
using ProductService.Models.Response;
using ProductService.Utils.Enum;

namespace ProductService.Services.Interface;

public interface IProductService
{
    /// <summary>Lấy danh sách sản phẩm có filter + phân trang</summary>
    Task<ResultPaginationDto<ProductResponse>> GetPagedProductsAsync(ProductFilterRequest filter);

    /// <summary>Lấy chi tiết 1 sản phẩm theo ID</summary>
    Task<ProductResponse> GetProductByIdAsync(Guid id);

    /// <summary>Tạo mới sản phẩm (SKU tự động nếu không nhập)</summary>
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request);

    /// <summary>Cập nhật thông tin sản phẩm</summary>
    Task<ProductResponse> UpdateProductAsync(Guid id, UpdateProductRequest request);

    /// <summary>Soft delete sản phẩm (theo Impl.md: giữ lịch sử bán hàng)</summary>
    Task DeleteProductAsync(Guid id);

    /// <summary>Khôi phục sản phẩm đã soft delete</summary>
    Task RestoreProductAsync(Guid id);

    /// <summary>Khóa / Mở khóa sản phẩm (chuyển trạng thái Ngừng bán / Đang bán)</summary>
    Task ToggleProductStatusAsync(Guid id, ProductStatus newStatus);
}
