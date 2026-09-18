using bagsisbaku.Domain.Orders;
using bagsisbaku.Domain.Promotions;
using bagsisbaku.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class PromoCodeUsageConfiguration
    : IEntityTypeConfiguration<PromoCodeUsage>
{
    public void Configure(
        EntityTypeBuilder<PromoCodeUsage> builder)
    {
        builder.ToTable(
            "promo_code_usages",
            "promotions");

        builder.HasKey(usage =>
            usage.Id);

        builder.Property(usage =>
                usage.Id)
            .ValueGeneratedNever();

        builder.Property(usage =>
                usage.PromoCodeId)
            .IsRequired();

        builder.Property(usage =>
                usage.UserId)
            .IsRequired();

        builder.Property(usage =>
                usage.OrderId)
            .IsRequired();

        builder.Property(usage =>
                usage.DiscountAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(usage =>
                usage.UsedAtUtc)
            .HasPrecision(0)
            .IsRequired();

        builder.HasIndex(usage =>
                usage.OrderId)
            .IsUnique()
            .HasDatabaseName(
                "ux_promo_code_usages_order");

        builder.HasIndex(usage =>
                new
                {
                    usage.PromoCodeId,
                    usage.UsedAtUtc
                })
            .HasDatabaseName(
                "ix_promo_code_usages_promo_date");

        builder.HasIndex(usage =>
                new
                {
                    usage.UserId,
                    usage.PromoCodeId
                })
            .HasDatabaseName(
                "ix_promo_code_usages_user_promo");

        builder.HasOne<PromoCode>()
            .WithMany()
            .HasForeignKey(usage =>
                usage.PromoCodeId)
            .OnDelete(
                DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(usage =>
                usage.UserId)
            .OnDelete(
                DeleteBehavior.Restrict);

        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(usage =>
                usage.OrderId)
            .OnDelete(
                DeleteBehavior.Cascade);
    }
}