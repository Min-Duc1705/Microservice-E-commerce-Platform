using CommonService.Specifications;
using ProductService.Models;

namespace ProductService.Specifications;

/// <summary>
/// Specification để lấy 1 danh mục theo ID
/// </summary>
public class CategoryByIdSpec : BaseSpecification<Category>
{
    public CategoryByIdSpec(Guid id, bool includeDeleted = false) 
        : base(c => c.Id == id && (includeDeleted || !c.IsDeleted))
    {
        AddInclude(c => c.Products);
    }
}

/// <summary>
/// Specification để kiểm tra tên danh mục đã tồn tại chưa
/// </summary>
public class CategoryByNameSpec : BaseSpecification<Category>
{
    public CategoryByNameSpec(string name) : base(c => c.Name.ToLower() == name.ToLower())
    {
    }
}

/// <summary>
/// Lấy tất cả danh mục (không phân trang — danh sách thường nhỏ)
/// </summary>
public class AllCategoriesSpec : BaseSpecification<Category>
{
    public AllCategoriesSpec(bool includeDeleted = false) 
        : base(c => includeDeleted || !c.IsDeleted)
    {
        AddInclude(c => c.Products);
        AddOrderBy(c => c.Name);
    }
}

/// <summary>
/// Specification để filter + sort + phân trang danh sách danh mục.
/// Tìm kiếm theo: Tên mã, Mô tả
/// </summary>
public class CategoryFilterSpec : BaseSpecification<Category>
{
    public CategoryFilterSpec(
        string? searchTerm,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize,
        bool includeDeleted = false)
        : base(c =>
            (includeDeleted || !c.IsDeleted) &&
            (string.IsNullOrEmpty(searchTerm) ||
                c.Name.ToLower().Contains(searchTerm.ToLower()) ||
                c.Description.ToLower().Contains(searchTerm.ToLower())))
    {
        // Dynamic sorting
        var sortMappings = new System.Collections.Generic.Dictionary<string, System.Linq.Expressions.Expression<System.Func<Category, object>>>
        {
            ["name"] = c => c.Name,
            ["createdat"] = c => c.CreatedAt,
            ["updatedat"] = c => c.UpdatedAt!,
            ["productcount"] = c => c.Products.Count(p => !p.IsDeleted)
        };

        var sortKey = (sortBy ?? "createdat").ToLower();
        var sortExpression = sortMappings.GetValueOrDefault(sortKey, sortMappings["createdat"]);

        if (isDescending)
            AddOrderByDescending(sortExpression);
        else
            AddOrderBy(sortExpression);

        AddInclude(c => c.Products);
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}

/// <summary>
/// Chỉ đếm tổng số danh mục (không paging) — để tính số trang
/// </summary>
public class CategoryFilterCountSpec : BaseSpecification<Category>
{
    public CategoryFilterCountSpec(
        string? searchTerm,
        bool includeDeleted = false)
        : base(c =>
            (includeDeleted || !c.IsDeleted) &&
            (string.IsNullOrEmpty(searchTerm) ||
                c.Name.ToLower().Contains(searchTerm.ToLower()) ||
                c.Description.ToLower().Contains(searchTerm.ToLower())))
    {
    }
}

/// <summary>
/// Lấy danh mục đang hoạt động cho dropdown (không phân trang, sắp xếp theo tên)
/// </summary>
public class CategoryDropdownSpec : BaseSpecification<Category>
{
    public CategoryDropdownSpec() 
        : base(c => !c.IsDeleted)
    {
        AddOrderBy(c => c.Name);
    }
}
