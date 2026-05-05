using PaymentService.Utils.Enum;

namespace PaymentService.Models.Request;

/// <summary>
/// Tham số filter + phân trang cho GET /api/payments
/// </summary>
public class PaymentFilterRequest
{
    private int _pageSize = 10;
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    /// <summary>Lọc theo đơn hàng cụ thể</summary>
    public Guid? OrderId { get; set; }

    /// <summary>Lọc theo khách hàng cụ thể</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>Lọc theo phương thức thanh toán</summary>
    public PaymentMethod? Method { get; set; }

    /// <summary>Lọc theo trạng thái giao dịch</summary>
    public PaymentStatus? Status { get; set; }

    public string? SortBy { get; set; }
    public bool IsDescending { get; set; } = true;

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
