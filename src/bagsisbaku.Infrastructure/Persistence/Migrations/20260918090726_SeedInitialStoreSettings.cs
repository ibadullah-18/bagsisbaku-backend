using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bagsisbaku.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialStoreSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "store",
                table: "store_settings",
                columns: new[] { "Id", "AboutText", "Address", "CreatedAtUtc", "DeliveryInformation", "Email", "InstagramUrl", "Latitude", "LogoPublicId", "LogoUrl", "Longitude", "MapUrl", "PrimaryPhone", "ReturnPolicy", "StoreName", "TikTokUrl", "UpdatedAtUtc", "WhatsAppPhone", "WorkingHours" },
                values: new object[] { new Guid("b4616c76-8748-447a-b154-e4d3fd5f8e72"), "bagsisbaku çanta və ayaqqabı mağazasıdır.", "Bakı, Azərbaycan", new DateTimeOffset(new DateTime(2026, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Çatdırılma məlumatları sifariş zamanı dəqiqləşdirilir.", "bagsisbaku2026@gmail.com", "https://www.instagram.com/bagsisbaku", 40.376504m, null, null, 49.841709m, null, "+994519723718", "Qaytarma və dəyişdirmə şərtləri mağaza ilə razılaşdırılır.", "bagsisbaku", null, null, "+994519723718", "Hər gün 10:00–21:00" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "store",
                table: "store_settings",
                keyColumn: "Id",
                keyValue: new Guid("b4616c76-8748-447a-b154-e4d3fd5f8e72"));
        }
    }
}
