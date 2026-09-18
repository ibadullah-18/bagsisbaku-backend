using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bagsisbaku.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPromotionSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PromoCode",
                schema: "sales",
                table: "orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PromoCodeId",
                schema: "sales",
                table: "orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PromoDiscountAmount",
                schema: "sales",
                table: "orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_orders_PromoCodeId",
                schema: "sales",
                table: "orders",
                column: "PromoCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_orders_promo_codes_PromoCodeId",
                schema: "sales",
                table: "orders",
                column: "PromoCodeId",
                principalSchema: "promotions",
                principalTable: "promo_codes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orders_promo_codes_PromoCodeId",
                schema: "sales",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_orders_PromoCodeId",
                schema: "sales",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PromoCode",
                schema: "sales",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PromoCodeId",
                schema: "sales",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PromoDiscountAmount",
                schema: "sales",
                table: "orders");
        }
    }
}
