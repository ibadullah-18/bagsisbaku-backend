using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bagsisbaku.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPromoUsageReleaseTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReleasedAtUtc",
                schema: "promotions",
                table: "promo_code_usages",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_promo_code_usages_PromoCodeId_UserId_ReleasedAtUtc",
                schema: "promotions",
                table: "promo_code_usages",
                columns: new[] { "PromoCodeId", "UserId", "ReleasedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_promo_code_usages_PromoCodeId_UserId_ReleasedAtUtc",
                schema: "promotions",
                table: "promo_code_usages");

            migrationBuilder.DropColumn(
                name: "ReleasedAtUtc",
                schema: "promotions",
                table: "promo_code_usages");
        }
    }
}
