using ReportService.Utils.Enum;

namespace ReportService.Models.Request;

/// <summary>
/// Tham số filter + phân trang cho GET /api/reports/revenue
/// </summary>
public class RevenueReportFilterRequest
{
    private int _pageSize = 30;
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    /// <summary>Chiều thời gian: 0=Daily, 1=Monthly, 2=Yearly</summary>
    public ReportPeriod Period { get; set; } = ReportPeriod.Daily;

    /// <summary>Lọc nhãn thời gian (VD: "2025-01" cho Monthly)</summary>
    public string? PeriodLabel { get; set; }

    public string? SortBy { get; set; } = "periodlabel";
    public bool IsDescending { get; set; } = true;
}
