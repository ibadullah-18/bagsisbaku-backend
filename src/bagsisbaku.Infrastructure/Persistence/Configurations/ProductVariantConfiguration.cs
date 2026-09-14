using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class ProductVariantConfiguration
    : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(
        EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable(
            "product_variants",
            "catalog");

        builder.ConfigureEntity();

        builder.Property(variant => variant.StockCount)
            .IsRequired();

        builder.Property(variant => variant.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(variant => variant.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(
                variant => new
                {
                    variant.ProductId,
                    variant.SizeId,
                    variant.ColorId
                })
            .IsUnique();

        builder.HasIndex(
            variant => new
            {
                variant.IsActive,
                variant.StockCount
            });

        builder.HasOne<Size>()
            .WithMany()
            .HasForeignKey(variant => variant.SizeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Color>()
            .WithMany()
            .HasForeignKey(variant => variant.ColorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
