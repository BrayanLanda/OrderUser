using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Products.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReservations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "Price", "Stock", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-0001-0000-0000-000000000001"), new DateTime(2026, 3, 5, 5, 15, 40, 6, DateTimeKind.Utc).AddTicks(37), "Laptop de alto rendimiento", true, "Laptop Pro 15", 1500.00m, 10, new DateTime(2026, 3, 5, 5, 15, 40, 6, DateTimeKind.Utc).AddTicks(38) },
                    { new Guid("a1b2c3d4-0002-0000-0000-000000000002"), new DateTime(2026, 3, 5, 5, 15, 40, 6, DateTimeKind.Utc).AddTicks(2828), "Mouse ergonómico inalámbrico", true, "Mouse Inalámbrico", 35.00m, 50, new DateTime(2026, 3, 5, 5, 15, 40, 6, DateTimeKind.Utc).AddTicks(2828) },
                    { new Guid("a1b2c3d4-0003-0000-0000-000000000003"), new DateTime(2026, 3, 5, 5, 15, 40, 6, DateTimeKind.Utc).AddTicks(4926), "Teclado mecánico RGB", true, "Teclado Mecánico", 120.00m, 25, new DateTime(2026, 3, 5, 5, 15, 40, 6, DateTimeKind.Utc).AddTicks(4926) },
                    { new Guid("a1b2c3d4-0004-0000-0000-000000000004"), new DateTime(2026, 3, 5, 5, 15, 40, 6, DateTimeKind.Utc).AddTicks(6297), "Monitor 27 pulgadas 4K", true, "Monitor 4K", 450.00m, 5, new DateTime(2026, 3, 5, 5, 15, 40, 6, DateTimeKind.Utc).AddTicks(6298) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_OrderId",
                table: "StockReservations",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_OrderId_ProductId",
                table: "StockReservations",
                columns: new[] { "OrderId", "ProductId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "StockReservations");
        }
    }
}
