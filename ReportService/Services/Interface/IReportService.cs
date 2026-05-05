using CommonService.Common;
using ReportService.Models.Request;
using ReportService.Models.Response;

namespace ReportService.Services.Interface;

public interface IReportService
{
    /// <summary>Lấy danh sách báo cáo doanh thu theo kỳ (phân trang)</summary>
    Task<ResultPaginationDto<RevenueReportResponse>> GetPagedRevenueAsync(RevenueReportFilterRequest filter);

    /// <summary>Lấy danh sách snapshot tồn kho (phân trang)</summary>
    Task<ResultPaginationDto<StockSnapshotResponse>> GetPagedStockSnapshotsAsync(StockSnapshotFilterRequest filter);

    /// <summary>Tổng quan nhanh cho Dashboard (top-level KPI numbers)</summary>
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync();
}
