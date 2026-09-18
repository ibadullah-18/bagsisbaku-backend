using bagsisbaku.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations.Promotions;

internal sealed class PromoCodeUsageReleaseConfiguration
    : IEntityTypeConfiguration<PromoCodeUsage>
{
    public void Configure(
        EntityTypeBuilder<PromoCodeUsage> builder)
    {
        builder
            .Property(usage => usage.ReleasedAtUtc)
            .IsRequired(false);

        builder.Ignore(
            usage => usage.IsReleased);

        builder
            .HasIndex(
                usage => new
                {
                    usage.PromoCodeId,
                    usage.UserId,
                    usage.ReleasedAtUtc
                })
            .HasDatabaseName(
                "IX_promo_code_usages_PromoCodeId_UserId_ReleasedAtUtc");
    }
}