using System.Linq.Expressions;
using CommonService.Specifications;
using ReportService.Models;
using ReportService.Utils.Enum;

namespace ReportService.Specifications;

/// <summary>Filter + sort + phân trang báo cáo doanh thu.</summary>
public class RevenueReportFilterSpec : BaseSpecification<RevenueReport>
{
    public RevenueReportFilterSpec(
        ReportPeriod period,
        string? periodLabel,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize)
        : base(r =>
            r.Period == period &&
            (string.IsNullOrEmpty(periodLabel) || r.PeriodLabel.Contains(periodLabel)))
    {
        var sortMappings = new Dictionary<string, Expression<Func<RevenueReport, object>>>
        {
            ["periodlabel"]    = r => r.PeriodLabel,
            ["totalrevenue"]   = r => r.TotalRevenue,
            ["totalcost"]      = r => r.TotalCost,
            ["totalorders"]    = r => r.TotalOrders,
            ["totalitemssold"] = r => r.TotalItemsSold,
            ["lastupdatedat"]  = r => r.LastUpdatedAt,
        };

        var sortKey  = (sortBy ?? "periodlabel").ToLower();
        var sortExpr = sortMappings.GetValueOrDefault(sortKey, sortMappings["periodlabel"]);

        if (isDescending) AddOrderByDescending(sortExpr);
        else              AddOrderBy(sortExpr);

        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}

/// <summary>Chỉ đếm — không paging. Dùng để tính tổng số trang.</summary>
public class RevenueReportCountSpec : BaseSpecification<RevenueReport>
{
    public RevenueReportCountSpec(ReportPeriod period, string? periodLabel)
        : base(r =>
            r.Period == period &&
            (string.IsNullOrEmpty(periodLabel) || r.PeriodLabel.Contains(periodLabel)))
    { }
}

// ======================================================
// Stock Snapshot Specifications
// ======================================================

/// <summary>Filter + sort + phân trang snapshot tồn kho.</summary>
public class StockSnapshotFilterSpec : BaseSpecification<ProductStockSnapshot>
{
    public StockSnapshotFilterSpec(
        string? searchTerm,
        bool? lowStockOnly,
        string? categoryName,
        string? sortBy,
        bool isDescending,
        int pageNumber,
        int pageSize)
        : base(s =>
            (string.IsNullOrEmpty(searchTerm) ||
                s.ProductName.ToLower().Contains(searchTerm.ToLower()) ||
                s.SKU.ToLower().Contains(searchTerm.ToLower())) &&
            (!lowStockOnly.HasValue || !lowStockOnly.Value || s.StockQuantity <= s.LowStockThreshold) &&
            (string.IsNullOrEmpty(categoryName) || s.CategoryName.ToLower().Contains(categoryName.ToLower())))
    {
        var sortMappings = new Dictionary<string, Expression<Func<ProductStockSnapshot, object>>>
        {
            ["productname"]     = s => s.ProductName,
            ["stockquantity"]   = s => s.StockQuantity,
            ["soldlast30days"]  = s => s.SoldLast30Days,
            ["lastsoldat"]      = s => s.LastSoldAt!,
            ["lastupdatedat"]   = s => s.LastUpdatedAt,
        };

        var sortKey  = (sortBy ?? "soldlast30days").ToLower();
        var sortExpr = sortMappings.GetValueOrDefault(sortKey, sortMappings["soldlast30days"]);

        if (isDescending) AddOrderByDescending(sortExpr);
        else              AddOrderBy(sortExpr);

        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}

/// <summary>Chỉ đếm — không paging.</summary>
public class StockSnapshotCountSpec : BaseSpecification<ProductStockSnapshot>
{
    public StockSnapshotCountSpec(string? searchTerm, bool? lowStockOnly, string? categoryName)
        : base(s =>
            (string.IsNullOrEmpty(searchTerm) ||
                s.ProductName.ToLower().Contains(searchTerm.ToLower()) ||
                s.SKU.ToLower().Contains(searchTerm.ToLower())) &&
            (!lowStockOnly.HasValue || !lowStockOnly.Value || s.StockQuantity <= s.LowStockThreshold) &&
            (string.IsNullOrEmpty(categoryName) || s.CategoryName.ToLower().Contains(categoryName.ToLower())))
    { }
}
