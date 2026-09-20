using bagsisbaku.Domain.Engagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class SiteVisitConfiguration
    : IEntityTypeConfiguration<SiteVisit>
{
    public void Configure(
        EntityTypeBuilder<SiteVisit> builder)
    {
        builder.ToTable(
            "site_visits",
            "engagement");

        builder.ConfigureAuditable();

        builder.Property(visit => visit.VisitorId)
            .IsRequired();

        builder.Property(visit => visit.PagePath)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(visit => new
        {
            visit.VisitorId,
            visit.CreatedAtUtc
        });

        builder.HasIndex(visit => new
        {
            visit.CreatedAtUtc,
            visit.PagePath
        });
    }
}