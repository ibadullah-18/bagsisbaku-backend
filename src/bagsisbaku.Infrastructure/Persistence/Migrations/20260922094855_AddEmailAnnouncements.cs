using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bagsisbaku.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailAnnouncements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "announcements");

            migrationBuilder.CreateTable(
                name: "email_announcements",
                schema: "announcements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByAdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HtmlBody = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: false),
                    TextBody = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RecipientCount = table.Column<int>(type: "int", nullable: false),
                    SentCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    QueuedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true),
                    ProcessingStartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_announcements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_email_announcements_users_CreatedByAdminId",
                        column: x => x.CreatedByAdminId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "email_announcement_recipients",
                schema: "announcements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnnouncementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    LastAttemptAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true),
                    SentAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_announcement_recipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_email_announcement_recipients_email_announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalSchema: "announcements",
                        principalTable: "email_announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_email_announcement_recipients_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_announcement_recipients_status_created",
                schema: "announcements",
                table: "email_announcement_recipients",
                columns: new[] { "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_email_announcement_recipients_UserId",
                schema: "announcements",
                table: "email_announcement_recipients",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "ux_announcement_recipients_announcement_email",
                schema: "announcements",
                table: "email_announcement_recipients",
                columns: new[] { "AnnouncementId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_email_announcements_CreatedByAdminId",
                schema: "announcements",
                table: "email_announcements",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_email_announcements_Status_CreatedAtUtc",
                schema: "announcements",
                table: "email_announcements",
                columns: new[] { "Status", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_announcement_recipients",
                schema: "announcements");

            migrationBuilder.DropTable(
                name: "email_announcements",
                schema: "announcements");
        }
    }
}
