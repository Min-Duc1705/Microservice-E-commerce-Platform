using System.Linq.Expressions;
using CommonService.Specifications;
using OrderService.Models;
using OrderService.Utils.Enum;

namespace OrderService.Specifications;

/// <summary>
/// Specification để lọc và phân trang danh sách đơn hàng.
/// Tất cả logic WHERE, ORDER BY, PAGING đều nằm gọn trong constructor.
/// </summary>
public class OrderFilterSpec : BaseSpecification<Order>
{
    public OrderFilterSpec(
        Guid? customerId,
        OrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize)
        : base(o =>
            (!customerId.HasValue || o.CustomerId == customerId.Value) &&
            (!status.HasValue || o.Status == status.Value) &&
            (!fromDate.HasValue || o.CreatedAt >= fromDate.Value) &&
            (!toDate.HasValue || o.CreatedAt <= toDate.Value) &&
            (string.IsNullOrEmpty(searchTerm) ||
                o.CustomerName.ToLower().Contains(searchTerm.ToLower()) ||
                o.CustomerPhone.Contains(searchTerm)))
    {
        // Include bảng OrderItems
        AddInclude(o => o.Items);

        // ===== Dynamic Sorting bằng Dictionary =====
        // Muốn thêm trường sort mới? Chỉ cần thêm 1 dòng vào dictionary!
        var sortMappings = new Dictionary<string, Expression<Func<Order, object>>>
        {
            ["createdAt"]    = o => o.CreatedAt,
            ["customername"] = o => o.CustomerName,
            ["status"]       = o => o.Status,
            ["shippingfee"]  = o => o.ShippingFee,
            // Thêm bao nhiêu trường tuỳ thích:
            // ["paymentmethod"] = o => o.PaymentMethod,
            // ["customerphone"] = o => o.CustomerPhone,
        };

        var sortKey = (sortBy ?? "createdAt").ToLower();
        var sortExpression = sortMappings.GetValueOrDefault(sortKey, sortMappings["createdAt"]);

        if (isDescending)
            AddOrderByDescending(sortExpression);
        else
            AddOrderBy(sortExpression);

        // Paging
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}

/// <summary>
/// Specification để đếm tổng số đơn hàng (không cần paging, chỉ cần WHERE)
/// </summary>
public class OrderFilterCountSpec : BaseSpecification<Order>
{
    public OrderFilterCountSpec(
        Guid? customerId,
        OrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm)
        : base(o =>
            (!customerId.HasValue || o.CustomerId == customerId.Value) &&
            (!status.HasValue || o.Status == status.Value) &&
            (!fromDate.HasValue || o.CreatedAt >= fromDate.Value) &&
            (!toDate.HasValue || o.CreatedAt <= toDate.Value) &&
            (string.IsNullOrEmpty(searchTerm) ||
                o.CustomerName.ToLower().Contains(searchTerm.ToLower()) ||
                o.CustomerPhone.Contains(searchTerm)))
    {
    }
}
