using bagsisbaku.Domain.Orders;
using bagsisbaku.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations.Orders;

internal sealed class OrderPromotionConfiguration
    : IEntityTypeConfiguration<Order>
{
    public void Configure(
        EntityTypeBuilder<Order> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(
                order => order.PromoCodeId)
            .IsRequired(false);

        builder.Property(
                order => order.PromoCode)
            .HasMaxLength(
                OrderPromotionSnapshot.MaximumCodeLength)
            .IsRequired(false);

        builder.Property(
                order => order.PromoDiscountAmount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m)
            .IsRequired();

        builder.HasIndex(
                order => order.PromoCodeId)
            .HasDatabaseName(
                "IX_orders_PromoCodeId");

        builder.HasOne<PromoCode>()
            .WithMany()
            .HasForeignKey(
                order => order.PromoCodeId)
            .OnDelete(
                DeleteBehavior.Restrict);
    }
}