using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration
    : IEntityTypeConfiguration<Product>
{
    public void Configure(
        EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(
            "products",
            "catalog");

        builder.ConfigureAuditable();

        builder.Property(product => product.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(product => product.Description)
            .HasMaxLength(4000);

        builder.Property(product => product.ProductCode)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(product => product.Model)
            .HasMaxLength(120);

        builder.Property(product => product.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(product => product.DiscountPrice)
            .HasPrecision(18, 2);

        builder.Ignore(product => product.IsDiscounted);

        builder.Property(product => product.ProductType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(product => product.IsFeatured)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(product => product.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(product => product.ViewCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasIndex(product => product.ProductCode)
            .IsUnique();

        builder.HasIndex(
            product => new
            {
                product.ProductType,
                product.IsActive
            });

        builder.HasIndex(
            product => new
            {
                product.CategoryId,
                product.IsActive
            });

        builder.HasIndex(
            product => new
            {
                product.BrandId,
                product.IsActive
            });

        builder.HasIndex(
            product => new
            {
                product.IsFeatured,
                product.IsActive
            });

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Brand>()
            .WithMany()
            .HasForeignKey(product => product.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(product => product.Images)
            .WithOne()
            .HasForeignKey(image => image.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(product => product.Images)
            .HasField("_images")
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);

        builder.HasMany(product => product.Variants)
            .WithOne()
            .HasForeignKey(variant => variant.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(product => product.Variants)
            .HasField("_variants")
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);
    }
}
