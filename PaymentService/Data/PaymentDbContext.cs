using Microsoft.EntityFrameworkCore;
using PaymentService.Models;

namespace PaymentService.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Indexes để query nhanh theo OrderId và Status
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Method);
            entity.HasIndex(e => e.GatewayTransactionId);
            entity.HasIndex(e => e.PaidAt);

            // Composite index để tìm giao dịch theo đơn hàng + trạng thái
            entity.HasIndex(e => new { e.OrderId, e.Status });
            entity.HasIndex(e => new { e.Status, e.PaidAt });

            // Độ chính xác decimal cho tiền tệ
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.RefundedAmount).HasPrecision(18, 2);

            // Enum → smallint để tiết kiệm storage
            entity.Property(e => e.Method).HasConversion<short>();
            entity.Property(e => e.Status).HasConversion<short>();
        });
    }
}
