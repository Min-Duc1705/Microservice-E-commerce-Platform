using System.Linq.Expressions;
using CommonService.Specifications;
using ProductService.Models;
using ProductService.Utils.Enum;

namespace ProductService.Specifications;

/// <summary>
/// Specification để filter + sort + phân trang danh sách sản phẩm.
/// Tìm kiếm theo: Tên, Mã SKU
/// Lọc theo: Loại hàng hóa, Trạng thái
/// </summary>
public class ProductFilterSpec : BaseSpecification<Product>
{
    public ProductFilterSpec(
        string? searchTerm,
        Guid? categoryId,
        ProductStatus? status,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool includeDeleted = false)
        : base(p =>
            (includeDeleted || !p.IsDeleted) &&
            (string.IsNullOrEmpty(searchTerm) ||
                p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                p.SKU.ToLower().Contains(searchTerm.ToLower())) &&
            (!categoryId.HasValue || p.CategoryId == categoryId.Value) &&
            (!status.HasValue || p.Status == status.Value) &&
            (!fromDate.HasValue || p.CreatedAt >= fromDate.Value) &&
            (!toDate.HasValue || p.CreatedAt <= toDate.Value.AddDays(1)))
    {
        // Include Category để lấy tên danh mục
        AddInclude(p => p.Category!);

        // Dynamic sorting
        var sortMappings = new Dictionary<string, Expression<Func<Product, object>>>
        {
            ["name"] = p => p.Name,
            ["sku"] = p => p.SKU,
            ["price"] = p => p.Price,
            ["costprice"] = p => p.CostPrice,
            ["stockquantity"] = p => p.StockQuantity,
            ["status"] = p => p.Status,
            ["createdat"] = p => p.CreatedAt,
        };

        var sortKey = (sortBy ?? "createdAt").ToLower();
        var sortExpression = sortMappings.GetValueOrDefault(sortKey, sortMappings["createdat"]);

        if (isDescending)
            AddOrderByDescending(sortExpression);
        else
            AddOrderBy(sortExpression);

        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}

/// <summary>
/// Chỉ đếm tổng số sản phẩm (không paging) — để tính số trang
/// </summary>
public class ProductFilterCountSpec : BaseSpecification<Product>
{
    public ProductFilterCountSpec(
        string? searchTerm,
        Guid? categoryId,
        ProductStatus? status,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool includeDeleted = false)
        : base(p =>
            (includeDeleted || !p.IsDeleted) &&
            (string.IsNullOrEmpty(searchTerm) ||
                p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                p.SKU.ToLower().Contains(searchTerm.ToLower())) &&
            (!categoryId.HasValue || p.CategoryId == categoryId.Value) &&
            (!status.HasValue || p.Status == status.Value) &&
            (!fromDate.HasValue || p.CreatedAt >= fromDate.Value) &&
            (!toDate.HasValue || p.CreatedAt <= toDate.Value.AddDays(1)))
    {
    }
}

/// <summary>
/// Specification để lấy 1 sản phẩm theo ID (kèm thông tin danh mục)
/// </summary>
public class ProductByIdSpec : BaseSpecification<Product>
{
    public ProductByIdSpec(Guid id, bool includeDeleted = false) 
        : base(p => p.Id == id && (includeDeleted || !p.IsDeleted))
    {
        AddInclude(p => p.Category!);
    }
}

/// <summary>
/// Specification để kiểm tra SKU trùng
/// </summary>
public class ProductBySkuSpec : BaseSpecification<Product>
{
    public ProductBySkuSpec(string sku) : base(p => p.SKU.ToLower() == sku.ToLower())
    {
    }
}
