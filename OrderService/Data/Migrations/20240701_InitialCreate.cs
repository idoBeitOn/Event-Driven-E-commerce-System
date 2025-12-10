using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace OrderService.Data.Migrations;

// Migration class: defines how to create (Up) and remove (Down) the DB schema.
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                OrderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CustomerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                CustomerEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                CustomerAddress = table.Column<string>(type: "text", nullable: false),
                CustomerCity = table.Column<string>(type: "text", nullable: false),
                CustomerCountry = table.Column<string>(type: "text", nullable: false),
                CustomerPostalCode = table.Column<string>(type: "text", nullable: false),
                SubTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                Tax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                Discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                Currency = table.Column<string>(type: "text", nullable: false),
                ShippingMethod = table.Column<string>(type: "text", nullable: false),
                ShippingCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                ShippingAddress = table.Column<string>(type: "text", nullable: false),
                ShippingCity = table.Column<string>(type: "text", nullable: false),
                ShippingCountry = table.Column<string>(type: "text", nullable: false),
                ShippingPostalCode = table.Column<string>(type: "text", nullable: false),
                EstimatedDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                PaymentMethod = table.Column<string>(type: "text", nullable: false),
                TransactionId = table.Column<string>(type: "text", nullable: false),
                Paid = table.Column<bool>(type: "boolean", nullable: false),
                Source = table.Column<string>(type: "text", nullable: false),
                Version = table.Column<int>(type: "integer", nullable: false),
                ItemsNum = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.OrderId);
            });

        migrationBuilder.CreateTable(
            name: "OrderItems",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                OrderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                LineId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Quantity = table.Column<int>(type: "integer", nullable: false),
                UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                Currency = table.Column<string>(type: "text", nullable: false),
                Category = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_OrderItems_Orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "Orders",
                    principalColumn: "OrderId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_OrderItems_OrderId",
            table: "OrderItems",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderItems_OrderId_LineId",
            table: "OrderItems",
            columns: new[] { "OrderId", "LineId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Orders_CreatedAt",
            table: "Orders",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_Status",
            table: "Orders",
            column: "Status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OrderItems");

        migrationBuilder.DropTable(
            name: "Orders");
    }
}
