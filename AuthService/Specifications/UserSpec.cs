using System.Linq.Expressions;
using AuthService.Models;
using CommonService.Specifications;

namespace AuthService.Specifications;

/// <summary>
/// Lấy danh sách user với filter, sort, phân trang.
/// Include Role để có thể hiển thị tên role trong danh sách.
/// </summary>
public class UserFilterSpec : BaseSpecification<AppUser>
{
    public UserFilterSpec(
        string? searchTerm,
        bool? isActive,
        Guid? roleId,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize)
        : base(u =>
            (string.IsNullOrEmpty(searchTerm) ||
                u.Username.ToLower().Contains(searchTerm.ToLower()) ||
                u.Email.ToLower().Contains(searchTerm.ToLower())) &&
            (!isActive.HasValue || u.IsActive == isActive.Value) &&
            (!roleId.HasValue || u.RoleId == roleId.Value))
    {
        AddInclude(u => u.Role!);

        var sortMappings = new Dictionary<string, Expression<Func<AppUser, object>>>
        {
            ["username"] = u => u.Username,
            ["email"] = u => u.Email,
            ["createdat"] = u => u.CreatedAt,
        };

        var sortKey = (sortBy ?? "createdat").ToLower();
        var sortExpr = sortMappings.GetValueOrDefault(sortKey, sortMappings["createdat"]);

        if (isDescending) AddOrderByDescending(sortExpr);
        else AddOrderBy(sortExpr);

        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}

/// <summary>
/// Đếm tổng số user (không paging) — để tính tổng trang.
/// </summary>
public class UserFilterCountSpec : BaseSpecification<AppUser>
{
    public UserFilterCountSpec(string? searchTerm, bool? isActive, Guid? roleId)
        : base(u =>
            (string.IsNullOrEmpty(searchTerm) ||
                u.Username.ToLower().Contains(searchTerm.ToLower()) ||
                u.Email.ToLower().Contains(searchTerm.ToLower())) &&
            (!isActive.HasValue || u.IsActive == isActive.Value) &&
            (!roleId.HasValue || u.RoleId == roleId.Value))
    { }
}

/// <summary>
/// Lấy 1 user theo ID — include Role (dùng cho GetById, Update, Delete).
/// </summary>
public class UserByIdSpec : BaseSpecification<AppUser>
{
    public UserByIdSpec(Guid id) : base(u => u.Id == id)
    {
        AddInclude(u => u.Role!);
    }
}
