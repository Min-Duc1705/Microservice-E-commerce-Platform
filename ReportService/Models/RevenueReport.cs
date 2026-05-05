using ReportService.Utils.Enum;

namespace ReportService.Models;

/// <summary>
/// Báo cáo doanh thu đã tổng hợp theo kỳ — bảng RevenueReports.
/// Pre-aggregated bởi OrderCompletedConsumer. Read-only từ phía API.
/// Theo DB Analysis: 02.Database_Analysis.md Section 6.
/// </summary>
public class RevenueReport
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Chiều thời gian: Daily / Monthly / Yearly</summary>
    public ReportPeriod Period { get; set; }

    /// <summary>Nhãn xác định kỳ: "2025-01-15" (Daily), "2025-01" (Monthly), "2025" (Yearly)</summary>
    public string PeriodLabel { get; set; } = string.Empty;

    // ---------- Số liệu tổng hợp ----------
    public decimal TotalRevenue   { get; set; }
    public decimal TotalCost      { get; set; }

    /// <summary>Lợi nhuận gộp — computed, không lưu vào DB</summary>
    public decimal GrossProfit => TotalRevenue - TotalCost;

    public int TotalOrders    { get; set; }
    public int TotalItemsSold { get; set; }

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
