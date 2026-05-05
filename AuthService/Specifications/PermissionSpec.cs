using System.Linq.Expressions;
using AuthService.Models;
using CommonService.Specifications;

namespace AuthService.Specifications;

public class PermissionFilterSpec : BaseSpecification<Permission>
{
    public PermissionFilterSpec(
        string? searchTerm,
        string? module,
        string? method,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize)
        : base(p =>
            (string.IsNullOrEmpty(searchTerm) ||
                p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                p.ApiPath.ToLower().Contains(searchTerm.ToLower())) &&
            (string.IsNullOrEmpty(module) || p.Module.ToLower() == module.ToLower()) &&
            (string.IsNullOrEmpty(method) || p.Method.ToUpper() == method.ToUpper()))
    {
        var sortMappings = new Dictionary<string, Expression<Func<Permission, object>>>
        {
            ["name"]   = p => p.Name,
            ["module"] = p => p.Module,
            ["method"] = p => p.Method,
        };

        var sortKey  = (sortBy ?? "module").ToLower();
        var sortExpr = sortMappings.GetValueOrDefault(sortKey, sortMappings["module"]);

        if (isDescending) AddOrderByDescending(sortExpr);
        else              AddOrderBy(sortExpr);

        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}

public class PermissionFilterCountSpec : BaseSpecification<Permission>
{
    public PermissionFilterCountSpec(string? searchTerm, string? module, string? method)
        : base(p =>
            (string.IsNullOrEmpty(searchTerm) ||
                p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                p.ApiPath.ToLower().Contains(searchTerm.ToLower())) &&
            (string.IsNullOrEmpty(module) || p.Module.ToLower() == module.ToLower()) &&
            (string.IsNullOrEmpty(method) || p.Method.ToUpper() == method.ToUpper()))
    { }
}

public class PermissionByIdSpec : BaseSpecification<Permission>
{
    public PermissionByIdSpec(Guid id) : base(p => p.Id == id) { }
}
