using ProductService.Utils.Enum;

namespace ProductService.Models.Request;

/// <summary>
/// Tham số filter + phân trang cho GET /api/v1/products
/// </summary>
public class ProductFilterRequest
{
    private int _pageSize = 10;
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    /// <summary>Tìm kiếm theo Tên hoặc Mã SKU (theo Impl.md)</summary>
    public string? SearchTerm { get; set; }

    /// <summary>Lọc theo Loại hàng hóa (danh mục)</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Lọc theo Trạng thái (Đang bán / Ngừng bán / Hết hàng)</summary>
    public ProductStatus? Status { get; set; }

    // Sorting
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; } = false;

    // Date Range Filter
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    /// <summary>Bao gồm cả bản ghi đã xóa (Soft Delete)</summary>
    public bool IncludeDeleted { get; set; } = false;
}
