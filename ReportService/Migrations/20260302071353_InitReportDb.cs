using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportService.Migrations
{
    /// <inheritdoc />
    public partial class InitReportDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductStockSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    SKU = table.Column<string>(type: "text", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "integer", nullable: false),
                    SoldLast30Days = table.Column<int>(type: "integer", nullable: false),
                    LastSoldAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStockSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RevenueReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Period = table.Column<short>(type: "smallint", nullable: false),
                    PeriodLabel = table.Column<string>(type: "text", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    TotalItemsSold = table.Column<int>(type: "integer", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevenueReports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockSnapshots_LastSoldAt",
                table: "ProductStockSnapshots",
                column: "LastSoldAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockSnapshots_ProductId",
                table: "ProductStockSnapshots",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockSnapshots_SoldLast30Days",
                table: "ProductStockSnapshots",
                column: "SoldLast30Days");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockSnapshots_StockQuantity",
                table: "ProductStockSnapshots",
                column: "StockQuantity");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockSnapshots_StockQuantity_LowStockThreshold",
                table: "ProductStockSnapshots",
                columns: new[] { "StockQuantity", "LowStockThreshold" });

            migrationBuilder.CreateIndex(
                name: "IX_RevenueReports_Period",
                table: "RevenueReports",
                column: "Period");

            migrationBuilder.CreateIndex(
                name: "IX_RevenueReports_Period_PeriodLabel",
                table: "RevenueReports",
                columns: new[] { "Period", "PeriodLabel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductStockSnapshots");

            migrationBuilder.DropTable(
                name: "RevenueReports");
        }
    }
}
