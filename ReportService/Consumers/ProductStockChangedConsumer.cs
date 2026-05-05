using MassTransit;
using Microsoft.EntityFrameworkCore;
using ReportService.Data;
using ReportService.Models;
using CommonService.Events;

namespace ReportService.Consumers;

/// <summary>
/// Hứng ProductStockChangedEvent từ ProductService mỗi khi tồn kho thay đổi.
/// UPSERT vào bảng ProductStockSnapshots.
/// </summary>
public class ProductStockChangedConsumer : IConsumer<ProductStockChangedEvent>
{
    private readonly ReportDbContext _dbContext;

    public ProductStockChangedConsumer(ReportDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<ProductStockChangedEvent> context)
    {
        var msg = context.Message;

        var snapshot = await _dbContext.ProductStockSnapshots
            .FirstOrDefaultAsync(s => s.ProductId == msg.ProductId);

        if (snapshot is null)
        {
            // Sản phẩm mới → tạo snapshot lần đầu
            snapshot = new ProductStockSnapshot
            {
                Id                = Guid.NewGuid(),
                ProductId         = msg.ProductId,
                ProductName       = msg.ProductName,
                SKU               = msg.SKU,
                CategoryName      = msg.CategoryName,
                StockQuantity     = msg.StockQuantity,
                LowStockThreshold = msg.LowStockThreshold,
                SoldLast30Days    = msg.SoldLast30Days,
                LastSoldAt        = msg.LastSoldAt,
                LastUpdatedAt     = DateTime.UtcNow,
            };
            _dbContext.ProductStockSnapshots.Add(snapshot);
        }
        else
        {
            // Cập nhật snapshot hiện tại
            snapshot.ProductName       = msg.ProductName;
            snapshot.SKU               = msg.SKU;
            snapshot.CategoryName      = msg.CategoryName;
            snapshot.StockQuantity     = msg.StockQuantity;
            snapshot.LowStockThreshold = msg.LowStockThreshold;
            snapshot.SoldLast30Days    = msg.SoldLast30Days;
            snapshot.LastSoldAt        = msg.LastSoldAt;
            snapshot.LastUpdatedAt     = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
    }
}
