using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OrderService.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CreatedAt", "CustomerId", "CustomerName", "CustomerPhone", "Note", "PaymentMethod", "ShippingAddress", "ShippingFee", "Status" },
                values: new object[,]
                {
                    { new Guid("a0000001-0000-0000-0000-000000000001"), new DateTime(2025, 1, 5, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-1111-1111-1111-111111111111"), "Nguyễn Văn An", "0901000001", "Giao giờ hành chính", 0, "123 Lê Lợi, Q1, HCM", 30000m, 0 },
                    { new Guid("a0000002-0000-0000-0000-000000000002"), new DateTime(2025, 1, 10, 14, 30, 0, 0, DateTimeKind.Utc), new Guid("11111111-1111-1111-1111-111111111111"), "Nguyễn Văn An", "0901000001", "", 1, "123 Lê Lợi, Q1, HCM", 0m, 1 },
                    { new Guid("a0000003-0000-0000-0000-000000000003"), new DateTime(2025, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), "Trần Thị Bích", "0902000002", "Gọi trước khi giao", 2, "456 Nguyễn Huệ, Q1, HCM", 15000m, 3 },
                    { new Guid("a0000004-0000-0000-0000-000000000004"), new DateTime(2025, 2, 1, 8, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), "Trần Thị Bích", "0902000002", "", 3, "789 Cách Mạng Tháng 8, Q3, HCM", 20000m, 2 },
                    { new Guid("a0000005-0000-0000-0000-000000000005"), new DateTime(2025, 2, 5, 16, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-3333-3333-3333-333333333333"), "Lê Hoàng Cường", "0903000003", "Hàng dễ vỡ", 0, "10 Pasteur, Q3, HCM", 25000m, 4 },
                    { new Guid("a0000006-0000-0000-0000-000000000006"), new DateTime(2025, 2, 10, 11, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-1111-1111-1111-111111111111"), "Nguyễn Văn An", "0901000001", "", 1, "123 Lê Lợi, Q1, HCM", 0m, 3 },
                    { new Guid("a0000007-0000-0000-0000-000000000007"), new DateTime(2025, 2, 15, 13, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-3333-3333-3333-333333333333"), "Lê Hoàng Cường", "0903000003", "", 2, "20 Hai Bà Trưng, Q1, HCM", 10000m, 0 },
                    { new Guid("a0000008-0000-0000-0000-000000000008"), new DateTime(2025, 3, 1, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), "Trần Thị Bích", "0902000002", "Đóng gói kỹ", 0, "456 Nguyễn Huệ, Q1, HCM", 30000m, 1 },
                    { new Guid("a0000009-0000-0000-0000-000000000009"), new DateTime(2025, 3, 5, 9, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-1111-1111-1111-111111111111"), "Nguyễn Văn An", "0901000001", "", 3, "123 Lê Lợi, Q1, HCM", 15000m, 2 },
                    { new Guid("a0000010-0000-0000-0000-000000000010"), new DateTime(2025, 3, 10, 15, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-3333-3333-3333-333333333333"), "Lê Hoàng Cường", "0903000003", "", 1, "10 Pasteur, Q3, HCM", 0m, 3 },
                    { new Guid("a0000011-0000-0000-0000-000000000011"), new DateTime(2025, 3, 15, 8, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), "Trần Thị Bích", "0902000002", "", 0, "789 CMT8, Q3, HCM", 20000m, 0 },
                    { new Guid("a0000012-0000-0000-0000-000000000012"), new DateTime(2025, 3, 20, 14, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-1111-1111-1111-111111111111"), "Nguyễn Văn An", "0901000001", "Giao cuối tuần", 2, "123 Lê Lợi, Q1, HCM", 10000m, 1 },
                    { new Guid("a0000013-0000-0000-0000-000000000013"), new DateTime(2025, 3, 25, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-3333-3333-3333-333333333333"), "Lê Hoàng Cường", "0903000003", "", 3, "20 Hai Bà Trưng, Q1, HCM", 25000m, 4 },
                    { new Guid("a0000014-0000-0000-0000-000000000014"), new DateTime(2025, 4, 1, 11, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), "Trần Thị Bích", "0902000002", "", 1, "456 Nguyễn Huệ, Q1, HCM", 0m, 3 },
                    { new Guid("a0000015-0000-0000-0000-000000000015"), new DateTime(2025, 4, 5, 16, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-1111-1111-1111-111111111111"), "Nguyễn Văn An", "0901000001", "Ưu tiên giao sớm", 0, "123 Lê Lợi, Q1, HCM", 30000m, 0 }
                });

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "Id", "OrderId", "ProductId", "ProductName", "Quantity", "UnitPrice" },
                values: new object[,]
                {
                    { new Guid("b0000001-0000-0000-0000-000000000001"), new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("c0000001-0000-0000-0000-000000000001"), "Áo thun nam", 2, 150000m },
                    { new Guid("b0000002-0000-0000-0000-000000000002"), new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("c0000002-0000-0000-0000-000000000002"), "Quần jean", 1, 350000m },
                    { new Guid("b0000003-0000-0000-0000-000000000003"), new Guid("a0000002-0000-0000-0000-000000000002"), new Guid("c0000003-0000-0000-0000-000000000003"), "Giày thể thao", 1, 890000m },
                    { new Guid("b0000004-0000-0000-0000-000000000004"), new Guid("a0000003-0000-0000-0000-000000000003"), new Guid("c0000001-0000-0000-0000-000000000001"), "Áo thun nam", 3, 150000m },
                    { new Guid("b0000005-0000-0000-0000-000000000005"), new Guid("a0000003-0000-0000-0000-000000000003"), new Guid("c0000004-0000-0000-0000-000000000004"), "Túi xách nữ", 1, 520000m },
                    { new Guid("b0000006-0000-0000-0000-000000000006"), new Guid("a0000003-0000-0000-0000-000000000003"), new Guid("c0000005-0000-0000-0000-000000000005"), "Nón lưỡi trai", 2, 95000m },
                    { new Guid("b0000007-0000-0000-0000-000000000007"), new Guid("a0000004-0000-0000-0000-000000000004"), new Guid("c0000002-0000-0000-0000-000000000002"), "Quần jean", 2, 350000m },
                    { new Guid("b0000008-0000-0000-0000-000000000008"), new Guid("a0000005-0000-0000-0000-000000000005"), new Guid("c0000003-0000-0000-0000-000000000003"), "Giày thể thao", 1, 890000m },
                    { new Guid("b0000009-0000-0000-0000-000000000009"), new Guid("a0000005-0000-0000-0000-000000000005"), new Guid("c0000005-0000-0000-0000-000000000005"), "Nón lưỡi trai", 1, 95000m },
                    { new Guid("b0000010-0000-0000-0000-000000000010"), new Guid("a0000006-0000-0000-0000-000000000006"), new Guid("c0000004-0000-0000-0000-000000000004"), "Túi xách nữ", 1, 520000m },
                    { new Guid("b0000011-0000-0000-0000-000000000011"), new Guid("a0000007-0000-0000-0000-000000000007"), new Guid("c0000001-0000-0000-0000-000000000001"), "Áo thun nam", 5, 150000m },
                    { new Guid("b0000012-0000-0000-0000-000000000012"), new Guid("a0000008-0000-0000-0000-000000000008"), new Guid("c0000002-0000-0000-0000-000000000002"), "Quần jean", 1, 350000m },
                    { new Guid("b0000013-0000-0000-0000-000000000013"), new Guid("a0000008-0000-0000-0000-000000000008"), new Guid("c0000003-0000-0000-0000-000000000003"), "Giày thể thao", 1, 890000m },
                    { new Guid("b0000014-0000-0000-0000-000000000014"), new Guid("a0000009-0000-0000-0000-000000000009"), new Guid("c0000005-0000-0000-0000-000000000005"), "Nón lưỡi trai", 3, 95000m },
                    { new Guid("b0000015-0000-0000-0000-000000000015"), new Guid("a0000010-0000-0000-0000-000000000010"), new Guid("c0000001-0000-0000-0000-000000000001"), "Áo thun nam", 1, 150000m },
                    { new Guid("b0000016-0000-0000-0000-000000000016"), new Guid("a0000010-0000-0000-0000-000000000010"), new Guid("c0000004-0000-0000-0000-000000000004"), "Túi xách nữ", 2, 520000m },
                    { new Guid("b0000017-0000-0000-0000-000000000017"), new Guid("a0000011-0000-0000-0000-000000000011"), new Guid("c0000002-0000-0000-0000-000000000002"), "Quần jean", 1, 350000m },
                    { new Guid("b0000018-0000-0000-0000-000000000018"), new Guid("a0000012-0000-0000-0000-000000000012"), new Guid("c0000003-0000-0000-0000-000000000003"), "Giày thể thao", 1, 890000m },
                    { new Guid("b0000019-0000-0000-0000-000000000019"), new Guid("a0000012-0000-0000-0000-000000000012"), new Guid("c0000001-0000-0000-0000-000000000001"), "Áo thun nam", 2, 150000m },
                    { new Guid("b0000020-0000-0000-0000-000000000020"), new Guid("a0000013-0000-0000-0000-000000000013"), new Guid("c0000004-0000-0000-0000-000000000004"), "Túi xách nữ", 1, 520000m },
                    { new Guid("b0000021-0000-0000-0000-000000000021"), new Guid("a0000014-0000-0000-0000-000000000014"), new Guid("c0000005-0000-0000-0000-000000000005"), "Nón lưỡi trai", 4, 95000m },
                    { new Guid("b0000022-0000-0000-0000-000000000022"), new Guid("a0000014-0000-0000-0000-000000000014"), new Guid("c0000002-0000-0000-0000-000000000002"), "Quần jean", 1, 350000m },
                    { new Guid("b0000023-0000-0000-0000-000000000023"), new Guid("a0000015-0000-0000-0000-000000000015"), new Guid("c0000001-0000-0000-0000-000000000001"), "Áo thun nam", 1, 150000m },
                    { new Guid("b0000024-0000-0000-0000-000000000024"), new Guid("a0000015-0000-0000-0000-000000000015"), new Guid("c0000003-0000-0000-0000-000000000003"), "Giày thể thao", 1, 890000m },
                    { new Guid("b0000025-0000-0000-0000-000000000025"), new Guid("a0000015-0000-0000-0000-000000000015"), new Guid("c0000004-0000-0000-0000-000000000004"), "Túi xách nữ", 1, 520000m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000004-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000005-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000006-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000007-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000008-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000009-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000010-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000011-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000012-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000013-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000014-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000015-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000016-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000017-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000018-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000019-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000020-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000021-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000022-0000-0000-0000-000000000022"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000023-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000024-0000-0000-0000-000000000024"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000025-0000-0000-0000-000000000025"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000004-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000005-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000006-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000007-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000008-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000009-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000010-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000011-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000012-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000013-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000014-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: new Guid("a0000015-0000-0000-0000-000000000015"));
        }
    }
}
