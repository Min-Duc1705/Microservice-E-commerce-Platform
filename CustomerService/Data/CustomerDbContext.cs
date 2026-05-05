using CustomerService.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Data;

public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<DebtTransaction> DebtTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalSpent).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DebtAmount).HasColumnType("decimal(18,2)");

            // Global query filter: tự động loại bỏ các bản ghi đã soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);

            // ===== INDEXES =====
            entity.HasIndex(e => e.Phone)
                  .HasDatabaseName("IX_Customers_Phone");

            entity.HasIndex(e => e.Email)
                  .HasDatabaseName("IX_Customers_Email");

            entity.HasIndex(e => e.FullName)
                  .HasDatabaseName("IX_Customers_FullName");

            entity.HasIndex(e => e.Status)
                  .HasDatabaseName("IX_Customers_Status");

            entity.HasIndex(e => e.CreatedAt)
                  .HasDatabaseName("IX_Customers_CreatedAt");

            entity.HasIndex(e => e.IsDeleted)
                  .HasDatabaseName("IX_Customers_IsDeleted");

            // Composite: Filter thường gặp nhất (Status + CreatedAt)
            entity.HasIndex(e => new { e.Status, e.CreatedAt })
                  .HasDatabaseName("IX_Customers_Status_CreatedAt");
        });

        // ===== DEBT TRANSACTION =====
        modelBuilder.Entity<DebtTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");

            // FK → Customer (Xóa Customer không xóa lịch sử giao dịch)
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.DebtTransactions)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // ===== INDEXES =====
            // Index chính: lấy lịch sử của một khách hàng, sắp xếp theo thời gian
            entity.HasIndex(e => new { e.CustomerId, e.CreatedAt })
                  .HasDatabaseName("IX_DebtTransactions_CustomerId_CreatedAt");

            // Index hỗ trợ query: Lọc theo loại giao dịch
            entity.HasIndex(e => e.Type)
                  .HasDatabaseName("IX_DebtTransactions_Type");

            // Index hỗ trợ: Tìm giao dịch gốc từ đơn hàng
            entity.HasIndex(e => e.ReferenceOrderId)
                  .HasDatabaseName("IX_DebtTransactions_ReferenceOrderId");
        });
    }
}
