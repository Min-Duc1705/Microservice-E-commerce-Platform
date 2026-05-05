using System.Linq.Expressions;
using CommonService.Specifications;
using CustomerService.Models;
using CustomerService.Utils.Enum;

namespace CustomerService.Specifications;

/// <summary>
/// Specification để filter + sort + phân trang danh sách khách hàng.
/// Tìm kiếm theo: Tên, SĐT, Email
/// </summary>
public class CustomerFilterSpec : BaseSpecification<Customer>
{
    public CustomerFilterSpec(
        string? searchTerm,
        CustomerStatus? status,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool includeDeleted = false)
        : base(c =>
            (includeDeleted || !c.IsDeleted) &&
            (string.IsNullOrEmpty(searchTerm) ||
                c.FullName.ToLower().Contains(searchTerm.ToLower()) ||
                c.Phone.Contains(searchTerm) ||
                c.Email.ToLower().Contains(searchTerm.ToLower())) &&
            (!status.HasValue || c.Status == status.Value) &&
            (!fromDate.HasValue || c.CreatedAt >= fromDate.Value) &&
            (!toDate.HasValue || c.CreatedAt <= toDate.Value.AddDays(1)))
    {
        // Dynamic sorting
        var sortMappings = new Dictionary<string, Expression<Func<Customer, object>>>
        {
            ["fullname"]   = c => c.FullName,
            ["phone"]      = c => c.Phone,
            ["email"]      = c => c.Email,
            ["createdAt"]  = c => c.CreatedAt,
            ["totalspent"] = c => c.TotalSpent,
            ["debtamount"] = c => c.DebtAmount,
        };

        var sortKey = (sortBy ?? "createdAt").ToLower();
        var sortExpression = sortMappings.GetValueOrDefault(sortKey, sortMappings["createdAt"]);

        if (isDescending)
            AddOrderByDescending(sortExpression);
        else
            AddOrderBy(sortExpression);

        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}

/// <summary>
/// Chỉ đếm tổng số khách hàng (không paging) — để tính số trang
/// </summary>
public class CustomerFilterCountSpec : BaseSpecification<Customer>
{
    public CustomerFilterCountSpec(
        string? searchTerm,
        CustomerStatus? status,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool includeDeleted = false)
        : base(c =>
            (includeDeleted || !c.IsDeleted) &&
            (string.IsNullOrEmpty(searchTerm) ||
                c.FullName.ToLower().Contains(searchTerm.ToLower()) ||
                c.Phone.Contains(searchTerm) ||
                c.Email.ToLower().Contains(searchTerm.ToLower())) &&
            (!status.HasValue || c.Status == status.Value) &&
            (!fromDate.HasValue || c.CreatedAt >= fromDate.Value) &&
            (!toDate.HasValue || c.CreatedAt <= toDate.Value.AddDays(1)))
    {
    }
}

/// <summary>
/// Specification để lấy 1 khách hàng theo ID
/// </summary>
public class CustomerByIdSpec : BaseSpecification<Customer>
{
    public CustomerByIdSpec(Guid id, bool includeDeleted = false) 
        : base(c => c.Id == id && (includeDeleted || !c.IsDeleted))
    {
    }
}
