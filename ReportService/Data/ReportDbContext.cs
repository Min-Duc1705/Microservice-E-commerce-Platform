using Microsoft.EntityFrameworkCore;
using ReportService.Models;
using ReportService.Utils.Enum;

namespace ReportService.Data;

public class ReportDbContext : DbContext
{
    public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options) { }

    public DbSet<RevenueReport>        RevenueReports        => Set<RevenueReport>();
    public DbSet<ProductStockSnapshot> ProductStockSnapshots => Set<ProductStockSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── RevenueReport ──────────────────────────────────
        modelBuilder.Entity<RevenueReport>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Unique: (Period, PeriodLabel) — tránh tạo 2 dòng cùng kỳ
            entity.HasIndex(e => new { e.Period, e.PeriodLabel }).IsUnique();
            entity.HasIndex(e => e.Period);

            entity.Property(e => e.TotalRevenue).HasPrecision(18, 2);
            entity.Property(e => e.TotalCost).HasPrecision(18, 2);
            entity.Property(e => e.Period).HasConversion<short>();

            // GrossProfit là computed property → không map vào DB
            entity.Ignore(e => e.GrossProfit);
        });

        // ── ProductStockSnapshot ───────────────────────────
        modelBuilder.Entity<ProductStockSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Unique theo ProductId — mỗi sản phẩm chỉ có 1 snapshot
            entity.HasIndex(e => e.ProductId).IsUnique();
            entity.HasIndex(e => e.StockQuantity);
            entity.HasIndex(e => e.SoldLast30Days);
            entity.HasIndex(e => e.LastSoldAt);

            // Composite để query hàng sắp hết: StockQuantity <= LowStockThreshold
            entity.HasIndex(e => new { e.StockQuantity, e.LowStockThreshold });

            // IsLowStock là computed property → không map vào DB
            entity.Ignore(e => e.IsLowStock);
        });
    }
}
