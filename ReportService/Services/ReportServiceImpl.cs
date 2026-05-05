using CommonService.Common;
using Microsoft.EntityFrameworkCore;
using ReportService.Data;
using ReportService.Models.Request;
using ReportService.Models.Response;
using ReportService.Repository.Interface;
using ReportService.Services.Interface;
using ReportService.Specifications;
using ReportService.Utils.Enum;

namespace ReportService.Services;

public class ReportServiceImpl : IReportService
{
    private readonly IRevenueReportRepository _revenueRepo;
    private readonly IStockSnapshotRepository _stockRepo;
    private readonly ReportDbContext _dbContext;

    public ReportServiceImpl(
        IRevenueReportRepository revenueRepo,
        IStockSnapshotRepository stockRepo,
        ReportDbContext dbContext)
    {
        _revenueRepo = revenueRepo;
        _stockRepo   = stockRepo;
        _dbContext   = dbContext;
    }

    public async Task<ResultPaginationDto<RevenueReportResponse>> GetPagedRevenueAsync(RevenueReportFilterRequest filter)
    {
        var spec = new RevenueReportFilterSpec(
            filter.Period, filter.PeriodLabel,
            filter.SortBy, filter.IsDescending,
            filter.PageNumber, filter.PageSize);

        var countSpec = new RevenueReportCountSpec(filter.Period, filter.PeriodLabel);

        var items      = await _revenueRepo.ListAsync(spec);
        var totalCount = await _revenueRepo.CountAsync(countSpec);

        return new ResultPaginationDto<RevenueReportResponse>(
            items.Select(MapToRevenueResponse).ToList(),
            filter.PageNumber, filter.PageSize, totalCount);
    }

    public async Task<ResultPaginationDto<StockSnapshotResponse>> GetPagedStockSnapshotsAsync(StockSnapshotFilterRequest filter)
    {
        var spec = new StockSnapshotFilterSpec(
            filter.SearchTerm, filter.LowStockOnly, filter.CategoryName,
            filter.SortBy, filter.IsDescending,
            filter.PageNumber, filter.PageSize);

        var countSpec = new StockSnapshotCountSpec(filter.SearchTerm, filter.LowStockOnly, filter.CategoryName);

        var items      = await _stockRepo.ListAsync(spec);
        var totalCount = await _stockRepo.CountAsync(countSpec);

        return new ResultPaginationDto<StockSnapshotResponse>(
            items.Select(MapToStockResponse).ToList(),
            filter.PageNumber, filter.PageSize, totalCount);
    }

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync()
    {
        var today       = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var thisMonth   = DateTime.UtcNow.ToString("yyyy-MM");

        var todayReport = await _dbContext.RevenueReports
            .FirstOrDefaultAsync(r => r.Period == ReportPeriod.Daily && r.PeriodLabel == today);

        var monthReport = await _dbContext.RevenueReports
            .FirstOrDefaultAsync(r => r.Period == ReportPeriod.Monthly && r.PeriodLabel == thisMonth);

        var lowStockCount = await _dbContext.ProductStockSnapshots
            .CountAsync(s => s.StockQuantity <= s.LowStockThreshold);

        var topSelling = await _dbContext.ProductStockSnapshots
            .OrderByDescending(s => s.SoldLast30Days)
            .Take(5)
            .ToListAsync();

        return new DashboardSummaryResponse
        {
            TodayRevenue       = todayReport?.TotalRevenue  ?? 0,
            ThisMonthRevenue   = monthReport?.TotalRevenue  ?? 0,
            TodayOrders        = todayReport?.TotalOrders   ?? 0,
            LowStockProductCount = lowStockCount,
            TopSellingProducts = topSelling.Select(MapToStockResponse).ToList(),
        };
    }

    private static RevenueReportResponse MapToRevenueResponse(Models.RevenueReport r) => new()
    {
        Id             = r.Id,
        Period         = r.Period.ToString(),
        PeriodLabel    = r.PeriodLabel,
        TotalRevenue   = r.TotalRevenue,
        TotalCost      = r.TotalCost,
        GrossProfit    = r.TotalRevenue - r.TotalCost,
        TotalOrders    = r.TotalOrders,
        TotalItemsSold = r.TotalItemsSold,
        LastUpdatedAt  = r.LastUpdatedAt,
    };

    private static StockSnapshotResponse MapToStockResponse(Models.ProductStockSnapshot s) => new()
    {
        Id                = s.Id,
        ProductId         = s.ProductId,
        ProductName       = s.ProductName,
        SKU               = s.SKU,
        CategoryName      = s.CategoryName,
        StockQuantity     = s.StockQuantity,
        LowStockThreshold = s.LowStockThreshold,
        IsLowStock        = s.StockQuantity <= s.LowStockThreshold,
        SoldLast30Days    = s.SoldLast30Days,
        LastSoldAt        = s.LastSoldAt,
        LastUpdatedAt     = s.LastUpdatedAt,
    };
}
