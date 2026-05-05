using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data;

public class ProductDbContext : DbContext
{
      public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

      public DbSet<Product> Products { get; set; }
      public DbSet<Category> Categories { get; set; }
      public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
            base.OnModelCreating(modelBuilder);

            // ===== CATEGORY =====
            modelBuilder.Entity<Category>(entity =>
            {
                  entity.HasKey(e => e.Id);

                  entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_Categories_Name");

                  entity.HasIndex(e => e.IsDeleted)
                    .HasDatabaseName("IX_Categories_IsDeleted");
                  // Không dùng HasQueryFilter — Specification đảm nhận filter includeDeleted
            });

            // ===== PRODUCT =====
            modelBuilder.Entity<Product>(entity =>
            {
                  entity.HasKey(e => e.Id);

                  entity.Property(e => e.CostPrice).HasColumnType("decimal(18,2)");
                  entity.Property(e => e.Price).HasColumnType("decimal(18,2)");

                  // Lưu List<string> dưới dạng JSON trong cột text
                  entity.Property(e => e.ImageUrls)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>(),
                        new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                            (c1, c2) => c1!.SequenceEqual(c2!),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToList()));

                  // Không dùng HasQueryFilter — ProductFilterSpec đảm nhận filter includeDeleted
                  // Admin gửi includeDeleted=true → thấy tất, User mặc định false → chỉ thấy active

                  // Quan hệ Product → Category
                  entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                  // ===== INDEXES =====
                  entity.HasIndex(e => e.SKU)
                    .IsUnique()
                    .HasDatabaseName("IX_Products_SKU");

                  entity.HasIndex(e => e.Name)
                    .HasDatabaseName("IX_Products_Name");

                  entity.HasIndex(e => e.CategoryId)
                    .HasDatabaseName("IX_Products_CategoryId");

                  entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_Products_Status");

                  entity.HasIndex(e => e.StockQuantity)
                    .HasDatabaseName("IX_Products_StockQuantity");

                  entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("IX_Products_CreatedAt");

                  entity.HasIndex(e => e.IsDeleted)
                    .HasDatabaseName("IX_Products_IsDeleted");

                  // Composite: filter phổ biến nhất (Status + CategoryId)
                  entity.HasIndex(e => new { e.Status, e.CategoryId })
                    .HasDatabaseName("IX_Products_Status_CategoryId");

                  // Composite: Báo cáo tồn kho (IsDeleted + StockQuantity)
                  entity.HasIndex(e => new { e.IsDeleted, e.StockQuantity })
                    .HasDatabaseName("IX_Products_IsDeleted_StockQuantity");
            });

            // ===== INVENTORY TRANSACTION =====
            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                  entity.HasKey(e => e.Id);

                  // Global query filter: nhất quán với Product's filter
                  // → Bình thường không hiện giao dịch của Product đã xóa mềm
                  // → Dùng .IgnoreQueryFilters() khi cần báo cáo admin đầy đủ
                  entity.HasQueryFilter(e => !e.Product!.IsDeleted);

                  // FK → Product (Xóa Product không xóa lịch sử kho)
                  entity.HasOne(e => e.Product)
                    .WithMany(p => p.InventoryTransactions)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                  // ===== INDEXES =====
                  // Index chính: lấy lịch sử kho của 1 sản phẩm, sắp xếp theo thời gian
                  entity.HasIndex(e => new { e.ProductId, e.CreatedAt })
                    .HasDatabaseName("IX_InventoryTransactions_ProductId_CreatedAt");

                  // Index hỗ trợ: Lọc theo loại giao dịch
                  entity.HasIndex(e => e.Type)
                    .HasDatabaseName("IX_InventoryTransactions_Type");

                  // Index hỗ trợ: Tìm giao dịch gốc từ đơn hàng
                  entity.HasIndex(e => e.ReferenceOrderId)
                    .HasDatabaseName("IX_InventoryTransactions_ReferenceOrderId");
            });
      }
}

