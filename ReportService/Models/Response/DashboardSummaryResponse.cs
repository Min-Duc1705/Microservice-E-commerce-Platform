namespace ReportService.Models.Response;

/// <summary>Các chỉ số KPI tổng quan cho Dashboard Admin.</summary>
public class DashboardSummaryResponse
{
    /// <summary>Tổng doanh thu hôm nay</summary>
    public decimal TodayRevenue { get; set; }

    /// <summary>Tổng doanh thu tháng này</summary>
    public decimal ThisMonthRevenue { get; set; }

    /// <summary>Số đơn hàng hôm nay</summary>
    public int TodayOrders { get; set; }

    /// <summary>Số sản phẩm sắp hết hàng</summary>
    public int LowStockProductCount { get; set; }

    /// <summary>Top 5 sản phẩm bán chạy nhất 30 ngày qua</summary>
    public List<StockSnapshotResponse> TopSellingProducts { get; set; } = new();
}
