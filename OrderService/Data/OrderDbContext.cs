using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using OrderService.Utils.Enum;

namespace OrderService.Data
{
      public class OrderDbContext : DbContext
      {
            public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }
            public DbSet<Order> Orders { get; set; }
            public DbSet<OrderItem> OrderItems { get; set; }
            public DbSet<CustomerProfile> CustomerProfiles { get; set; }

            // ── MassTransit Outbox Tables ────────────────────────────────────────
            // Lưu tạm Event chưa bắn được lên RabbitMQ (Transactional Outbox Pattern)
            public DbSet<OutboxMessage> OutboxMessages { get; set; }
            public DbSet<OutboxState> OutboxStates { get; set; }
            public DbSet<InboxState> InboxStates { get; set; }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                  base.OnModelCreating(modelBuilder);


                  modelBuilder.Entity<Order>(entity =>
                  {
                        entity.HasKey(e => e.Id);
                        entity.Property(e => e.ShippingFee).HasColumnType("decimal(18,2)");

                        // ===== INDEXES =====
                        // Index đơn — cho filter riêng lẻ từng cột
                        entity.HasIndex(e => e.Status)
                        .HasDatabaseName("IX_Orders_Status");

                        entity.HasIndex(e => e.CustomerId)
                        .HasDatabaseName("IX_Orders_CustomerId");

                        entity.HasIndex(e => e.CreatedAt)
                        .HasDatabaseName("IX_Orders_CreatedAt");

                        entity.HasIndex(e => e.CustomerName)
                        .HasDatabaseName("IX_Orders_CustomerName");

                        entity.HasIndex(e => e.CustomerPhone)
                        .HasDatabaseName("IX_Orders_CustomerPhone");

                        entity.HasIndex(e => e.ShippingFee)
                        .HasDatabaseName("IX_Orders_ShippingFee");

                        entity.HasIndex(e => e.PaymentMethod)
                        .HasDatabaseName("IX_Orders_PaymentMethod");

                        // Composite index — tối ưu cho query kết hợp filter phổ biến nhất:
                        // WHERE CustomerId = ? AND Status = ? ORDER BY CreatedAt DESC
                        entity.HasIndex(e => new { e.CustomerId, e.Status, e.CreatedAt })
                        .HasDatabaseName("IX_Orders_CustomerId_Status_CreatedAt");
                  });

