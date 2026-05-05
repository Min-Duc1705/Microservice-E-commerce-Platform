using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerProfileTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId_Status_CreatedAt",
                table: "Orders",
                columns: new[] { "CustomerId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerName",
                table: "Orders",
                column: "CustomerName");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerPhone",
                table: "Orders",
                column: "CustomerPhone");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentMethod",
                table: "Orders",
                column: "PaymentMethod");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingFee",
                table: "Orders",
                column: "ShippingFee");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerId_Status_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerName",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerPhone",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PaymentMethod",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShippingFee",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems");
        }
    }
}
