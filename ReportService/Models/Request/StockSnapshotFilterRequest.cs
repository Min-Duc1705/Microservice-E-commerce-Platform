namespace ReportService.Models.Request;

/// <summary>
/// Tham số filter + phân trang cho GET /api/reports/stock-snapshots
/// </summary>
public class StockSnapshotFilterRequest
{
    private int _pageSize = 20;
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    /// <summary>Tìm theo tên hoặc SKU sản phẩm</summary>
    public string? SearchTerm { get; set; }

    /// <summary>Lọc chỉ hàng sắp hết (StockQuantity &lt;= LowStockThreshold)</summary>
    public bool? LowStockOnly { get; set; }

    /// <summary>Tên danh mục</summary>
    public string? CategoryName { get; set; }

    public string? SortBy { get; set; } = "soldlast30days";
    public bool IsDescending { get; set; } = true;
}
