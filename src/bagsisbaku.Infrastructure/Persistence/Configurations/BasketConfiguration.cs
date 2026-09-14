using bagsisbaku.Domain.Baskets;
using bagsisbaku.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

public sealed class BasketConfiguration
    : IEntityTypeConfiguration<Basket>
{
    public void Configure(
        EntityTypeBuilder<Basket> builder)
    {
        builder.ToTable(
            "baskets",
            "sales");

        builder.HasKey(basket => basket.Id);

        builder.Property(basket => basket.UserId)
            .IsRequired();

        builder.HasIndex(basket => basket.UserId)
            .IsUnique()
            .HasDatabaseName(
                "ux_baskets_user_id");

        builder.HasOne<AppUser>()
            .WithOne()
            .HasForeignKey<Basket>(
                basket => basket.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(basket => basket.Items)
            .WithOne()
            .HasForeignKey(item => item.BasketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(basket => basket.Items)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);
    }
}

public sealed class BasketItemConfiguration
    : IEntityTypeConfiguration<BasketItem>
{
    public void Configure(
        EntityTypeBuilder<BasketItem> builder)
    {
        builder.ToTable(
            "basket_items",
            "sales");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.BasketId)
            .IsRequired();

        builder.Property(item => item.ProductVariantId)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .IsRequired();

        builder.HasIndex(
                item => new
                {
                    item.BasketId,
                    item.ProductVariantId
                })
            .IsUnique()
            .HasDatabaseName(
                "ux_basket_items_basket_variant");

        builder.HasOne<
                bagsisbaku.Domain.Catalog.ProductVariant>()
            .WithMany()
            .HasForeignKey(item =>
                item.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}