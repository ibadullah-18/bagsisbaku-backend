using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bagsisbaku.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreSettingsFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "store");

            migrationBuilder.CreateTable(
                name: "store_settings",
                schema: "store",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PrimaryPhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    WhatsAppPhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    InstagramUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    TikTokUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    WorkingHours = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DeliveryInformation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ReturnPolicy = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AboutText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    MapUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    LogoPublicId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_store_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "store_setting_translations",
                schema: "store",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreSettingsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Language = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    WorkingHours = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DeliveryInformation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ReturnPolicy = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AboutText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_store_setting_translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_store_setting_translations_store_settings_StoreSettingsId",
                        column: x => x.StoreSettingsId,
                        principalSchema: "store",
                        principalTable: "store_settings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_store_setting_translations_settings_language",
                schema: "store",
                table: "store_setting_translations",
                columns: new[] { "StoreSettingsId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_store_settings_store_name",
                schema: "store",
                table: "store_settings",
                column: "StoreName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "store_setting_translations",
                schema: "store");

            migrationBuilder.DropTable(
                name: "store_settings",
                schema: "store");
        }
    }
}
