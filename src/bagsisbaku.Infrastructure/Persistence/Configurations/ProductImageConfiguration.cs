using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class ProductImageConfiguration
    : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(
        EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable(
            "product_images",
            "catalog");

        builder.ConfigureEntity();

        builder.Property(image => image.ImageUrl)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(image => image.ImagePublicId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(image => image.SortOrder)
            .IsRequired();

        builder.Property(image => image.IsPrimary)
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasIndex(image => image.ImagePublicId)
            .IsUnique();

        builder.HasIndex(
                image => new
                {
                    image.ProductId,
                    image.SortOrder
                })
            .IsUnique();

        builder.HasIndex(
                image => new
                {
                    image.ProductId,
                    image.IsPrimary
                })
            .IsUnique()
            .HasFilter("[IsPrimary] = 1");
    }
}
