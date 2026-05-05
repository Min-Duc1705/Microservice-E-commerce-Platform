using CommonService.Repository;
using Microsoft.EntityFrameworkCore;
using ReportService.Data;
using ReportService.Models;
using ReportService.Repository.Interface;
using ReportService.Utils.Enum;

namespace ReportService.Repository;

public class RevenueReportRepository : GenericRepository<ReportDbContext, RevenueReport>, IRevenueReportRepository
{
    public RevenueReportRepository(ReportDbContext context) : base(context) { }

    public async Task<RevenueReport?> GetByPeriodLabelAsync(string periodLabel, ReportPeriod period)
    {
        return await _context.RevenueReports
            .FirstOrDefaultAsync(r => r.Period == period && r.PeriodLabel == periodLabel);
    }
}

public class StockSnapshotRepository : GenericRepository<ReportDbContext, ProductStockSnapshot>, IStockSnapshotRepository
{
    public StockSnapshotRepository(ReportDbContext context) : base(context) { }

    public async Task<ProductStockSnapshot?> GetByProductIdAsync(Guid productId)
    {
        return await _context.ProductStockSnapshots
            .FirstOrDefaultAsync(s => s.ProductId == productId);
    }
}
