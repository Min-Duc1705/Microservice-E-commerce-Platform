using CustomerService.Utils.Enum;

namespace CustomerService.Models.Request;

/// <summary>
/// Tham số filter + phân trang cho GET /api/customers
/// Dùng [FromQuery] trong Controller
/// </summary>
public class CustomerFilterRequest
{
    private int _pageSize = 10;
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    /// <summary>Tìm kiếm theo Tên, SĐT, hoặc Email (theo Impl.md)</summary>
    public string? SearchTerm { get; set; }

    /// <summary>Lọc theo trạng thái (Active/Blocked)</summary>
    public CustomerStatus? Status { get; set; }

    // Sorting
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; } = false;

    // Date Range Filter
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    /// <summary>Bao gồm cả bản ghi đã xóa (Soft Delete)</summary>
    public bool IncludeDeleted { get; set; } = false;
}
