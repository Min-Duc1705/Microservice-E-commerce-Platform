using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitCostToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "OrderItems",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000001"),
                column: "UnitCost",
                value: 90000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000002"),
                column: "UnitCost",
                value: 200000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000003-0000-0000-0000-000000000003"),
                column: "UnitCost",
                value: 550000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000004-0000-0000-0000-000000000004"),
                column: "UnitCost",
                value: 90000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000005-0000-0000-0000-000000000005"),
                column: "UnitCost",
                value: 300000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000006-0000-0000-0000-000000000006"),
                column: "UnitCost",
                value: 50000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000007-0000-0000-0000-000000000007"),
                column: "UnitCost",
                value: 200000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000008-0000-0000-0000-000000000008"),
                column: "UnitCost",
                value: 550000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000009-0000-0000-0000-000000000009"),
                column: "UnitCost",
                value: 50000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000010-0000-0000-0000-000000000010"),
                column: "UnitCost",
                value: 300000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000011-0000-0000-0000-000000000011"),
                column: "UnitCost",
                value: 90000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000012-0000-0000-0000-000000000012"),
                column: "UnitCost",
                value: 200000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000013-0000-0000-0000-000000000013"),
                column: "UnitCost",
                value: 550000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000014-0000-0000-0000-000000000014"),
                column: "UnitCost",
                value: 50000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000015-0000-0000-0000-000000000015"),
                column: "UnitCost",
                value: 90000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000016-0000-0000-0000-000000000016"),
                column: "UnitCost",
                value: 300000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000017-0000-0000-0000-000000000017"),
                column: "UnitCost",
                value: 200000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000018-0000-0000-0000-000000000018"),
                column: "UnitCost",
                value: 550000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000019-0000-0000-0000-000000000019"),
                column: "UnitCost",
                value: 90000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000020-0000-0000-0000-000000000020"),
                column: "UnitCost",
                value: 300000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000021-0000-0000-0000-000000000021"),
                column: "UnitCost",
                value: 50000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000022-0000-0000-0000-000000000022"),
                column: "UnitCost",
                value: 200000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000023-0000-0000-0000-000000000023"),
                column: "UnitCost",
                value: 90000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000024-0000-0000-0000-000000000024"),
                column: "UnitCost",
                value: 550000m);

            migrationBuilder.UpdateData(
                table: "OrderItems",
                keyColumn: "Id",
                keyValue: new Guid("b0000025-0000-0000-0000-000000000025"),
                column: "UnitCost",
                value: 300000m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "OrderItems");
        }
    }
}
