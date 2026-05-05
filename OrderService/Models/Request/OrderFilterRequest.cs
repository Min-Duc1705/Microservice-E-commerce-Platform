using CommonService.Common;
using OrderService.Utils.Enum;

namespace OrderService.Models.Request;

/// <summary>
/// DTO chứa tham số filter + phân trang cho GET /api/orders.
/// Được gắn vào query string: ?pageNumber=1&pageSize=10&status=Processing
/// </summary>
public class OrderFilterRequest
{
    private int _pageSize = 10;
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    // Các điều kiện lọc đặc thù của Order
    public Guid? CustomerId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchTerm { get; set; }

    // Sorting
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; } = true;
}
