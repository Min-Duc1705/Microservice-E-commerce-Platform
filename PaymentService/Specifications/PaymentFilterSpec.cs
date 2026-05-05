using System.Linq.Expressions;
using CommonService.Specifications;
using PaymentService.Models;
using PaymentService.Utils.Enum;

namespace PaymentService.Specifications;

/// <summary>
/// Filter + sort + phân trang danh sách PaymentTransaction.
/// </summary>
public class PaymentFilterSpec : BaseSpecification<PaymentTransaction>
{
    public PaymentFilterSpec(
        Guid? orderId,
        Guid? customerId,
        PaymentMethod? method,
        PaymentStatus? status,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize,
        DateTime? fromDate = null,
        DateTime? toDate = null)
        : base(t =>
            (!orderId.HasValue    || t.OrderId    == orderId.Value)    &&
            (!customerId.HasValue || t.CustomerId == customerId.Value) &&
            (!method.HasValue     || t.Method     == method.Value)     &&
            (!status.HasValue     || t.Status     == status.Value)     &&
            (!fromDate.HasValue   || t.CreatedAt  >= fromDate.Value)   &&
            (!toDate.HasValue     || t.CreatedAt  <= toDate.Value.AddDays(1)))
    {
        var sortMappings = new Dictionary<string, Expression<Func<PaymentTransaction, object>>>
        {
            ["amount"]    = t => t.Amount,
            ["status"]    = t => t.Status,
            ["method"]    = t => t.Method,
            ["paidat"]    = t => t.PaidAt!,
            ["createdat"] = t => t.CreatedAt,
        };

        var sortKey  = (sortBy ?? "createdat").ToLower();
        var sortExpr = sortMappings.GetValueOrDefault(sortKey, sortMappings["createdat"]);

        if (isDescending) AddOrderByDescending(sortExpr);
        else              AddOrderBy(sortExpr);

        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}

/// <summary>
/// Chỉ đếm tổng số giao dịch — không paging (để tính số trang).
/// </summary>
public class PaymentFilterCountSpec : BaseSpecification<PaymentTransaction>
{
    public PaymentFilterCountSpec(
        Guid? orderId,
        Guid? customerId,
        PaymentMethod? method,
        PaymentStatus? status,
        DateTime? fromDate = null,
        DateTime? toDate = null)
        : base(t =>
            (!orderId.HasValue    || t.OrderId    == orderId.Value)    &&
            (!customerId.HasValue || t.CustomerId == customerId.Value) &&
            (!method.HasValue     || t.Method     == method.Value)     &&
            (!status.HasValue     || t.Status     == status.Value)     &&
            (!fromDate.HasValue   || t.CreatedAt  >= fromDate.Value)   &&
            (!toDate.HasValue     || t.CreatedAt  <= toDate.Value.AddDays(1)))
    { }
}

/// <summary>Lấy 1 transaction theo ID.</summary>
public class PaymentByIdSpec : BaseSpecification<PaymentTransaction>
{
    public PaymentByIdSpec(Guid id) : base(t => t.Id == id) { }
}

/// <summary>Lấy transaction mới nhất theo OrderId — để kiểm tra trạng thái.</summary>
public class PaymentByOrderIdSpec : BaseSpecification<PaymentTransaction>
{
    public PaymentByOrderIdSpec(Guid orderId) : base(t => t.OrderId == orderId)
    {
        AddOrderByDescending(t => t.CreatedAt);
    }
}
