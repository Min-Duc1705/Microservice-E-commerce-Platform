using CommonService.Repository;
using ReportService.Models;

namespace ReportService.Repository.Interface;

/// <summary>Repository cho RevenueReport — kế thừa IGenericRepository.</summary>
public interface IRevenueReportRepository : IGenericRepository<RevenueReport>
{
    /// <summary>Tìm chính xác 1 dòng báo cáo theo Period + PeriodLabel (để UPSERT).</summary>
    Task<RevenueReport?> GetByPeriodLabelAsync(string periodLabel, ReportService.Utils.Enum.ReportPeriod period);
}

/// <summary>Repository cho ProductStockSnapshot — kế thừa IGenericRepository.</summary>
public interface IStockSnapshotRepository : IGenericRepository<ProductStockSnapshot>
{
    /// <summary>Tìm snapshot theo ProductId (luôn unique).</summary>
    Task<ProductStockSnapshot?> GetByProductIdAsync(Guid productId);
}
