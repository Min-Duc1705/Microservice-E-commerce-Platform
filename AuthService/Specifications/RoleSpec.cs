using System.Linq.Expressions;
using AuthService.Models;
using CommonService.Specifications;

namespace AuthService.Specifications;

public class RoleFilterSpec : BaseSpecification<Role>
{
    public RoleFilterSpec(
        string? searchTerm,
        bool? isActive,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize)
        : base(r =>
            (string.IsNullOrEmpty(searchTerm) ||
                r.Name.ToLower().Contains(searchTerm.ToLower()) ||
                (r.Description != null && r.Description.ToLower().Contains(searchTerm.ToLower()))) &&
            (!isActive.HasValue || r.IsActive == isActive.Value))
    {
        var sortMappings = new Dictionary<string, Expression<Func<Role, object>>>
        {
            ["name"] = r => r.Name,
            ["createdat"] = r => r.CreatedAt,
        };

        var sortKey = (sortBy ?? "name").ToLower();
        var sortExpr = sortMappings.GetValueOrDefault(sortKey, sortMappings["name"]);

        if (isDescending) AddOrderByDescending(sortExpr);
        else AddOrderBy(sortExpr);

        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        AddInclude(r => r.Permissions);
    }
}

public class RoleFilterCountSpec : BaseSpecification<Role>
{
    public RoleFilterCountSpec(string? searchTerm, bool? isActive)
        : base(r =>
            (string.IsNullOrEmpty(searchTerm) ||
                r.Name.ToLower().Contains(searchTerm.ToLower()) ||
                (r.Description != null && r.Description.ToLower().Contains(searchTerm.ToLower()))) &&
            (!isActive.HasValue || r.IsActive == isActive.Value))
    { }
}

public class RoleByIdSpec : BaseSpecification<Role>
{
    public RoleByIdSpec(Guid id) : base(r => r.Id == id)
    {
        AddInclude(r => r.Permissions);
        AddInclude(r => r.Users);
    }
}