                  modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Order)
                      .WithMany(o => o.Items)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Index FK để JOIN nhanh
                entity.HasIndex(e => e.OrderId)
                      .HasDatabaseName("IX_OrderItems_OrderId");

                // Index ProductId để query "đơn hàng nào có sản phẩm X"
                entity.HasIndex(e => e.ProductId)
                      .HasDatabaseName("IX_OrderItems_ProductId");
            });

                  // ── Map MassTransit Outbox Tables ────────────────────────────────
                  modelBuilder.AddInboxStateEntity();
                  modelBuilder.AddOutboxMessageEntity();
                  modelBuilder.AddOutboxStateEntity();

                  // ======= SEED DATA =======
                  var customerId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
                  var customerId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
                  var customerId3 = Guid.Parse("33333333-3333-3333-3333-333333333333");

                  // 15 Orders đa dạng Status, Customer, PaymentMethod
                  var orders = new[]
                  {
                new { Id = Guid.Parse("a0000001-0000-0000-0000-000000000001"), CustomerId = (Guid?)customerId1, CustomerName = "Nguyễn Văn An", CustomerPhone = "0901000001", ShippingAddress = "123 Lê Lợi, Q1, HCM", PaymentMethod = PaymentMethod.COD, ShippingFee = 30000m, Note = "Giao giờ hành chính", Status = OrderStatus.New, CreatedAt = new DateTime(2025, 1, 5, 10, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a0000002-0000-0000-0000-000000000002"), CustomerId = (Guid?)customerId1, CustomerName = "Nguyễn Văn An", CustomerPhone = "0901000001", ShippingAddress = "123 Lê Lợi, Q1, HCM", PaymentMethod = PaymentMethod.BankTransfer, ShippingFee = 0m, Note = "", Status = OrderStatus.Processing, CreatedAt = new DateTime(2025, 1, 10, 14, 30, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a0000003-0000-0000-0000-000000000003"), CustomerId = (Guid?)customerId2, CustomerName = "Trần Thị Bích", CustomerPhone = "0902000002", ShippingAddress = "456 Nguyễn Huệ, Q1, HCM", PaymentMethod = PaymentMethod.VNPay, ShippingFee = 15000m, Note = "Gọi trước khi giao", Status = OrderStatus.Completed, CreatedAt = new DateTime(2025, 1, 15, 9, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a0000004-0000-0000-0000-000000000004"), CustomerId = (Guid?)customerId2, CustomerName = "Trần Thị Bích", CustomerPhone = "0902000002", ShippingAddress = "789 Cách Mạng Tháng 8, Q3, HCM", PaymentMethod = PaymentMethod.Momo, ShippingFee = 20000m, Note = "", Status = OrderStatus.Shipping, CreatedAt = new DateTime(2025, 2, 1, 8, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a0000005-0000-0000-0000-000000000005"), CustomerId = (Guid?)customerId3, CustomerName = "Lê Hoàng Cường", CustomerPhone = "0903000003", ShippingAddress = "10 Pasteur, Q3, HCM", PaymentMethod = PaymentMethod.COD, ShippingFee = 25000m, Note = "Hàng dễ vỡ", Status = OrderStatus.Cancelled, CreatedAt = new DateTime(2025, 2, 5, 16, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a0000006-0000-0000-0000-000000000006"), CustomerId = (Guid?)customerId1, CustomerName = "Nguyễn Văn An", CustomerPhone = "0901000001", ShippingAddress = "123 Lê Lợi, Q1, HCM", PaymentMethod = PaymentMethod.BankTransfer, ShippingFee = 0m, Note = "", Status = OrderStatus.Completed, CreatedAt = new DateTime(2025, 2, 10, 11, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a0000007-0000-0000-0000-000000000007"), CustomerId = (Guid?)customerId3, CustomerName = "Lê Hoàng Cường", CustomerPhone = "0903000003", ShippingAddress = "20 Hai Bà Trưng, Q1, HCM", PaymentMethod = PaymentMethod.VNPay, ShippingFee = 10000m, Note = "", Status = OrderStatus.New, CreatedAt = new DateTime(2025, 2, 15, 13, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a0000008-0000-0000-0000-000000000008"), CustomerId = (Guid?)customerId2, CustomerName = "Trần Thị Bích", CustomerPhone = "0902000002", ShippingAddress = "456 Nguyễn Huệ, Q1, HCM", PaymentMethod = PaymentMethod.COD, ShippingFee = 30000m, Note = "Đóng gói kỹ", Status = OrderStatus.Processing, CreatedAt = new DateTime(2025, 3, 1, 10, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a0000009-0000-0000-0000-000000000009"), CustomerId = (Guid?)customerId1, CustomerName = "Nguyễn Văn An", CustomerPhone = "0901000001", ShippingAddress = "123 Lê Lợi, Q1, HCM", PaymentMethod = PaymentMethod.Momo, ShippingFee = 15000m, Note = "", Status = OrderStatus.Shipping, CreatedAt = new DateTime(2025, 3, 5, 9, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a0000010-0000-0000-0000-000000000010"), CustomerId = (Guid?)customerId3, CustomerName = "Lê Hoàng Cường", CustomerPhone = "0903000003", ShippingAddress = "10 Pasteur, Q3, HCM", PaymentMethod = PaymentMethod.BankTransfer, ShippingFee = 0m, Note = "", Status = OrderStatus.Completed, CreatedAt = new DateTime(2025, 3, 10, 15, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a0000011-0000-0000-0000-000000000011"), CustomerId = (Guid?)customerId2, CustomerName = "Trần Thị Bích", CustomerPhone = "0902000002", ShippingAddress = "789 CMT8, Q3, HCM", PaymentMethod = PaymentMethod.COD, ShippingFee = 20000m, Note = "", Status = OrderStatus.New, CreatedAt = new DateTime(2025, 3, 15, 8, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a0000012-0000-0000-0000-000000000012"), CustomerId = (Guid?)customerId1, CustomerName = "Nguyễn Văn An", CustomerPhone = "0901000001", ShippingAddress = "123 Lê Lợi, Q1, HCM", PaymentMethod = PaymentMethod.VNPay, ShippingFee = 10000m, Note = "Giao cuối tuần", Status = OrderStatus.Processing, CreatedAt = new DateTime(2025, 3, 20, 14, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a0000013-0000-0000-0000-000000000013"), CustomerId = (Guid?)customerId3, CustomerName = "Lê Hoàng Cường", CustomerPhone = "0903000003", ShippingAddress = "20 Hai Bà Trưng, Q1, HCM", PaymentMethod = PaymentMethod.Momo, ShippingFee = 25000m, Note = "", Status = OrderStatus.Cancelled, CreatedAt = new DateTime(2025, 3, 25, 10, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a0000014-0000-0000-0000-000000000014"), CustomerId = (Guid?)customerId2, CustomerName = "Trần Thị Bích", CustomerPhone = "0902000002", ShippingAddress = "456 Nguyễn Huệ, Q1, HCM", PaymentMethod = PaymentMethod.BankTransfer, ShippingFee = 0m, Note = "", Status = OrderStatus.Completed, CreatedAt = new DateTime(2025, 4, 1, 11, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a0000015-0000-0000-0000-000000000015"), CustomerId = (Guid?)customerId1, CustomerName = "Nguyễn Văn An", CustomerPhone = "0901000001", ShippingAddress = "123 Lê Lợi, Q1, HCM", PaymentMethod = PaymentMethod.COD, ShippingFee = 30000m, Note = "Ưu tiên giao sớm", Status = OrderStatus.New, CreatedAt = new DateTime(2025, 4, 5, 16, 0, 0, DateTimeKind.Utc) },
            };

                  modelBuilder.Entity<Order>().HasData(orders);

                  // OrderItems cho từng Order
                  var orderItems = new[]
            {
                // Order 1 - 2 items
                new { Id = Guid.Parse("b0000001-0000-0000-0000-000000000001"), OrderId = Guid.Parse("a0000001-0000-0000-0000-000000000001"), ProductId = Guid.Parse("c0000001-0000-0000-0000-000000000001"), ProductName = "Áo thun nam", Quantity = 2, UnitPrice = 150000m, UnitCost = 90000m },
                new { Id = Guid.Parse("b0000002-0000-0000-0000-000000000002"), OrderId = Guid.Parse("a0000001-0000-0000-0000-000000000001"), ProductId = Guid.Parse("c0000002-0000-0000-0000-000000000002"), ProductName = "Quần jean", Quantity = 1, UnitPrice = 350000m, UnitCost = 200000m },
                // Order 2
                new { Id = Guid.Parse("b0000003-0000-0000-0000-000000000003"), OrderId = Guid.Parse("a0000002-0000-0000-0000-000000000002"), ProductId = Guid.Parse("c0000003-0000-0000-0000-000000000003"), ProductName = "Giày thể thao", Quantity = 1, UnitPrice = 890000m, UnitCost = 550000m },
                // Order 3 - 3 items
                new { Id = Guid.Parse("b0000004-0000-0000-0000-000000000004"), OrderId = Guid.Parse("a0000003-0000-0000-0000-000000000003"), ProductId = Guid.Parse("c0000001-0000-0000-0000-000000000001"), ProductName = "Áo thun nam", Quantity = 3, UnitPrice = 150000m, UnitCost = 90000m },
                new { Id = Guid.Parse("b0000005-0000-0000-0000-000000000005"), OrderId = Guid.Parse("a0000003-0000-0000-0000-000000000003"), ProductId = Guid.Parse("c0000004-0000-0000-0000-000000000004"), ProductName = "Túi xách nữ", Quantity = 1, UnitPrice = 520000m, UnitCost = 300000m },
                new { Id = Guid.Parse("b0000006-0000-0000-0000-000000000006"), OrderId = Guid.Parse("a0000003-0000-0000-0000-000000000003"), ProductId = Guid.Parse("c0000005-0000-0000-0000-000000000005"), ProductName = "Nón lưỡi trai", Quantity = 2, UnitPrice = 95000m, UnitCost = 50000m },
                // Order 4
                new { Id = Guid.Parse("b0000007-0000-0000-0000-000000000007"), OrderId = Guid.Parse("a0000004-0000-0000-0000-000000000004"), ProductId = Guid.Parse("c0000002-0000-0000-0000-000000000002"), ProductName = "Quần jean", Quantity = 2, UnitPrice = 350000m, UnitCost = 200000m },
                // Order 5
                new { Id = Guid.Parse("b0000008-0000-0000-0000-000000000008"), OrderId = Guid.Parse("a0000005-0000-0000-0000-000000000005"), ProductId = Guid.Parse("c0000003-0000-0000-0000-000000000003"), ProductName = "Giày thể thao", Quantity = 1, UnitPrice = 890000m, UnitCost = 550000m },
                new { Id = Guid.Parse("b0000009-0000-0000-0000-000000000009"), OrderId = Guid.Parse("a0000005-0000-0000-0000-000000000005"), ProductId = Guid.Parse("c0000005-0000-0000-0000-000000000005"), ProductName = "Nón lưỡi trai", Quantity = 1, UnitPrice = 95000m, UnitCost = 50000m },
                // Order 6
                new { Id = Guid.Parse("b0000010-0000-0000-0000-000000000010"), OrderId = Guid.Parse("a0000006-0000-0000-0000-000000000006"), ProductId = Guid.Parse("c0000004-0000-0000-0000-000000000004"), ProductName = "Túi xách nữ", Quantity = 1, UnitPrice = 520000m, UnitCost = 300000m },
                // Order 7
                new { Id = Guid.Parse("b0000011-0000-0000-0000-000000000011"), OrderId = Guid.Parse("a0000007-0000-0000-0000-000000000007"), ProductId = Guid.Parse("c0000001-0000-0000-0000-000000000001"), ProductName = "Áo thun nam", Quantity = 5, UnitPrice = 150000m, UnitCost = 90000m },
                // Order 8
                new { Id = Guid.Parse("b0000012-0000-0000-0000-000000000012"), OrderId = Guid.Parse("a0000008-0000-0000-0000-000000000008"), ProductId = Guid.Parse("c0000002-0000-0000-0000-000000000002"), ProductName = "Quần jean", Quantity = 1, UnitPrice = 350000m, UnitCost = 200000m },
                new { Id = Guid.Parse("b0000013-0000-0000-0000-000000000013"), OrderId = Guid.Parse("a0000008-0000-0000-0000-000000000008"), ProductId = Guid.Parse("c0000003-0000-0000-0000-000000000003"), ProductName = "Giày thể thao", Quantity = 1, UnitPrice = 890000m, UnitCost = 550000m },
                // Order 9
                new { Id = Guid.Parse("b0000014-0000-0000-0000-000000000014"), OrderId = Guid.Parse("a0000009-0000-0000-0000-000000000009"), ProductId = Guid.Parse("c0000005-0000-0000-0000-000000000005"), ProductName = "Nón lưỡi trai", Quantity = 3, UnitPrice = 95000m, UnitCost = 50000m },
                // Order 10
                new { Id = Guid.Parse("b0000015-0000-0000-0000-000000000015"), OrderId = Guid.Parse("a0000010-0000-0000-0000-000000000010"), ProductId = Guid.Parse("c0000001-0000-0000-0000-000000000001"), ProductName = "Áo thun nam", Quantity = 1, UnitPrice = 150000m, UnitCost = 90000m },
                new { Id = Guid.Parse("b0000016-0000-0000-0000-000000000016"), OrderId = Guid.Parse("a0000010-0000-0000-0000-000000000010"), ProductId = Guid.Parse("c0000004-0000-0000-0000-000000000004"), ProductName = "Túi xách nữ", Quantity = 2, UnitPrice = 520000m, UnitCost = 300000m },
                // Order 11
                new { Id = Guid.Parse("b0000017-0000-0000-0000-000000000017"), OrderId = Guid.Parse("a0000011-0000-0000-0000-000000000011"), ProductId = Guid.Parse("c0000002-0000-0000-0000-000000000002"), ProductName = "Quần jean", Quantity = 1, UnitPrice = 350000m, UnitCost = 200000m },
                // Order 12
                new { Id = Guid.Parse("b0000018-0000-0000-0000-000000000018"), OrderId = Guid.Parse("a0000012-0000-0000-0000-000000000012"), ProductId = Guid.Parse("c0000003-0000-0000-0000-000000000003"), ProductName = "Giày thể thao", Quantity = 1, UnitPrice = 890000m, UnitCost = 550000m },
                new { Id = Guid.Parse("b0000019-0000-0000-0000-000000000019"), OrderId = Guid.Parse("a0000012-0000-0000-0000-000000000012"), ProductId = Guid.Parse("c0000001-0000-0000-0000-000000000001"), ProductName = "Áo thun nam", Quantity = 2, UnitPrice = 150000m, UnitCost = 90000m },
                // Order 13
                new { Id = Guid.Parse("b0000020-0000-0000-0000-000000000020"), OrderId = Guid.Parse("a0000013-0000-0000-0000-000000000013"), ProductId = Guid.Parse("c0000004-0000-0000-0000-000000000004"), ProductName = "Túi xách nữ", Quantity = 1, UnitPrice = 520000m, UnitCost = 300000m },
                // Order 14
                new { Id = Guid.Parse("b0000021-0000-0000-0000-000000000021"), OrderId = Guid.Parse("a0000014-0000-0000-0000-000000000014"), ProductId = Guid.Parse("c0000005-0000-0000-0000-000000000005"), ProductName = "Nón lưỡi trai", Quantity = 4, UnitPrice = 95000m, UnitCost = 50000m },
                new { Id = Guid.Parse("b0000022-0000-0000-0000-000000000022"), OrderId = Guid.Parse("a0000014-0000-0000-0000-000000000014"), ProductId = Guid.Parse("c0000002-0000-0000-0000-000000000002"), ProductName = "Quần jean", Quantity = 1, UnitPrice = 350000m, UnitCost = 200000m },
                // Order 15
                new { Id = Guid.Parse("b0000023-0000-0000-0000-000000000023"), OrderId = Guid.Parse("a0000015-0000-0000-0000-000000000015"), ProductId = Guid.Parse("c0000001-0000-0000-0000-000000000001"), ProductName = "Áo thun nam", Quantity = 1, UnitPrice = 150000m, UnitCost = 90000m },
                new { Id = Guid.Parse("b0000024-0000-0000-0000-000000000024"), OrderId = Guid.Parse("a0000015-0000-0000-0000-000000000015"), ProductId = Guid.Parse("c0000003-0000-0000-0000-000000000003"), ProductName = "Giày thể thao", Quantity = 1, UnitPrice = 890000m, UnitCost = 550000m },
                new { Id = Guid.Parse("b0000025-0000-0000-0000-000000000025"), OrderId = Guid.Parse("a0000015-0000-0000-0000-000000000015"), ProductId = Guid.Parse("c0000004-0000-0000-0000-000000000004"), ProductName = "Túi xách nữ", Quantity = 1, UnitPrice = 520000m, UnitCost = 300000m },
            };


                  modelBuilder.Entity<OrderItem>().HasData(orderItems);
            }
      }
}
