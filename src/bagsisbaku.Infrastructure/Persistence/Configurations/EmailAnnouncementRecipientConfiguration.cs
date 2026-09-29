using bagsisbaku.Domain.Announcements;
using bagsisbaku.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class EmailAnnouncementRecipientConfiguration
    : IEntityTypeConfiguration<EmailAnnouncementRecipient>
{
    public void Configure(
        EntityTypeBuilder<EmailAnnouncementRecipient> builder)
    {
        builder.ToTable(
            "email_announcement_recipients",
            "announcements");

        builder.ConfigureAuditable();

        builder.Property(
                recipient =>
                    recipient.AnnouncementId)
            .IsRequired();

        builder.Property(
                recipient =>
                    recipient.UserId)
            .IsRequired(false);

        builder.Property(
                recipient =>
                    recipient.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(
                recipient =>
                    recipient.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(
                recipient =>
                    recipient.AttemptCount)
            .IsRequired();

        builder.Property(
                recipient =>
                    recipient.LastAttemptAtUtc)
            .HasPrecision(0);

        builder.Property(
                recipient =>
                    recipient.SentAtUtc)
            .HasPrecision(0);

        builder.Property(
                recipient =>
                    recipient.FailureReason)
            .HasMaxLength(2000);

        builder.HasIndex(
                recipient => new
                {
                    recipient.AnnouncementId,
                    recipient.Email
                })
            .IsUnique()
            .HasDatabaseName(
                "ux_announcement_recipients_announcement_email");

        builder.HasIndex(
            recipient => new
            {
                recipient.Status,
                recipient.CreatedAtUtc
            })
            .HasDatabaseName(
                "ix_announcement_recipients_status_created");

        builder.HasOne<EmailAnnouncement>()
            .WithMany()
            .HasForeignKey(
                recipient =>
                    recipient.AnnouncementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(
                recipient =>
                    recipient.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}