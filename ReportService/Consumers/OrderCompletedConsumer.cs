using MassTransit;
using ReportService.Data;
using ReportService.Models;
using ReportService.Repository.Interface;
using ReportService.Utils.Enum;
using CommonService.Events;

namespace ReportService.Consumers;

/// <summary>
/// Hứng OrderCompletedEvent từ OrderService mỗi khi Admin hoàn thành đơn hàng.
/// Cộng dồn vào báo cáo doanh thu theo ngày, tháng, năm.
/// </summary>
public class OrderCompletedConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly IRevenueReportRepository _revenueRepo;

    public OrderCompletedConsumer(IRevenueReportRepository revenueRepo)
    {
        _revenueRepo = revenueRepo;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var msg     = context.Message;
        var dateNow = msg.CompletedAt.ToUniversalTime();

        // Tính tổng doanh thu, chi phí, số lượng từ các item trong event
        decimal totalRevenue = 0;
        decimal totalCost    = 0;
        int     itemsSold    = 0;

        foreach (var item in msg.Items)
        {
            totalRevenue += item.Quantity * item.UnitPrice;
            totalCost    += item.Quantity * item.UnitCost;
            itemsSold    += item.Quantity;
        }

        // Cập nhật (hoặc tạo mới) báo cáo NGÀY, THÁNG, NĂM cùng lúc
        await UpsertReportAsync(ReportPeriod.Daily,   dateNow.ToString("yyyy-MM-dd"), totalRevenue, totalCost, itemsSold);
        await UpsertReportAsync(ReportPeriod.Monthly, dateNow.ToString("yyyy-MM"),   totalRevenue, totalCost, itemsSold);
        await UpsertReportAsync(ReportPeriod.Yearly,  dateNow.ToString("yyyy"),       totalRevenue, totalCost, itemsSold);
    }

    private async Task UpsertReportAsync(
        ReportPeriod period, string label,
        decimal revenue, decimal cost, int itemsSold)
    {
        var report = await _revenueRepo.GetByPeriodLabelAsync(label, period);

        if (report is null)
        {
            // Ngày/Tháng/Năm đầu tiên có dữ liệu → Tạo mới
            report = new RevenueReport
            {
                Id             = Guid.NewGuid(),
                Period         = period,
                PeriodLabel    = label,
                TotalRevenue   = revenue,
                TotalCost      = cost,
                TotalOrders    = 1,
                TotalItemsSold = itemsSold,
                LastUpdatedAt  = DateTime.UtcNow,
            };
            await _revenueRepo.AddAsync(report);
        }
        else
        {
            // Cộng dồn vào bản ghi đã tồn tại
            report.TotalRevenue   += revenue;
            report.TotalCost      += cost;
            report.TotalOrders    += 1;
            report.TotalItemsSold += itemsSold;
            report.LastUpdatedAt   = DateTime.UtcNow;
            _revenueRepo.Update(report);
        }

        await _revenueRepo.SaveChangesAsync();
    }
}
