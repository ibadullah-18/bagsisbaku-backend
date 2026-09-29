using bagsisbaku.Domain.Announcements;
using bagsisbaku.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class EmailAnnouncementConfiguration
    : IEntityTypeConfiguration<EmailAnnouncement>
{
    public void Configure(
        EntityTypeBuilder<EmailAnnouncement> builder)
    {
        builder.ToTable(
            "email_announcements",
            "announcements");

        builder.ConfigureAuditable();

        builder.Property(
                announcement =>
                    announcement.CreatedByAdminId)
            .IsRequired();

        builder.Property(
                announcement =>
                    announcement.Subject)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(
                announcement =>
                    announcement.HtmlBody)
            .HasMaxLength(20_000)
            .IsRequired();

        builder.Property(
                announcement =>
                    announcement.TextBody)
            .HasMaxLength(10_000)
            .IsRequired();

        builder.Property(
                announcement =>
                    announcement.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(
                announcement =>
                    announcement.RecipientCount)
            .IsRequired();

        builder.Property(
                announcement =>
                    announcement.SentCount)
            .IsRequired();

        builder.Property(
                announcement =>
                    announcement.FailedCount)
            .IsRequired();

        builder.Property(
                announcement =>
                    announcement.QueuedAtUtc)
            .HasPrecision(0);

        builder.Property(
                announcement =>
                    announcement.ProcessingStartedAtUtc)
            .HasPrecision(0);

        builder.Property(
                announcement =>
                    announcement.CompletedAtUtc)
            .HasPrecision(0);

        builder.HasIndex(
            announcement => new
            {
                announcement.Status,
                announcement.CreatedAtUtc
            });

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(
                announcement =>
                    announcement.CreatedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}