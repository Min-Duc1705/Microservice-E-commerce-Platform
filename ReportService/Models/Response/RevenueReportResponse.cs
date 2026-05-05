using ReportService.Utils.Enum;

namespace ReportService.Models.Response;

public class RevenueReportResponse
{
    public Guid Id { get; set; }
    public string Period { get; set; } = string.Empty;
    public string PeriodLabel { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal GrossProfit { get; set; }
    public int TotalOrders { get; set; }
    public int TotalItemsSold { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
