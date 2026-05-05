using CommonService.Annotations;
using Microsoft.AspNetCore.Mvc;
using CommonService.Filters;
using ReportService.Models.Request;
using ReportService.Services.Interface;

namespace ReportService.Controllers;

[ApiController]
[Route("api/v1/reports")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    [RequiresPermission("GET", "/api/v1/reports")]
    [ApiMessage("Report Service is running")]
    public IActionResult Index() => Ok(new { Service = "ReportService", Status = "Running" });

    /// <summary>Dashboard KPI — doanh thu hôm nay, tháng này, top bán chạy, hàng sắp hết</summary>
    [HttpGet("dashboard")]
    [RequiresPermission("GET", "/api/v1/reports/dashboard")]
    [ApiMessage("Lấy tổng quan dashboard thành công")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _reportService.GetDashboardSummaryAsync();
        return Ok(result);
    }

    /// <summary>Danh sách báo cáo doanh thu theo kỳ (Daily / Monthly / Yearly)</summary>
    [HttpGet("revenue")]
    [RequiresPermission("GET", "/api/v1/reports/revenue")]
    [ApiMessage("Lấy báo cáo doanh thu thành công")]
    public async Task<IActionResult> GetRevenue([FromQuery] RevenueReportFilterRequest filter)
    {
        var result = await _reportService.GetPagedRevenueAsync(filter);
        return Ok(result);
    }

    /// <summary>Danh sách snapshot tồn kho sản phẩm</summary>
    [HttpGet("stock-snapshots")]
    [RequiresPermission("GET", "/api/v1/reports/stock-snapshots")]
    [ApiMessage("Lấy snapshot tồn kho thành công")]
    public async Task<IActionResult> GetStockSnapshots([FromQuery] StockSnapshotFilterRequest filter)
    {
        var result = await _reportService.GetPagedStockSnapshotsAsync(filter);
        return Ok(result);
    }

    /// <summary>Hàng sắp hết kho (shortcut filter LowStockOnly=true)</summary>
    [HttpGet("stock-snapshots/low-stock")]
    [RequiresPermission("GET", "/api/v1/reports/stock-snapshots/low-stock")]
    [ApiMessage("Lấy danh sách hàng sắp hết thành công")]
    public async Task<IActionResult> GetLowStock([FromQuery] int pageSize = 20)
    {
        var result = await _reportService.GetPagedStockSnapshotsAsync(
            new StockSnapshotFilterRequest { LowStockOnly = true, PageSize = pageSize, SortBy = "stockquantity", IsDescending = false });
        return Ok(result);
    }

    /// <summary>Top sản phẩm bán chạy nhất 30 ngày (shortcut sort SoldLast30Days DESC)</summary>
    [HttpGet("stock-snapshots/top-selling")]
    [RequiresPermission("GET", "/api/v1/reports/stock-snapshots/top-selling")]
    [ApiMessage("Lấy top sản phẩm bán chạy thành công")]
    public async Task<IActionResult> GetTopSelling([FromQuery] int pageSize = 10)
    {
        var result = await _reportService.GetPagedStockSnapshotsAsync(
            new StockSnapshotFilterRequest { PageSize = pageSize, SortBy = "soldlast30days", IsDescending = true });
        return Ok(result);
    }
}
