using bagsisbaku.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class PromoCodeConfiguration
    : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(
        EntityTypeBuilder<PromoCode> builder)
    {
        builder.ToTable(
            "promo_codes",
            "promotions");

        builder.HasKey(promoCode =>
            promoCode.Id);

        builder.Property(promoCode =>
                promoCode.Id)
            .ValueGeneratedNever();

        builder.Property(promoCode =>
                promoCode.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(promoCode =>
                promoCode.Code)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(promoCode =>
                promoCode.DiscountType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(promoCode =>
                promoCode.DiscountValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(promoCode =>
                promoCode.MinimumOrderAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(promoCode =>
                promoCode.MaximumDiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(promoCode =>
                promoCode.UsageLimit);

        builder.Property(promoCode =>
                promoCode.UsageCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(promoCode =>
                promoCode.PerCustomerUsageLimit);

        builder.Property(promoCode =>
                promoCode.StartsAtUtc)
            .HasPrecision(0)
            .IsRequired();

        builder.Property(promoCode =>
                promoCode.EndsAtUtc)
            .HasPrecision(0)
            .IsRequired();

        builder.Property(promoCode =>
                promoCode.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(promoCode =>
                promoCode.CreatedAtUtc)
            .HasPrecision(0)
            .IsRequired();

        builder.Property(promoCode =>
                promoCode.UpdatedAtUtc)
            .HasPrecision(0);

        builder.Property(promoCode =>
                promoCode.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(promoCode =>
                promoCode.Code)
            .IsUnique()
            .HasDatabaseName(
                "ux_promo_codes_code");

        builder.HasIndex(promoCode =>
                new
                {
                    promoCode.IsActive,
                    promoCode.StartsAtUtc,
                    promoCode.EndsAtUtc
                })
            .HasDatabaseName(
                "ix_promo_codes_active_schedule");
    }
}